namespace Trenton.Cli.Verbs.Auth

open Argu
open Trenton.Cli

module SubCommands =
    let Exec console (args: ParseResults<AuthArgs>) =
        match args.GetSubCommand() with
        | AuthArgs.Fitbit a -> Fitbit.SubCommands.Exec console a
        | AuthArgs.Whoop a -> Whoop.SubCommands.Exec console a
