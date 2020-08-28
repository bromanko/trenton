namespace Trenton.Webhooks.Server.Routes

open Giraffe
open System

module Index =
    let handler<'a> =
        GET >=> route "/"
        >=> warbler (fun _ -> json {| now = DateTime.UtcNow |})
