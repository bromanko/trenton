namespace Trenton.Cli.Verbs

open Trenton.Common
open Trenton.Cli
open FsToolkit.ErrorHandling.Operator.Result
open System.Diagnostics
open System.Runtime.InteropServices

module AuthFitbit =
    [<Literal>]
    let RedirectUri = "http://localhost:9032/fitbit/callback"

    let Scopes =
        [| "activity"
           "heartrate"
           "location"
           "nutrition"
           "profile"
           "settings"
           "sleep"
           "social"
           "weight" |]

    type private AuthConfig = { ClientId: NonEmptyString.T }

    let private mkConfig clientId = { ClientId = clientId }

    let private parseCfg (cfg: AppConfig) =
        mkConfig
        <!> (parseNes "ClientId must be specified in app configuration file"
             <| cfg.Fitbit.ClientId)

    let private browser url =
        if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
            ProcessStartInfo("cmd", sprintf "/c start %s" url)
        elif RuntimeInformation.IsOSPlatform OSPlatform.OSX then
            ProcessStartInfo("open", url)
        else
            ProcessStartInfo("xdg-open", url)

    let private launchUrl cfg =
        let url =
            sprintf
                "https://www.fitbit.com/oauth2/authorize?response_type=code&client_id=%s&redirect_uri=%s&scope=%s&expires_in=2592000000"
                (NonEmptyString.value cfg.ClientId)
                RedirectUri
                (String.concat "%20" Scopes)

        try
            browser url |> Process.Start |> ignore

            Ok()
        with ex -> ExecError.Exception ex |> Result.Error

    let exec cfg = parseCfg cfg >>= launchUrl
