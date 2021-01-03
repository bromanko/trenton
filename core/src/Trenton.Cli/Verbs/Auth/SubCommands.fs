namespace Trenton.Cli.Verbs.Auth

open Argu
open Trenton.Cli

module SubCommands =
    let Exec console (args: ParseResults<AuthArgs>) =
        match args.GetSubCommand() with
        | AuthArgs.Fitbit f -> Fitbit.Execution.Exec console f
