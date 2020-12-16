namespace Trenton.Cli

open Argu

type MainArgs =
    | Version
    | [<Inherit>] Debug
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Auth of ParseResults<AuthArgs>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Export of
        ParseResults<ExportArgs>

    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Version -> "Prints the version and exits."
            | Debug -> "Output debug logging during command execution."
            | Auth _ -> "Configures authentication for services."
            | Export _ -> "Exports data from services."

type GlobalOptions =
    { Debug: bool
      ConfigFilePath: string }

    static member private DefaultOptions =
        { Debug = false
          ConfigFilePath = Config.DefaultConfigPath }

    static member FromParseResults(res: MainArgs list) =
        match res with
        | x when Seq.contains Debug x ->
            { GlobalOptions.DefaultOptions with
                  Debug = true }
        | _ ->
            { GlobalOptions.DefaultOptions with
                  Debug = false }
