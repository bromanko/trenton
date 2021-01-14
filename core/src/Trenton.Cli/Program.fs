namespace Trenton.Cli

open System
open Argu
open Trenton.Cli
open Trenton.Cli.Verbs

module Main =
    let private errorHandler =
        ProcessExiter
            (colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red)

    let private printUsage (parser: ArgumentParser<_>) (console: #IConsole) =
        parser.PrintUsage() |> console.Out.Write

    let private printError (console: #IConsole) ex =
        sprintf "ERROR: %O" ex |> console.Error.Write

    let private printConfigError (console: #IConsole) e =
        sprintf "The config file could not be parsed.\n%O" e
        |> printError console

    let private reportError console parser =
        function
        | UnknownVerb m ->
            printError console m
            printf "\n"
            printUsage parser console
        | ArgParseError e -> printError console e
        | Exception ex -> printError console ex
        | UnknownError e -> printError console e

    let private execCommand console (parser: ArgumentParser<MainArgs>) argv =
        let parsed = parser.ParseCommandLine(argv)
        let res = parsed.GetAllResults()

        match res with
        | [ Version ] -> Version.Exec console
        | _ ->
            match parsed.GetSubCommand() with
            | Auth args -> Auth.SubCommands.Exec console args
            | Export args -> Export.SubCommands.Exec console args
            | _ ->
                UnknownVerb "A valid command must be specified."
                |> Error

    [<EntryPoint>]
    let main argv =
        let console = SystemConsole()

        let parser =
            ArgumentParser.Create<MainArgs>
                (programName = "trenton", errorHandler = errorHandler)

        execCommand console parser argv
        |> function
        | Ok _ -> 0
        | Error e ->
            reportError console parser e
            1
