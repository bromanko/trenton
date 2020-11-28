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

    let private printUnknown msg =
        eprintfn "ERROR: %s" msg
        printUsage ()

    let private printError ex =
        eprintfn "ERROR: An error has occurred."
        eprintfn "%O" ex

    let private printConfigError e =
        sprintf "The config file could not be parsed.\n%O" e
        |> printError

    let private execCommand config args =
        match args with
        | [ Version ] -> Version.exec ()
        | [ Export r ] -> Export.exec config r
        | _ ->
            UnknownVerb "A command must be specified." |> Error

    let loadConfig () =
        Config.load Config.DefaultConfigRoot Config.DefaultConfigFilename

    let parseCommandLine argv =
        parser.ParseCommandLine(argv).GetAllResults()
        |> Ok

    [<EntryPoint>]
    let main argv =
        execCommand
        <!> loadConfig ()
        <*> parseCommandLine argv
        |> function
        | Error e ->
            match e with
            | UnknownVerb m -> printUnknown m
            | ConfigFileNotFound -> printError "Config file not found."
            | ConfigParseError e -> printConfigError e
            | ArgParseError e -> printError e
            | Exception ex -> printError ex
            1
        | Ok _ -> 0
