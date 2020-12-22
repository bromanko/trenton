namespace Trenton.Cli

open Argu
open Trenton.Common

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

type GlobalConfig =
    { Debug: bool
      ConfigFilePath: NonEmptyString.T }

    static member private DefaultConfig =
        { Debug = false
          ConfigFilePath =
              (NonEmptyString.create Config.DefaultConfigPath)
                  .Value }

    static member ParseArgs(parseResults: ParseResults<_>) =
        match parseResults.UnrecognizedCliParseResults with
        | [ :? MainArgs as m ] ->
            match m with
            | Debug ->
                { GlobalConfig.DefaultConfig with
                      Debug = true }
            | _ -> GlobalConfig.DefaultConfig
        | _ -> GlobalConfig.DefaultConfig
