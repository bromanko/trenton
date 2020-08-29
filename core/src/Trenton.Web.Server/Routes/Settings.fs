namespace Trenton.Web.Server.Routes

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Giraffe.Razor
open Trenton.Web.Server.ViewModels
open Trenton.Common.HttpUtils
open Trenton.Web.Server.Config
open System

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


        let handler cfg =
            GET
            >=> route Paths.Settings.View
            >=> fun (next: HttpFunc) (ctx: HttpContext) ->
                    task {
                        let redirectUri = getRedirectUri ctx.Request

                        let model =
                            { FitbitAuthUri =
                                  getFitbitAuthUri cfg.Fitbit redirectUri }

                        return! razorHtmlView
                                    "Settings"
                                    (Some model)
                                    None
                                    None
                                    next
                                    ctx
                    }
