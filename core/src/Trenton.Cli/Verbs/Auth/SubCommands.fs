namespace Trenton.Cli.Verbs.Auth

open Argu
open Trenton.Cli

module SubCommands =
    let Exec (args: ParseResults<AuthArgs>) =
        match args.GetSubCommand() with
        | AuthArgs.Fitbit f -> Fitbit.Execution.Exec f
