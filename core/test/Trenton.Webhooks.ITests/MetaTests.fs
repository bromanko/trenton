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


    [<Tests>]
    let tests =
        testList "Index"
            [ testAsync "Returns information" {
                  let client = createClient ()

                  get client IndexPath
                  |> Async.expectStatus HttpStatusCode.OK
                  |> Async.bind readJson
                  |> Async.map
                      (fun actual ->
                          Expect.isNotEmpty actual.now "Response is empty")
                  |> Async.RunSynchronously
                  |> ignore
              } ]
