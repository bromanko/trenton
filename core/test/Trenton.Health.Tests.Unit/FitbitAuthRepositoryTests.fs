namespace Trenton.Health.Tests.Unit

open Config
open Expecto
open Google.Api.Gax
open Google.Cloud.Firestore
open Grpc.Core
open System
open Trenton.Health
open Trenton.Health.FitbitClient

module FitbitAuthRepositoryTests =
    let getNow () = DateTime.Now

    let fakeAccessToken () =
        { AccessToken = Guid.NewGuid().ToString()
          ExpiresInSeconds = 200
          RefreshToken = Guid.NewGuid().ToString() }

    let fakeAccessTokenState () = { UserId = Guid.NewGuid().ToString() }

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
                  let tokenState = fakeAccessTokenState ()
                  let repo = firestoreRepo ()

                  let! result = repo.UpsertAccessToken token tokenState
                  Expect.isOk result "Result was not Ok"
              }
              testAsync "Retrieves access token" {
                  let token = fakeAccessToken ()
                  let tokenState = fakeAccessTokenState ()
                  let repo = firestoreRepo ()

                  do! repo.UpsertAccessToken token tokenState
                      |> Async.Ignore
                  match! repo.TryGetAccessToken tokenState.UserId with
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
