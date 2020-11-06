namespace Trenton.Webhooks.ITests

open Expecto
open FSharpx.Control
open System.Net

module MetaTests =
    [<Literal>]
    let IndexPath = "/"

    [<CLIMutable>]
    type IndexResponse = { now: string }

    [<Literal>]
    let LivenessPath = "/healthz/liveness"

    [<Literal>]
    let ReadinessPath = "/healthz/readiness"

    [<CLIMutable>]
    type HealthSuccessResponse = { status: string }

    let client = createClient ()

    [<Tests>]
    let tests =
        testList
            "Meta"
            [ testList
                "Index"
                  [ testAsync "Returns information" {
                        return! get client IndexPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map (fun actual ->
                                    Expect.isNotEmpty
                                        actual.now
                                        "Response is empty")
                    } ]

              testList
                  "Health Checks"
                  [ testAsync "Liveness returns information" {
                        return! get client LivenessPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map (fun actual ->
                                    Expect.equal
                                        actual.status
                                        "Healthy"
                                        "Status was not healthy")
                    }
                    testAsync "Readiness returns information" {
                        return! get client ReadinessPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map (fun actual ->
                                    Expect.equal
                                        actual.status
                                        "Healthy"
                                        "Status was not healthy")
                    } ] ]
