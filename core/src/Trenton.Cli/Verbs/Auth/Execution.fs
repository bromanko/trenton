namespace Trenton.Cli.Verbs.Auth

open Argu
open Trenton.Cli

module Execution =
    let exec cfg gOpts (args: ParseResults<AuthArgs>) =
        match args.GetSubCommand() with
        | AuthArgs.Fitbit  -> Fitbit.Execution.exec cfg gOpts args
