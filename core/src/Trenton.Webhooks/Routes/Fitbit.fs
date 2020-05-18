namespace Trenton.Webhooks.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Trenton.Health.FitbitClient

module Fitbit =
    [<CLIMutable>]
    type AuthCallbackQuery =
        { code: string }

    let bindQueryOrErr<'T> f =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let result = ctx.TryBindQueryString<'T>()
            match result with
            | Ok o -> f o next ctx
            | Result.Error r -> (RequestErrors.BAD_REQUEST r) next ctx

    let private getAccessToken fitbitClient query: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let req =
                    AuthorizationCodeWithPkce
                        { Code = query.code
                          RedirectUri = Some ""
                          State = None
                          CodeVerifier = None }
                let! tokenRes = fitbitClient.GetAccessToken req
                return! match tokenRes with
                        | Ok token -> json token next ctx
                        | Result.Error err ->
                            let msg =
                                match err with
                                | Error e -> e
                                | Exception e -> e.Message
                            ServerErrors.INTERNAL_ERROR msg next ctx
            }

    let authCallbackHandler<'a> fitbitClient =
        GET >=> route "/fitbit/callback"
        >=> bindQueryOrErr<AuthCallbackQuery> (getAccessToken fitbitClient)
        >=> Successful.NO_CONTENT
