namespace Trenton.Cli.Verbs

open System.Threading
open Trenton.Common
open Microsoft.Extensions.Hosting
open Trenton.Cli
open Trenton.Cli.Verbs.AuthFitbit
open Trenton.Cli.Verbs.AuthFitbit.Host
open Trenton.Cli.Verbs.AuthFitbit.ConfigFileAuthRepository
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open System.Diagnostics
open System.Runtime.InteropServices

module AuthFitbitExecution =
    module private Config =
        type AuthConfig =
            { ClientId: NonEmptyString.T
              ClientSecret: NonEmptyString.T
              ServerPort: int }

        let mkConfig port clientId clientSecret =
            { ClientId = clientId
              ClientSecret = clientSecret
              ServerPort = port }

        let parse (cfg: AppConfig) =
            mkConfig cfg.Server.Port
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


    let private startAccessTokenProcessor (cfg: Config.AuthConfig)
                                          (cts: CancellationTokenSource)
                                          =
        let fitbitClient =
            FitbitClient.defaultConfig
                (NonEmptyString.value cfg.ClientId)
                (NonEmptyString.value cfg.ClientSecret)
            |> FitbitClient.getClient

        let fitbitAuthRepo = configFileAuthRepository

        let svc =
            FitbitService.defaultSvc fitbitClient fitbitAuthRepo

        let atp = AccessTokenProcessor(svc, cts)
        atp.Start()
        atp

    let private mkServer (cfg: Config.AuthConfig)
                         (atAgent: AccessTokenProcessor)
                         =
        { ProcessAccessToken = atAgent.Process }
        |> createHostBuilder { Port = cfg.ServerPort }

    let exec cfg =
        let K x = Result.map (fun _ -> x)

        use cts = new CancellationTokenSource()

        Config.parse cfg
        >>= (fun cfg -> Browser.launchUrl cfg |> K cfg)
        >>= (fun cfg ->
            let atAgent = startAccessTokenProcessor cfg cts

            let server = (mkServer cfg atAgent).Build()
            server.RunAsync cts.Token
            |> Async.AwaitTask
            |> Async.RunSynchronously

            Ok())
