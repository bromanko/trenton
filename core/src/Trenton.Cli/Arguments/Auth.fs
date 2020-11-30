namespace Trenton.Cli

open Argu

type AuthArgs =
    | [<CliPrefix(CliPrefix.None)>] Fitbit
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit -> "Exports data from Fitbit."
