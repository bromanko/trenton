namespace Trenton.Health

open Google.Cloud.Firestore
open System
open Trenton.Health.FitbitClient

module FitbitAuthRepository =
    [<Literal>]
    let private AccessTokenCollectionName = "fitbit-access-tokens"

    [<FirestoreData; CLIMutable>]
    type private AccessToken =
        { [<FirestoreProperty>]
          AccessToken: string
          [<FirestoreProperty>]
          ExpiresInSeconds: int32
          [<FirestoreProperty>]
          RefreshToken: string
          [<FirestoreProperty>]
          CreatedAt: DateTime }

    type FitbitAuthRepositoryError = Exception of exn

    [<RequireQualifiedAccess>]
    type T =
        { tryGetAccessToken: string -> Async<AccessTokenDto option>
          upsertAccessToken: AccessTokenDto -> ActiveTokenStateDto -> Async<Result<unit, FitbitAuthRepositoryError>> }

    let private parseAccessToken (token: AccessTokenDto) (now: DateTime) =
        { AccessToken.AccessToken = token.AccessToken
          ExpiresInSeconds = token.ExpiresInSeconds
          RefreshToken = token.RefreshToken
          CreatedAt = now.ToUniversalTime() }

    let private toAccessTokenDto (token: AccessToken) =
        { AccessTokenDto.AccessToken = token.AccessToken
          ExpiresInSeconds = token.ExpiresInSeconds
          RefreshToken = token.RefreshToken }

    let private upsertAccessToken (dbBuilder: FirestoreDbBuilder)
                                  getNow
                                  token
                                  tokenState
                                  =
        async {
            let! db = dbBuilder.BuildAsync() |> Async.AwaitTask

            let docRef =
                db.Collection(AccessTokenCollectionName)
                  .Document(tokenState.UserId)

            try
                do! getNow ()
                    |> parseAccessToken token
                    |> docRef.SetAsync
                    |> Async.AwaitTask
                    |> Async.Ignore

                return Ok()
            with e ->
                return Result.Error
                       <| FitbitAuthRepositoryError.Exception e
        }


    let private tryGetAccessToken (dbBuilder: FirestoreDbBuilder) userId =
        async {
            let! db = dbBuilder.BuildAsync() |> Async.AwaitTask

            let docRef =
                db.Collection(AccessTokenCollectionName).Document(userId)

            match! docRef.GetSnapshotAsync() |> Async.AwaitTask with
            | s when s.Exists ->
                return s.ConvertTo<AccessToken>()
                       |> toAccessTokenDto
                       |> Some
            | _ -> return None
        }

    let firestoreAuthRepository dbBuilder getNow =
        { T.tryGetAccessToken = tryGetAccessToken dbBuilder
          T.upsertAccessToken = upsertAccessToken dbBuilder getNow }
