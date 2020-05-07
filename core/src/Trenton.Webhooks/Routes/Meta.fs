namespace Trenton.Webhooks.Routes

open System
open Giraffe

module Meta =
    let index () = json {| now = DateTime.UtcNow |}
