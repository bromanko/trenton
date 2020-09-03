namespace Trenton.Web.Server.Routes

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Giraffe.Razor
open Trenton.Health
open Trenton.Web.Server.ViewModels
open Trenton.Common.HttpUtils
open Trenton.Web.Server.Config
open System
open Trenton.Web.Server.Routes.Common

module Settings =
    module View =
        let private scopes =
            [ "activity"
              "heartrate"
              "location"
              "nutrition"
              "profile"
              "settings"
              "sleep"
              "social"
              "weight" ]

        let private getFitbitAuthUri cfg redirectUri =
            let scopesStr = String.concat " " scopes
            sprintf
                "https://www.fitbit.com/oauth2/authorize?response_type=code&client_id=%s&redirect_uri=%s&scope=%s&expires_in=2592000000"
                cfg.ClientId
                (redirectUri)
                (encodeUrlParam scopesStr)

        let private getRedirectUri (req: HttpRequest) =
            let uri =
                Uri
                    (sprintf
                        "%s://%s%s"
                         req.Scheme
                         (req.Host.ToString())
                         Paths.Fitbit.AuthCallback)

            uri.ToString()


        let private getModel cfg (fitbitSvc: FitbitService.T) (ctx: HttpContext) =
            async {
                let redirectUri = getRedirectUri ctx.Request
                let userId = getUserId ctx

                let! f = fitbitSvc.TryGetLastTokenUpdateDate userId

                return match f with
                       | None ->
                           Disconnected
                               { AuthUri =
                                     getFitbitAuthUri cfg.Fitbit redirectUri }
                       | Some lu -> Connected { LastUpdated = lu }
            }


        let handler cfg (fitbitSvc: FitbitService.T) =
            GET
            >=> route Paths.Settings.View
            >=> fun (next: HttpFunc) (ctx: HttpContext) ->
                    task {
                        let! model = getModel cfg fitbitSvc ctx
                        let model = Some { Fitbit = model }

                        return! razorHtmlView
                                    "Settings/Index"
                                    model
                                    None
                                    None
                                    next
                                    ctx
                    }
