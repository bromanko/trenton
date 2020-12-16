namespace Trenton.Cli

open System
open Argu
open Trenton.Cli
open Trenton.Cli.Verbs
open FsToolkit.ErrorHandling.Operator.Result

module Main =
    let private errorHandler =
        ProcessExiter
            (colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red)

    let private parser =
        ArgumentParser.Create<MainArgs>
            (programName = "trenton", errorHandler = errorHandler)

    let private printUsage () = parser.PrintUsage() |> printf "%s"

    let private printError ex =
        eprintfn "ERROR: An error has occurred."
        eprintfn "%O" ex

    let private printConfigError e =
        sprintf "The config file could not be parsed.\n%O" e
        |> printError

    let private reportError =
        function
        | UnknownVerb m ->
            printError m
            printUsage ()
        | ArgParseError e ->
            printError e
            printUsage ()
        | Exception ex -> printError ex
        | ConfigLoadError c ->
            match c with
            | LoadException e -> printError e
            | NotFound ->
                sprintf "Config file not found at %s." Config.DefaultConfigPath
                |> printError
            | ParseError e ->
                sprintf "The config file could not be parsed.\n%O" e
                |> printError

    let private execCommand argv config =
        let parsed = parser.ParseCommandLine(argv)
        let res = parsed.GetAllResults()

        match res with
        | [ Version ] -> Version.exec ()
        | _ ->
            let gOpts = GlobalOptions.FromParseResults res

            match parsed.GetSubCommand() with
            | Auth s -> Auth.exec config gOpts s
            | _ ->
                UnknownVerb "A valid command must be specified."
                |> Error

    let loadConfig () = Config.load Config.DefaultConfigPath

    [<EntryPoint>]
    let main argv =
        loadConfig ()
        |> Result.mapError ExecError.ConfigLoadError
        >>= execCommand argv
        |> function
        | Ok _ -> 0
        | Error e ->
            reportError e
            1
