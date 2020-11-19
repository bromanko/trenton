namespace Trenton.Cli

open System
open Argu
open Trenton.Cli.Verbs

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

    let private printException ex =
        eprintfn "ERROR: An exception has occurred."
        eprintfn "%O" ex

    let private execCommand results =
        match results with
        | [ Version ] -> Version.exec ()
        | [ Export r ] -> Export.exec r
        | _ -> Result.Error <| UnknownVerb "A command must be specified."

    [<EntryPoint>]
    let main argv =
        parser.ParseCommandLine(argv).GetAllResults()
        |> execCommand
        |> function
        | Error e ->
            match e with
            | UnknownVerb m ->
                printUnknown m
                1
            | Exception ex ->
                printException ex
                1
        | Ok _ ->
            0
