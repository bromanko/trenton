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

    let private printError ex = eprintfn "ERROR: %O" ex

    let private printConfigError e =
        sprintf "The config file could not be parsed.\n%O" e
        |> printError

    let private reportError =
        function
        | UnknownVerb m ->
            printError m
            printf "\n"
            printUsage ()
        | ArgParseError e -> printError e
        | Exception ex -> printError ex
        | ConfigFileError c ->
            match c with
            | LoadException e -> printError e
            | NotFound p ->
                sprintf "Config file not found at %s." p
                |> printError
            | ParseError e ->
                sprintf "The config file could not be parsed.\n%O" e
                |> printError

    let private execCommand argv cfg =
        let parsed = parser.ParseCommandLine(argv)
        let res = parsed.GetAllResults()

        match res with
        | [ Version ] -> Version.exec ()
        | _ ->
            match parsed.GetSubCommand() with
            | Auth args -> Auth.Execution.exec cfg args
            //            | Export s -> Export.Execution.exec gOpts s
            | _ ->
                UnknownVerb "A valid command must be specified."
                |> Error

    [<EntryPoint>]
    let main argv =
        Config.load Config.DefaultConfigPath
        |> Result.mapError ExecError.ConfigFileError
        >>= execCommand argv
        |> function
        | Ok _ -> 0
        | Error e ->
            reportError e
            1
