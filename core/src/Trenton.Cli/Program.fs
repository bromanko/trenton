namespace Trenton.Cli

open System
open Argu
open Trenton.Cli.Commands

module Main =
    let private printUsage (parser: ArgumentParser<_>) =
        parser.PrintUsage() |> printf "%s"

    let private printUnknown parser =
        eprintf "Unknown argument specified."
        printUsage parser

    [<EntryPoint>]
    let main argv =
        let errorHandler =
            ProcessExiter
                (colorizer =
                    function
                    | ErrorCode.HelpText -> None
                    | _ -> Some ConsoleColor.Red)

        let parser =
            ArgumentParser.Create<MainArgs>
                (programName = "trenton", errorHandler = errorHandler)

        match parser.ParseCommandLine(inputs = argv).GetAllResults() with
        | [] -> printUsage parser
        | [ Version ] -> Version.exec
        | _ -> printUnknown parser

        0
