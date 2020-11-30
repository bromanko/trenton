namespace Trenton.Cli

open Argu

type AuthArgs =
    | [<CliPrefix(CliPrefix.None)>] Fitbit
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit -> "Authenticates with Fitbit."
