namespace Trenton.Cli.Verbs.Export

open Argu
open Trenton.Cli

module Execution =
    let exec cfg gOpts (args: ParseResults<ExportArgs>) =
        match args.GetSubCommand() with
        | ExportArgs.Fitbit sc -> Fitbit.Execution.exec gOpts sc
