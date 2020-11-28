namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli

module Export =
    let exec cfg (args: ParseResults<ExportArgs>) =
        match args.GetAllResults() with
        | [ Fitbit args ] -> ExportFitbit.exec cfg args
        | _ ->
            UnknownVerb "An export command must be specified."
            |> Error
