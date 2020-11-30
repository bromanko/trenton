namespace Trenton.Cli

open Argu

type MainArgs =
    | Version
    | [<CliPrefix(CliPrefix.None)>] Auth of ParseResults<AuthArgs>
    | [<CliPrefix(CliPrefix.None)>] Config of ParseResults<ConfigArgs>
    | [<CliPrefix(CliPrefix.None)>] Export of ParseResults<ExportArgs>

    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Version -> "Prints the version and exits."
            | Auth _ -> "Configures authentication for services."
            | Config _ -> "Configures the application."
            | Export _ -> "Exports data from services."
