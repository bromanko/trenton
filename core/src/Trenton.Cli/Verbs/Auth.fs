namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli

module Auth =
    let exec cfg (args: ParseResults<AuthArgs>) =
        match args.GetAllResults() with
        | [ AuthArgs.Fitbit ] -> AuthFitbitExecution.exec cfg
        | _ ->
            UnknownVerb "An auth command must be specified."
            |> Error
