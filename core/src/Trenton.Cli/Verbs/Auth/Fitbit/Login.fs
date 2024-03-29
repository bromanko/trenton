namespace Trenton.Cli.Verbs.Auth.Fitbit

open Argu
open System.Diagnostics
open System.Runtime.InteropServices
open System.Threading
open Trenton.Common
open Microsoft.Extensions.Hosting
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Cli.Verbs.Auth.Fitbit
open Trenton.Cli.Verbs.Auth.Fitbit.Host
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module Login =
    [<Literal>]
    let DefaultServerPort = 9032

    type private ParsedArgs =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          ServerPort: int
          ServerLogLevel: Microsoft.Extensions.Logging.LogLevel }

    module private Parsing =
        let parseLogLevel =
            function
            | Some l -> Ok l
            | None -> Ok Microsoft.Extensions.Logging.LogLevel.None

        let parsePort msg port =
            match port with
            | Some p -> p
            | None -> DefaultServerPort
            |> function
            | p when p <= 0 -> ArgParseError msg |> Error
            | p -> Ok p

        let mkConfig logLevel port clientId clientSecret =
            { ClientId = clientId
              ClientSecret = clientSecret
              ServerPort = port
              ServerLogLevel = logLevel }

        let parse (args: ParseResults<FitbitLoginArgs>) =
            mkConfig
            <!> (parseLogLevel
                 <| args.TryGetResult FitbitLoginArgs.ServerLogLevel)
            <*> (parsePort "Server port must be a valid port."
                 <| args.TryGetResult FitbitLoginArgs.ServerPort)
            <*> (parseNes "Client Id must be provided."
                 <| args.GetResult FitbitLoginArgs.ClientId)
            <*> (parseNes "Client Secret must be provided."
                 <| args.GetResult FitbitLoginArgs.ClientSecret)

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
                ProcessStartInfo("cmd", $"/c start %s{url}")
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
            with ex -> ExecError.Exception ex |> Error


    let private startAccessTokenProcessor (cfg: ParsedArgs) console cts =
        let fitbitClient =
            FitbitClient.defaultConfig
                (NonEmptyString.value cfg.ClientId)
                (NonEmptyString.value cfg.ClientSecret)
            |> FitbitClient.getClient

        let atp =
            AccessTokenProcessor(fitbitClient, console, cts)

        atp.Start()
        atp

    let private mkServer (cfg: ParsedArgs) (atAgent: AccessTokenProcessor) =
        { ProcessAccessToken = atAgent.Process }
        |> createHostBuilder
            { Port = cfg.ServerPort
              LogLevel = cfg.ServerLogLevel }

    let private startServer console cfg =
        use cts = new CancellationTokenSource()

        let atAgent =
            startAccessTokenProcessor cfg console cts

        let server = (mkServer cfg atAgent).Build()

        server.RunAsync cts.Token
        |> Async.AwaitTask
        |> Async.RunSynchronously

        Ok()

    let Exec console args =
        let K f x = f x |> Result.map (fun _ -> x)

        Parsing.parse args
        >>= K Browser.launchUrl
        >>= K(startServer console)
        |> Result.ignore
