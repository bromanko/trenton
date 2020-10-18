namespace Trenton.Webhooks.Server.Routes

open Giraffe
open Trenton.Webhooks.Server
open Trenton.Webhooks.Server.Routes
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive

module Location =
    [<Literal>]
    let Bearer = "Bearer"

    let private expectedAuthHeader preSharedToken =
        sprintf "%s %s" Bearer preSharedToken

    module Webhook =
        let private authenticate preSharedToken =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                match ctx.GetRequestHeader "Authorization" with
                | Ok o when o = expectedAuthHeader preSharedToken -> next ctx
                | _ -> unauthorizedErr Bearer "Locations" "Unauthorized" earlyReturn ctx

        let private handleWebhook fitbitSvc =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task { return! Successful.OK {| Result = "ok" |} next ctx }

        let handler<'a> accessToken locSvc =
            POST
            >=> route Paths.Location.Webhook
            >=> authenticate accessToken
            >=> handleWebhook locSvc
