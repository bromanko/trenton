namespace Trenton.Cli.Verbs

open System.Diagnostics
open System.Runtime.InteropServices
open System.Threading
open Microsoft.Extensions.Logging
open Trenton.Common
open Microsoft.Extensions.Hosting
open Trenton.Cli
open Trenton.Cli.Verbs.AuthFitbit
open Trenton.Cli.Verbs.AuthFitbit.Host
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module AuthFitbitExecution =
    module private Config =
        type AuthConfig =
            { ClientId: NonEmptyString.T
              ClientSecret: NonEmptyString.T
              ServerPort: int
              ServerLogLevel: Microsoft.Extensions.Logging.LogLevel }

        let parseLogLevel (gOpts: GlobalOptions) =
            if gOpts.Debug
            then Microsoft.Extensions.Logging.LogLevel.Debug
            else LogLevel.None

        let mkConfig port logLevel clientId clientSecret =
            { ClientId = clientId
              ClientSecret = clientSecret
              ServerPort = port
              ServerLogLevel = logLevel }

        let parse (cfg: AppConfig) gOpts =
            mkConfig cfg.Server.Port (parseLogLevel gOpts)
            <!> (parseNes
                     "ClientId must be specified in app configuration file."
                 <| cfg.Fitbit.ClientId)
            <*> (parseNes
                     "ClientSecret must be specified in app configuration file."
                 <| cfg.Fitbit.ClientSecret)

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

        let launchUrl (cfg: Config.AuthConfig) =
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


    let private startAccessTokenProcessor appCfg
                                          (cfg: Config.AuthConfig)
                                          (cts: CancellationTokenSource)
                                          =
        let fitbitClient =
            FitbitClient.defaultConfig
                (NonEmptyString.value cfg.ClientId)
                (NonEmptyString.value cfg.ClientSecret)
            |> FitbitClient.getClient

        let atp =
            AccessTokenProcessor(fitbitClient, appCfg, cts)

        atp.Start()
        atp

    let private mkServer (cfg: Config.AuthConfig)
                         (atAgent: AccessTokenProcessor)
                         =
        { ProcessAccessToken = atAgent.Process }
        |> createHostBuilder
            { Port = cfg.ServerPort
              LogLevel = cfg.ServerLogLevel }

    let private startServer appCfg cfg =
        use cts = new CancellationTokenSource()
        let atAgent = startAccessTokenProcessor appCfg cfg cts

        let server = (mkServer cfg atAgent).Build()

        server.RunAsync cts.Token
        |> Async.AwaitTask
        |> Async.RunSynchronously

        Ok()


    let exec cfg gOpts _ =
        let K f x = f x |> Result.map (fun _ -> x)

        Config.parse cfg gOpts
        >>= K Browser.launchUrl
        >>= K (startServer cfg)
        |> Result.map (fun _ -> ())
