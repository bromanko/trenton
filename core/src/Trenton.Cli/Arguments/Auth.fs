namespace Trenton.Cli

open Argu

type AuthArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Fitbit
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit -> "Authenticates with Fitbit."
