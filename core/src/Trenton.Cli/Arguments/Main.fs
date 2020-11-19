namespace Trenton.Cli

open Argu

type MainArgs =
    | Version
    | [<CliPrefix(CliPrefix.None)>] Export of ParseResults<ExportArgs>

    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Version -> "Prints the version and exits."
            | Export _ -> "Exports data from services."
