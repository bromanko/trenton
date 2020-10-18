namespace Trenton.Webhooks.ITests

open FSharpx.Control
open System.Net.Http
open System.Text

open System.Text.Json
open Trenton.Webhooks.ITests.Config

[<AutoOpen>]
module Client =
    /// Create a web client for the currently configured server
    let createClient () =
        let config = loadConfig ()
        new HttpClient(BaseAddress = config.Server.AddressUri)

    /// Create an HTTP request
    let createRequest (method: HttpMethod) (path: string) =
        new HttpRequestMessage(method, path)

    /// Issues an HTTP request
    let makeRequest (client: HttpClient) request =
        client.SendAsync request |> Async.AwaitTask

    /// Issues an HTTP GET request
    let get (client: HttpClient) (path: string) =
        client.GetAsync path |> Async.AwaitTask

    /// Issues an HTTP POST request
    let post (client: HttpClient) (path: string) content =
        let req = createRequest HttpMethod.Post path
        req.Content <- content
        makeRequest client req

    /// Creates a Json serialized string content
    let jsonContent content =
        new StringContent(JsonSerializer.Serialize content,
                          Encoding.UTF8,
                          "application/json")

    /// Issues an HTTP POST request serialising the body as Json
    let inline postJson<'Content> (client: HttpClient) path content =
        jsonContent content |> post client path

    /// Reads an HTTP response as a string
    let readText (response: HttpResponseMessage) =
        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask

    /// Reads a Json HTTP response and deserializes it into an object
    let readJson<'T> (response: HttpResponseMessage) =
        readText response
        |> Async.map (fun text ->
            try
                JsonSerializer.Deserialize<'T>(text, null)
            with e ->
                JsonException
                    (sprintf
                        "Could not convert JSON value of %s to %O"
                         text
                         (typeof<'T>),
                     e)
                |> raise)
