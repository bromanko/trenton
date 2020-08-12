namespace Trenton.Webhooks.ITests

open Config
open Expecto
open Google.Api.Gax
open Google.Cloud.Firestore
open System
open Grpc.Core
open Trenton.Health.FitbitClient
open Trenton.Webhooks.Firestore.Fitbit

module FirestoreTests =
    let getNow () = DateTime.Now

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
                  let token =
                      { AccessToken = "accessToken"
                        ExpiresInSeconds = 200
                        RefreshToken = "refreshToken" }

                  let tokenState =
                      { UserId = "userId" }

                  let beforeSave = getNow ()

                  let! db = buildDb ()
                  let! result = upsertAccessToken db getNow token tokenState

                  Expect.isGreaterThan (result.UpdateTime.ToDateTime())
                      beforeSave
                      "UpdateTime was not set to after the save operation"
              } ]
