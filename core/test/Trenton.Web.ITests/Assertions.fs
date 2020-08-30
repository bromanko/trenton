namespace Trenton.Web.ITests

open Expecto
open FSharpx.Control
open System.Net.Http

[<AutoOpen>]
module Assertions =
    /// Expects that an HttpResponseMessage has the specified HttpStatusCode
    let expectStatus code (response: HttpResponseMessage) =
        Expect.equal response.StatusCode code "Status code did not match"
        response

    /// Expects that an HttpResponseMessage has the specified Content-Type header
    let expectContentType contentType (response: HttpResponseMessage) =
        Expect.equal
            response.Content.Headers.ContentType.MediaType
            contentType
            "Content-Type header did not match"
        response

    module Async =
        let expectStatus code (request: Async<HttpResponseMessage>) =
            request
            |> Async.map (fun f -> expectStatus code f)

        let expectBodyEq expected (request: Async<HttpResponseMessage>) =
            request
            |> Async.bind readJson
            |> Async.map (fun actual ->
                Expect.equal actual expected "Response object did not match")
