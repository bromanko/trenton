namespace Trenton.Health.Tests.Unit

open Expecto
open Trenton.Health.FitbitClient
open Trenton.Health.Tests.Unit.Config

module FitbitClientTests =
    let config = loadConfig()

    [<Tests>]
    let tests = testList "FitbitClient" [
        testList "getAccessToken" [
            testAsync "" {
                let client = getClient <| defaultConfig config.Fitbit.ClientId config.Fitbit.ClientSecret
                let req =
                    AuthorizationCodeWithPkce
                        { Code = "16387a444d3f06990b27d53072d6af5958b1cc5a"
                          RedirectUri = Some "http://localhost:5000/fitbit/callback"
                          State = None
                          CodeVerifier = None }

                let! tokenResp = client.GetAccessToken req
                Expect.isOk tokenResp "Request failed"
            }
        ]
    ]

