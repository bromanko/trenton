namespace Trenton.Health.Tests.Unit

open Config
open Expecto
open Google.Api.Gax
open Google.Cloud.Firestore
open Grpc.Core
open System
open Trenton.Health
open Trenton.Health.FitbitClient
open Trenton.Iam

module FitbitAuthRepositoryTests =
    let getNow () = DateTime.Now

    let fakeAccessToken () =
        { AccessToken = Guid.NewGuid().ToString()
          ExpiresInSeconds = 200
          RefreshToken = Guid.NewGuid().ToString() }

    let fakeUserId () =
        (UserId.create <| Guid.NewGuid().ToString()).Value

    let firestoreRepo () =
        let config = loadConfig ()

        let builder =
            FirestoreDbBuilder
                (ProjectId = config.Firestore.Project,
                 Endpoint = config.Firestore.Host,
                 ChannelCredentials = ChannelCredentials.Insecure)

        FitbitAuthRepository.firestoreAuthRepository builder getNow

    [<Tests>]
    let tests =
        testList
            "FitbitAuthRepository for Firestore"
            [ testAsync "Stores access token" {
                  let token = fakeAccessToken ()
                  let userId = fakeUserId ()
                  let repo = firestoreRepo ()

                  let! result = repo.UpsertAccessToken userId token
                  Expect.isOk result "Result was not Ok"
              }
              testAsync "Retrieves access token" {
                  let token = fakeAccessToken ()
                  let userId = fakeUserId ()
                  let repo = firestoreRepo ()

                  do! repo.UpsertAccessToken userId token
                      |> Async.Ignore
                  match! repo.TryGetAccessToken userId with
                  | None -> failtestf "Should have retrieved the access token"
                  | Some a ->
                      Expect.equal
                          a.AccessToken
                          token.AccessToken
                          "Access token loaded did not match"
                      Expect.equal
                          a.RefreshToken
                          token.RefreshToken
                          "Refresh token loaded did not match"
              } ]
