namespace Trenton.Webhooks.Server.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Trenton.Webhooks.Server.Config
open Trenton.Webhooks.Server
open Trenton.Webhooks.Server.Routes

module Fitbit =
    let bindQueryOrErr<'T> f =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let result = ctx.TryBindQueryString<'T>()
            match result with
            | Ok o -> f o next ctx
            | Result.Error r -> badRequestErr r earlyReturn ctx

    module VerifySubscriber =
        [<CLIMutable>]
        type VerifySubscriberQuery = { verify: string }

        let private verifySubscriber subscriberCfg query =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                match subscriberCfg.VerificationCode = query.verify with
                | true -> Successful.NO_CONTENT next ctx
                | false ->
                    RequestErrors.NOT_FOUND
                        "Verify code is incorrect"
                        earlyReturn
                        ctx

        let handler<'a> subscriberCfg =
            GET
            >=> route Paths.Fitbit.VerifySubscriber
            >=> bindQueryOrErr<VerifySubscriberQuery>
                    (verifySubscriber subscriberCfg)

    module Webhook =
        let private handleFitbitHook fitbitSvc =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task {
                    // use the fitbitservice to
                    return! Successful.NO_CONTENT next ctx
                }

        let handler<'a> fitbitSvc =
            POST
            >=> route Paths.Fitbit.Webhook
            >=> handleFitbitHook fitbitSvc
