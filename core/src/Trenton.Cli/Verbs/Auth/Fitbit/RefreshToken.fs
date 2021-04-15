namespace Trenton.Cli.Verbs.Auth.Fitbit

open Argu
open Trenton.Common
open Trenton.Cli
open Trenton.Cli.LogFormatters
open Trenton.Cli.Verbs
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module RefreshToken =
    type private ParsedArgs =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T }

    module private Parsing =
        let mkConfig clientId clientSecret accessToken refreshToken =
            { ClientId = clientId
              ClientSecret = clientSecret
              AccessToken = accessToken
              RefreshToken = refreshToken }

        let parse (args: ParseResults<FitbitRefreshTokenArgs>) =
            mkConfig
            <!> (parseNes "Client ID must be a valid string."
                 <| args.GetResult FitbitRefreshTokenArgs.ClientId)
            <*> (parseNes "Client Secret must be a valid string."
                 <| args.GetResult FitbitRefreshTokenArgs.ClientSecret)
            <*> (parseNes "Access Token must be a valid string."
                 <| args.GetResult FitbitRefreshTokenArgs.AccessToken)
            <*> (parseNes "Refresh token must be a valid string."
                 <| args.GetResult FitbitRefreshTokenArgs.RefreshToken)

    module private Execution =
        let mkClient cfg : FitbitClient.T =
            FitbitClient.defaultConfig
                (NonEmptyString.value cfg.ClientId)
                (NonEmptyString.value cfg.ClientSecret)
            |> FitbitClient.getClient

        let mapError =
            function
            | FitbitClient.Error e -> ExecError.UnknownError e
            | FitbitClient.Exception ex -> ExecError.Exception ex

        let exec console cfg =
            let client = mkClient cfg

            client.RefreshAccessToken
                { RefreshToken = NonEmptyString.value cfg.RefreshToken }
            |> AsyncResult.map (logDto console)
            |> AsyncResult.mapError mapError
            |> Async.RunSynchronously

    let Exec console args =
        Parsing.parse args >>= Execution.exec console
