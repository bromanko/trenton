namespace Trenton.Web.Server.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Giraffe
open Trenton.Health.FitbitClient
open Trenton.Web.Server.ErrorHandler
open Trenton.Web.Server.Routes.Common
open System

module Fitbit =
    module AuthCallback =
        [<Literal>]
        let path = "/fitbit/callback"

        [<CLIMutable>]
        type Query = { code: string }

        let private getRedirectUri (req: HttpRequest) =
            let uri =
                Uri
                    (sprintf
                        "%s://%s%s"
                         req.Scheme
                         (req.Host.ToString())
                         (req.Path.ToString()))

            uri.ToString()

        let private respondErr =
            function
            | FitbitApiError.Error e -> badRequestErr e earlyReturn
            | Exception e -> internalError e.Message earlyReturn

        let private getAccessToken fitbitClient query: HttpHandler =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                let req =
                    AuthorizationCodeWithPkce
                        { Code = query.code
                          RedirectUri = getRedirectUri ctx.Request |> Some
                          State = None
                          CodeVerifier = None }

                task {
                    let! tokenRes = fitbitClient.GetAccessToken req

                    return! match tokenRes with
                            | Ok token -> json token next ctx
                            | Result.Error err -> respondErr err ctx
                }

        let handler<'a> fitbitClient =
            GET
            >=> route path
            >=> bindQueryOrErr<Query> (getAccessToken fitbitClient)
            >=> redirectTo false Paths.Settings.View
