namespace Trenton.Cli.Verbs.Export

open Argu
open Trenton.Cli

module SubCommands =
    let Exec console (args: ParseResults<ExportArgs>) =
        match args.GetSubCommand() with
        | ExportArgs.Fitbit sc -> Fitbit.Execution.Exec console sc
        | ExportArgs.Whoop sc -> Whoop.Execution.Exec console sc
