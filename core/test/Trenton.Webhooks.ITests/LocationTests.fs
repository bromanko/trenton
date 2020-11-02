namespace Trenton.Webhooks.ITests

open Expecto
open Trenton.Webhooks.ITests.Client
open Trenton.Webhooks.ITests.Assertions
open Trenton.Webhooks.ITests.Config
open System.Net.Http
open System.Net.Http.Headers
open System.Net
open FSharpx.Control

module LocationTests =
    [<Literal>]
    let WebhookPath = "/locations"

    [<CLIMutable>]
    type IndexResponse = { now: string }

    let client = createClient ()

    let postAuthenticated (client: HttpClient) (path: string) token content =
        let req = createRequest HttpMethod.Post path
        req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", token)
        req.Content <- content
        makeRequest client req

    let locationsContent () = {| locations = []  |} |> jsonContent

    [<Tests>]
    let tests =
        testList
            "Locations"
            [ testList
                "Webhook"
                  [ testAsync "Returns unauthorized without header" {
                        return! postJson client WebhookPath (locationsContent())
                                |> Async.expectStatus
                                    HttpStatusCode.Unauthorized
                                |> Async.Ignore
                    }
                    testAsync "Returns unauthorized with invalid header" {
                        return! postAuthenticated
                                    client
                                    WebhookPath
                                    "invalid"
                                    (locationsContent())
                                |> Async.expectStatus
                                    HttpStatusCode.Unauthorized
                                |> Async.Ignore
//                    }
//                    testAsync "Succeeds with valid header" {
//                        let config = loadConfig ()
//                        return! postAuthenticated
//                                    client
//                                    WebhookPath
//                                    config.Location.AccessToken
//                                    (locationsContent())
//                                |> Async.expectStatus HttpStatusCode.OK
//                                |> Async.Ignore
                    } ] ]
