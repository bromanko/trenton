namespace Trenton.Web.ITests

open System.Net
open Expecto
open FSharpx.Control

module DashboardTests =
    [<Literal>]
    let IndexPath = "/"

    let client = createClient ()

    let tests =
        testList
            "Dashboard"
            [ testList
                "Index"
                  [ testAsync "Returns information" {
                        return! get client IndexPath
                                |> Async.expectStatus HttpStatusCode.OK
                                |> Async.Ignore
                    } ] ]
