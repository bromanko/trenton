namespace Trenton.Webhooks

open Google.Cloud.Firestore
open System
open Trenton.Health.FitbitClient

module Firestore =
    module Fitbit =
        [<Literal>]
        let AccessTokenCollectionName = "fitbit-access-tokens"

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
              ExpiresAt =
                  now.ToUniversalTime().AddSeconds
                  <| (float token.ExpiresInSeconds)
              RefreshToken = token.RefreshToken }

        let upsertAccessToken (db: FirestoreDb) getNow token tokenState =
            let docRef =
                db.Collection(AccessTokenCollectionName)
                  .Document(tokenState.UserId)
            getNow ()
            |> parseAccessToken token
            |> docRef.SetAsync
            |> Async.AwaitTask

        let tryGetAccessToken (db: FirestoreDb) userId =
            async {
                let docRef =
                    db.Collection(AccessTokenCollectionName).Document(userId)
                match! docRef.GetSnapshotAsync() |> Async.AwaitTask with
                | s when s.Exists -> return s.ConvertTo<AccessToken>() |> Some
                | _ -> return None
            }
