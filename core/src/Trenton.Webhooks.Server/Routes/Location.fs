namespace Trenton.Webhooks.Server.Routes

open System
open FsToolkit.ErrorHandling
open Giraffe
open Trenton.Webhooks.Server
open Trenton.Webhooks.Server.Routes
open Trenton.Location.LocationService
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
                | _ ->
                    unauthorizedErr
                        Bearer
                        "Locations"
                        "Unauthorized"
                        earlyReturn
                        ctx

        let private timeNow () = DateTime.UtcNow

        let private fileName timeNow =
            (timeNow () |> DateTimeOffset).ToUnixTimeMilliseconds()
            |> sprintf "locations/%d.json"

        let private handleWebhook locSvc =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task {
                    let fName = fileName timeNow
                    let l = ctx.GetLogger()
                    let! response =
                        locSvc.StoreLocationData fName ctx.Request.Body
                        |> AsyncResult.foldResult (fun _ ->
                            Successful.OK {| Result = "ok" |})
                               (internalErrorEx l)

                    return! response next ctx
                }

        let handler<'a> accessToken locSvc =
            POST
            >=> route Paths.Location.Webhook
            >=> authenticate accessToken
            >=> handleWebhook locSvc
