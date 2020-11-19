namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli

module Export =
    let exec (args: ParseResults<ExportArgs>) =
        match args.GetAllResults() with
        | [ Fitbit args ] -> ExportFitbit.exec args
        | _ -> Error <| UnknownVerb "An export command must be specified."
