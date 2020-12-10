namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli

module Export =
    let exec cfg (args: ParseResults<ExportArgs>) =
        match args.GetSubCommand() with
        | ExportArgs.Fitbit args -> ExportFitbit.exec cfg args
