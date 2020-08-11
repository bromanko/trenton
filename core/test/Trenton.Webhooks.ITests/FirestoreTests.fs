namespace Trenton.Webhooks.ITests

open Expecto
open Google.Api.Gax
open Google.Cloud.Firestore
open System
open Trenton.Health.FitbitClient
open Trenton.Webhooks.Firestore.Fitbit

module FirestoreTests =
    let getNow () = DateTime.Now

    [<Tests>]
    let tests =
        ftestList "Firestore"
            [ testAsync "Stores access token" {
                  let token =
                      { AccessToken = "accessToken"
                        ExpiresInSeconds = 200
                        RefreshToken = "refreshToken" }

                  let tokenState =
                      { UserId = "userId" }

                  let beforeSave = getNow ()

                  let! db =
                      FirestoreDbBuilder(ProjectId = "trenton-local-dev", EmulatorDetection = EmulatorDetection.EmulatorOnly)
                          .BuildAsync() |> Async.AwaitTask
                  let! result = upsertAccessToken db getNow token tokenState

                  printfn "%O" result.UpdateTime
                  Expect.isGreaterThan (result.UpdateTime.ToDateTime())
                      beforeSave "UpdateTime was "
              } ]
