namespace Trenton.Cli.Verbs.Auth.Fitbit

open Argu
open System.Diagnostics
open System.Runtime.InteropServices
open System.Threading
open Microsoft.Extensions.Logging
open Trenton.Common
open Microsoft.Extensions.Hosting
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Cli.Verbs.Auth.Fitbit
open Trenton.Cli.Verbs.Auth.Fitbit.Host
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module Execution =
    type private ParsedArgs =
        { AppConfigPath: NonEmptyString.T
          ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          ServerPort: int
          ServerLogLevel: Microsoft.Extensions.Logging.LogLevel }

    module private Parsing =
        let parseLogLevel (args: ParseResults<_>) =
            match (GlobalConfig.ParseArgs args).Debug with
            | true -> Microsoft.Extensions.Logging.LogLevel.Debug
            | _ -> LogLevel.None

        let parseAppCfgPath (args: ParseResults<_>) =
            (GlobalConfig.ParseArgs args).ConfigFilePath

        let parsePort (cfg: AppConfig) msg port =
            match port with
            | Some p -> p
            | None -> cfg.Server.Port
            |> function
            | p when p <= 0 -> ArgParseError msg |> Result.Error
            | p -> Ok p

        let parseClientId (cfg: AppConfig) =
            parseNesWithFallback (fun () -> Some cfg.Fitbit.ClientId)

        let parseClientSecret (cfg: AppConfig) =
            parseNesWithFallback (fun () -> Some cfg.Fitbit.ClientSecret)

        let mkConfig logLevel appCfgPath port clientId clientSecret =
            { AppConfigPath = appCfgPath
              ClientId = clientId
              ClientSecret = clientSecret
              ServerPort = port
              ServerLogLevel = logLevel }

        let parse (cfg: AppConfig) (args: ParseResults<FitbitAuthArgs>) =
            mkConfig (parseLogLevel args) (parseAppCfgPath args)
            <!> (parsePort cfg "Server port must be a valid port."
                 <| args.TryGetResult FitbitAuthArgs.ServerPort)
            <*> (parseClientId
                     cfg
                     "ClientId must be provided or specified in app configuration file."
                 <| args.TryGetResult FitbitAuthArgs.ClientId)
            <*> (parseClientSecret
                     cfg
                     "ClientSecret must be provided or specified in app configuration file."
                 <| args.TryGetResult FitbitAuthArgs.ClientSecret)

    module private Browser =
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

        let browser url =
            if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                ProcessStartInfo("cmd", sprintf "/c start %s" url)
            elif RuntimeInformation.IsOSPlatform OSPlatform.OSX then
                ProcessStartInfo("open", url)
            else
                ProcessStartInfo("xdg-open", url)

        let launchUrl (cfg: ParsedArgs) =
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


    let private startAccessTokenProcessor (cfg: ParsedArgs)
                                          (cts: CancellationTokenSource)
                                          =
        let fitbitClient =
            FitbitClient.defaultConfig
                (NonEmptyString.value cfg.ClientId)
                (NonEmptyString.value cfg.ClientSecret)
            |> FitbitClient.getClient

        let atp =
            AccessTokenProcessor(fitbitClient, cfg.AppConfigPath, cts)

        atp.Start()
        atp

    let private mkServer (cfg: ParsedArgs) (atAgent: AccessTokenProcessor) =
        { ProcessAccessToken = atAgent.Process }
        |> createHostBuilder
            { Port = cfg.ServerPort
              LogLevel = cfg.ServerLogLevel }

    let private startServer cfg =
        use cts = new CancellationTokenSource()

        let atAgent = startAccessTokenProcessor cfg cts

        let server = (mkServer cfg atAgent).Build()

        server.RunAsync cts.Token
        |> Async.AwaitTask
        |> Async.RunSynchronously

        Ok()


    let exec cfg args =
        let K f x = f x |> Result.map (fun _ -> x)

        Parsing.parse cfg args
        >>= K Browser.launchUrl
        >>= K startServer
        |> Result.ignore
