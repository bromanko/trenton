namespace Trenton.Webhooks.ITests

open Config
open Expecto
open Google.Api.Gax
open Google.Cloud.Firestore
open Grpc.Core
open System
open Trenton.Health.FitbitClient
open Trenton.Webhooks.Server.Firestore.Fitbit

module FirestoreTests =
    let getNow () = DateTime.Now

    let fakeAccessToken () =
        { AccessToken = Guid.NewGuid().ToString()
          ExpiresInSeconds = 200
          RefreshToken = Guid.NewGuid().ToString() }

    let fakeAccessTokenState () =
        { UserId = Guid.NewGuid().ToString() }

    let buildDb () =
        let config = loadConfig ()
        let builder =
            FirestoreDbBuilder
                (ProjectId = config.Firestore.Project,
                 Endpoint = config.Firestore.Host,
                 ChannelCredentials = ChannelCredentials.Insecure)
        builder.BuildAsync() |> Async.AwaitTask

    [<Tests>]
    let tests =
        testList "Firestore"
            [ testAsync "Stores access token" {
                  let token = fakeAccessToken()
                  let tokenState = fakeAccessTokenState()
                  let beforeSave = getNow ()
                  let! db = buildDb ()

                  let! result = upsertAccessToken db getNow token tokenState

                  Expect.isGreaterThan (result.UpdateTime.ToDateTime())
                      beforeSave
                      "UpdateTime was not set to after the save operation"
              }
              testAsync "Retrieves access token" {
                  let token = fakeAccessToken()
                  let tokenState = fakeAccessTokenState()
                  let! db = buildDb ()

                  do! upsertAccessToken db getNow token tokenState |> Async.Ignore
                  match! tryGetAccessToken db tokenState.UserId with
                  | None -> failtestf "Should have retrieved the access token"
                  | Some a ->
                      Expect.equal a.AccessToken token.AccessToken "Access token loaded did not match"
                      Expect.equal a.RefreshToken token.RefreshToken "Refresh token loaded did not match"
              } ]
