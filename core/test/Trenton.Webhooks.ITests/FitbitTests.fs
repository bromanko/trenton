namespace Trenton.Webhooks.ITests

open Expecto
open FSharpx.Control
open System.Net
open Trenton.Webhooks.ITests.Config

module FitbitTests =
    [<Literal>]
    let CallbackPath = "/fitbit/callback"

    [<Literal>]
    let VerifySubscriberPath = "/fitbit"

    [<Literal>]
    let WebhookPath = "/fitbit"

    [<CLIMutable>]
    type AuthSuccessResponse =
        { accessToken: string }

    [<CLIMutable>]
    type AuthErrorResponse =
        { message: string }

    let client = createClient ()

    let expectErrMsg expected =
        fun actual ->
            Expect.isMatch actual.message expected "Error message is incorrect"

    [<Tests>]
    let tests =
        testList "Fitbit"
            [ testList "Auth Callback"
                  [ testAsync "Fails without code" {
                        return! get client CallbackPath
                                |> Async.expectStatus HttpStatusCode.BadRequest
                                |> Async.bind readJson
                                |> Async.map
                                    (expectErrMsg
                                        "Missing value for required property code.")
                    }

                    testAsync "Fails with invalid code" {
                        return! get client
                                    (sprintf "%s?code=123" CallbackPath)
                                |> Async.expectStatus HttpStatusCode.BadRequest
                                |> Async.bind readJson
                                |> Async.map
                                    (expectErrMsg "Authorization code invalid")
                    } ]
              testList "Subscriber Verify"
                  [ testAsync "Fails without code" {
                        return! get client VerifySubscriberPath
                                |> Async.expectStatus HttpStatusCode.BadRequest
                                |> Async.bind readJson
                                |> Async.map
                                    (expectErrMsg
                                        "Missing value for required property verify.")
                    }

                    testAsync "Returns 404 for invalid code" {
                        return! get client
                                    (sprintf "%s?verify=123"
                                         VerifySubscriberPath)
                                |> Async.expectStatus HttpStatusCode.NotFound
                                |> Async.Ignore
                    }

                    testAsync "Returns 204 for valid code" {
                        let cfg = loadConfig ()
                        return! get client
                                    (sprintf "%s?verify=%s"
                                         VerifySubscriberPath
                                         cfg.Fitbit.SubscriberVerificationCode)
                                |> Async.expectStatus HttpStatusCode.NoContent
                                |> Async.Ignore
                    } ] ]
