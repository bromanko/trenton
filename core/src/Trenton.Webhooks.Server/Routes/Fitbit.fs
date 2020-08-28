namespace Trenton.Webhooks.Server.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Trenton.Health.FitbitClient
open Trenton.Webhooks.Server.Config
open Trenton.Webhooks.Server

module Fitbit =
    let private earlyReturn: HttpFunc = Some >> Task.FromResult

    let bindQueryOrErr<'T> f =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let result = ctx.TryBindQueryString<'T>()
            match result with
            | Ok o -> f o next ctx
            | Result.Error r -> badRequestErr r earlyReturn ctx

    module AuthCallback =
        [<CLIMutable>]
        type AuthCallbackQuery =
            { code: string }

        let private getRedirectUri serverBaseUrl (ctx: HttpContext) =
            sprintf "%s%s" serverBaseUrl (ctx.Request.Path.ToString())

        let private respondErr =
            function
            | FitbitApiError.Error e -> badRequestErr e earlyReturn
            | Exception e -> internalError e.Message earlyReturn

        let private getAccessToken fitbitClient serverBaseUrl query: HttpHandler =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                let req =
                    AuthorizationCodeWithPkce
                        { Code = query.code
                          RedirectUri = getRedirectUri serverBaseUrl ctx |> Some
                          State = None
                          CodeVerifier = None }
                task {
                    let! tokenRes = fitbitClient.GetAccessToken req
                    return! match tokenRes with
                            | Ok token -> json token next ctx
                            | Result.Error err -> respondErr err ctx
                }

        let handler<'a> fitbitClient serverBaseUrl =
            GET >=> route "/fitbit/callback"
            >=> bindQueryOrErr<AuthCallbackQuery>
                    (getAccessToken fitbitClient serverBaseUrl)
            >=> Successful.NO_CONTENT

    module VerifySubscriber =
        [<CLIMutable>]
        type VerifySubscriberQuery =
            { verify: string }

        let private verifySubscriber subscriberCfg query =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                match subscriberCfg.VerificationCode = query.verify with
                | true -> Successful.NO_CONTENT next ctx
                | false ->
                    RequestErrors.NOT_FOUND "Verify code is incorrect" earlyReturn
                        ctx

        let handler<'a> subscriberCfg =
            GET >=> route "/fitbit"
            >=> bindQueryOrErr<VerifySubscriberQuery> (verifySubscriber subscriberCfg)

    module Webhook =
        let private publishEvent =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task {
                    // take the body and enqueue it to GC pubsub
                    return! Successful.NO_CONTENT next ctx }

        let handler<'a> =
            POST >=> route "/fitbit" >=> publishEvent
