namespace Trenton.Cli.Verbs.Auth.Whoop

open Argu
open Trenton.Common
open Trenton.Cli
open Trenton.Cli.LogFormatters
open Trenton.Cli.Verbs
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module Login =
    type private ParsedArgs =
        { UserName: NonEmptyString.T
          Password: NonEmptyString.T }

    module private Parsing =
        let mkConfig username password =
            { UserName = username
              Password = password }

        let parse (args: ParseResults<WhoopLoginArgs>) =
            mkConfig
            <!> (parseNes "Username must be provided."
                 <| args.GetResult WhoopLoginArgs.UserName)
            <*> (parseNes "Password must be provided."
                 <| args.GetResult WhoopLoginArgs.Password)


    module private Execution =
        let mapError =
            function
            | WhoopClient.Error e -> ExecError.UnknownError e
            | WhoopClient.Exception ex -> ExecError.Exception ex

        let exec console (cfg: ParsedArgs) =
            let client =
                WhoopClient.defaultConfig |> WhoopClient.getClient

            let r =
                WhoopClient.Password
                    { WhoopClient.Username = NonEmptyString.value cfg.UserName
                      WhoopClient.Password = NonEmptyString.value cfg.Password
                      WhoopClient.IssueRefresh = true }

            client.GetAccessToken r
            |> AsyncResult.map (logDto console)
            |> AsyncResult.mapError mapError
            |> Async.RunSynchronously

    let Exec console args =
        Parsing.parse args >>= Execution.exec console
