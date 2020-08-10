namespace Trenton.Webhooks

open Google.Cloud.Firestore
open System
open Trenton.Health.FitbitClient

module Firestore =
    module Fitbit =
        [<FirestoreData; CLIMutable>]
        type AccessToken =
            { [<FirestoreProperty>]
              AccessToken: string
              [<FirestoreProperty>]
              ExpiresAt: DateTime
              [<FirestoreProperty>]
              RefreshToken: string }

        let private parseAccessToken (token: AccessTokenDto) (now: DateTime) =
            { AccessToken.AccessToken = token.AccessToken
              ExpiresAt = now.AddSeconds <| (float token.ExpiresInSeconds)
              RefreshToken = token.RefreshToken }

        let upsertAccessToken (db: FirestoreDb) getNow token tokenState =
            let docRef =
                db.Collection("fitbit-access-tokens")
                  .Document(tokenState.UserId)
            getNow ()
            |> parseAccessToken token
            |> docRef.SetAsync
