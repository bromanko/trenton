namespace Trenton.Webhooks.Server.Routes

open Giraffe
open System.Threading.Tasks

[<AutoOpen>]
module Respond =
    let earlyReturn: HttpFunc = Some >> Task.FromResult
