namespace Trenton.Cli.Commands

open Argu

[<NoAppSettings>]
type MainArgs =
    | Version

    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Version -> "Prints the version and exits."
