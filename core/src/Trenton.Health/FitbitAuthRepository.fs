namespace Trenton.Health

open FsToolkit.ErrorHandling
open Google.Cloud.Firestore
open System
open Trenton.Health.FitbitClient
open Trenton.Iam

module FitbitAuthRepository =
    [<Literal>]
    let private AccessTokenCollectionName = "fitbit-access-tokens"

    [<FirestoreData; CLIMutable>]
    type PersistedAccessTokenDto =
        { [<FirestoreProperty>]
          AccessToken: string
          [<FirestoreProperty>]
          ExpiresInSeconds: int32
          [<FirestoreProperty>]
          RefreshToken: string
          [<FirestoreProperty>]
          CreatedAt: DateTime
          [<FirestoreProperty>]
          UpdatedAt: DateTime }

    type FitbitAuthRepositoryError = Exception of exn

    [<RequireQualifiedAccess>]
    type T =
        { TryGetAccessToken: UserId.T -> Async<PersistedAccessTokenDto option>
          UpsertAccessToken: UserId.T -> AccessTokenDto -> Async<Result<unit, FitbitAuthRepositoryError>> }

    let private parseAccessToken (token: AccessTokenDto)
                                 (createdAt: DateTime)
                                 (updatedAt: DateTime)
                                 =
        { AccessToken = token.AccessToken
          ExpiresInSeconds = token.ExpiresInSeconds
          RefreshToken = token.RefreshToken
          CreatedAt = createdAt.ToUniversalTime()
          UpdatedAt = updatedAt.ToUniversalTime() }

    let private parseFitbitClientAccessToken (token: PersistedAccessTokenDto) =
        { AccessToken = token.AccessToken
          RefreshToken = token.RefreshToken
          ExpiresInSeconds = token.ExpiresInSeconds }

    let private accessTokenDocRef (db: FirestoreDb) userId =
        UserId.value userId
        |> db.Collection(AccessTokenCollectionName).Document

    let tryGetDoc (docRef: DocumentReference) =
        async {
            let! s = docRef.GetSnapshotAsync() |> Async.AwaitTask

            return match s.Exists with
                   | false -> None
                   | true -> Some s
        }

    let private upsertAccessToken (dbBuilder: FirestoreDbBuilder)
                                  getNow
                                  userId
                                  token
                                  =
        asyncResult {
            let! db = dbBuilder.BuildAsync() |> Async.AwaitTask
            let docRef = accessTokenDocRef db userId

            let! atDoc = docRef.GetSnapshotAsync() |> Async.AwaitTask

            let updatedAt = getNow ()

            let createdAt =
                if atDoc.Exists
                then atDoc.ConvertTo<PersistedAccessTokenDto>().CreatedAt
                else updatedAt

            try
                do! parseAccessToken token createdAt updatedAt
                    |> docRef.SetAsync
                    |> Async.AwaitTask
                    |> Async.Ignore

                return! Ok()
            with e ->
                return! Result.Error
                        <| FitbitAuthRepositoryError.Exception e
        }


    let private tryGetAccessToken (dbBuilder: FirestoreDbBuilder) userId =
        asyncOption {
            let! db =
                dbBuilder.BuildAsync()
                |> Async.AwaitTask
                |> Async.map Some

            let! doc = accessTokenDocRef db userId |> tryGetDoc

            return doc.ConvertTo<PersistedAccessTokenDto>()
        }


    let firestoreAuthRepository dbBuilder getNow =
        { T.TryGetAccessToken = tryGetAccessToken dbBuilder
          T.UpsertAccessToken = upsertAccessToken dbBuilder getNow }
