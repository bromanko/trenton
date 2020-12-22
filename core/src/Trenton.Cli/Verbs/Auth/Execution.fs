namespace Trenton.Cli.Verbs.Auth

open Argu
open Trenton.Cli

module Execution =
    let exec cfg (args: ParseResults<AuthArgs>) =
        match args.GetSubCommand() with
        | AuthArgs.Fitbit f -> Fitbit.Execution.exec cfg f
