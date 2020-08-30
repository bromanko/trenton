namespace Trenton.Web.ITests

open System.Net
open Expecto
open FSharpx.Control

module MetaTests =
    [<Literal>]
    let LivenessPath = "/healthz/liveness"

    [<Literal>]
    let ReadinessPath = "/healthz/readiness"

    [<CLIMutable>]
    type LivenessSuccessResponse = { status: int }

    [<CLIMutable>]
    type ReadinessSuccessResponse = { status: int }

    let client = createClient ()

    [<Tests>]
    let tests =
        testList
            "Meta"
            [ testList
                "Health Checks"
                  [ testAsync "Liveness returns information" {
                        return! get client LivenessPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map (fun actual ->
                                    Expect.equal
                                        actual.status
                                        2 // Healthy
                                        "Status was not healthy")
                    }
                    testAsync "Readiness returns information" {
                        return! get client ReadinessPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.bind readJson
                                |> Async.map (fun actual ->
                                    Expect.equal
                                        actual.status
                                        2 // Healthy
                                        "Status was not healthy")
                    } ] ]
