namespace Trenton.Cli.Verbs.Auth.Whoop

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
        { AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T }

    module private Parsing =
        let mkConfig accessToken refreshToken =
            { AccessToken = accessToken
              RefreshToken = refreshToken }

        let parse (args: ParseResults<WhoopRefreshTokenArgs>) =
            mkConfig
            <!> (parseNes "Access Token must be a valid string."
                 <| args.GetResult WhoopRefreshTokenArgs.AccessToken)
            <*> (parseNes "Refresh token must be a valid string."
                 <| args.GetResult WhoopRefreshTokenArgs.RefreshToken)

    module private Execution =
        let mapError =
            function
            | WhoopClient.Error e -> ExecError.UnknownError e
            | WhoopClient.Exception ex -> ExecError.Exception ex

        let exec console cfg =
            let client =
                WhoopClient.defaultConfig |> WhoopClient.getClient

            client.GetAccessToken(
                WhoopClient.RefreshToken
                    { RefreshToken = NonEmptyString.value cfg.RefreshToken }
            )
            |> AsyncResult.map (logDto console)
            |> AsyncResult.mapError mapError
            |> Async.RunSynchronously

    let Exec console args =
        Parsing.parse args >>= Execution.exec console
