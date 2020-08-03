namespace Trenton.Webhooks.ITests

open Expecto
open FSharpx.Control
open System.Net

module MetaTests =
    [<Literal>]
    let IndexPath = "/"

    [<CLIMutable>]
    type IndexResponse =
        { now: string }

    [<Literal>]
    let HealthPath = "/healthz"

    [<CLIMutable>]
    type HealthSuccessResponse =
        { status: string }

    let client = createClient ()

    [<Tests>]
    let tests =
        testList "Meta"
            [ testList "Index"
                  [ testAsync "Returns information" {
                        return! get client IndexPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map
                                    (fun actual ->
                                        Expect.isNotEmpty actual.now
                                            "Response is empty")
                    } ]

              testList "Health Checks"
                  [ testAsync "Returns information" {
                        return! get client HealthPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map
                                    (fun actual ->
                                        Expect.equal actual.status "Healthy"
                                            "Status was not healthy")
                    } ] ]
