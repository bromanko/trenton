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

    let private getRedirectUri serverBaseUrl (ctx: HttpContext) =
        sprintf "%s%s" serverBaseUrl (ctx.Request.Path.ToString())

    let private getAccessToken fitbitClient serverBaseUrl query: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let req =
                    AuthorizationCodeWithPkce
                        { Code = query.code
                          RedirectUri =
                              getRedirectUri serverBaseUrl ctx |> Some
                          State = None
                          CodeVerifier = None }
                let! tokenRes = fitbitClient.GetAccessToken req
                return! match tokenRes with
                        | Ok token -> json token next ctx
                        | Result.Error err ->
                            (match err with
                             | Error e -> RequestErrors.BAD_REQUEST e
                             | Exception e ->
                                 ServerErrors.INTERNAL_ERROR e.Message) next
                                ctx
            }

    let authCallbackHandler<'a> fitbitClient serverBaseUrl =
        GET >=> route "/fitbit/callback"
        >=> bindQueryOrErr<AuthCallbackQuery>
                (getAccessToken fitbitClient serverBaseUrl)
        >=> Successful.NO_CONTENT
