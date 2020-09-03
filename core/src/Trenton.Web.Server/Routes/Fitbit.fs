namespace Trenton.Web.Server.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Giraffe
open Trenton.Health.FitbitService
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
            | FitbitApiError e -> badRequestErr e earlyReturn
            | Exception e -> internalError e.Message earlyReturn

        let private getAndStoreAccessToken fitbitSvc query: HttpHandler =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task {
                    let! res =
                        fitbitSvc.GetAndStoreAccessToken
                            (getUserId ctx)
                            query.code
                            (getRedirectUri ctx.Request)

                    return! match res with
                            | Ok _ -> next ctx
                            | Result.Error err -> respondErr err ctx
                }

        let handler fitbitSvc =
            GET
            >=> route path
            >=> bindQueryOrErr<Query> (getAndStoreAccessToken fitbitSvc)
            >=> redirectTo false Paths.Settings.View
