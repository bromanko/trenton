namespace Trenton.Cli

open Argu

type ConfigArgs =
    | [<CustomCommandLine("--client_id"); Mandatory; Unique>] ClientId of string
    | [<CustomCommandLine("--client_secret"); Mandatory; Unique>] ClientSecret of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | ClientId _ -> "Client ID of the Fitbit app."
            | ClientSecret _ -> "Client Secret of the Fitbit app."
