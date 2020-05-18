namespace Trenton.Webhooks.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Trenton.Health.FitbitClient

module Fitbit =
    [<CLIMutable>]
    type AuthCallbackQuery =
        { Code: string }

    let private getAccessToken fitbitClient query =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! tokenRes = fitbitClient.GetAccessToken query.Code
                return!
                    match tokenRes with
                    | Ok token -> json token next ctx
                    | Error err -> ServerErrors.INTERNAL_ERROR err next ctx
            }

    let authCallbackHandler<'a> fitbitClient =
        GET >=> route "/fitbit/callback"
        >=> bindQuery<AuthCallbackQuery> None (getAccessToken fitbitClient)
        >=> Successful.NO_CONTENT
