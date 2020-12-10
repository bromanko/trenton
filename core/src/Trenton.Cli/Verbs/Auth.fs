namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli

module Auth =
    let exec cfg gOpts (args: ParseResults<AuthArgs>) =
        match args.GetSubCommand() with
        | AuthArgs.Fitbit  -> AuthFitbitExecution.exec cfg gOpts args
