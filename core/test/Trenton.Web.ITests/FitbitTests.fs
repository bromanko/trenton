namespace Trenton.Web.ITests

open Expecto
open FSharpx.Control
open System.Net

module FitbitTests =
    [<Literal>]
    let CallbackPath = "/fitbit/callback"

    let client = createClient ()

    [<Tests>]
    let tests =
        testList
            "Fitbit"
            [ testList
                "Auth Callback"
                  [ testAsync "Fails without code" {
                        return! get client CallbackPath
                                |> Async.expectStatus HttpStatusCode.BadRequest
                                |> Async.Ignore
                    }

                    testAsync "Fails with invalid code" {
                        return! get client (sprintf "%s?code=123" CallbackPath)
                                |> Async.expectStatus HttpStatusCode.BadRequest
                                |> Async.Ignore
                    } ] ]
