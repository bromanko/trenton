namespace Trenton.Cli

open Argu

type FitbitAuthArgs =
    | [<Unique; CustomCommandLine("--server_port")>] ServerPort of int
    | [<Unique; CustomCommandLine("--server_log_level")>] ServerLogLevel of
        Microsoft.Extensions.Logging.LogLevel
    | [<Unique; Mandatory; CustomCommandLine("--client_id")>] ClientId of string
    | [<Unique; Mandatory; CustomCommandLine("--client_secret")>] ClientSecret of
        string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | ServerPort _ -> "The port to run the auth callback server on."
            | ServerLogLevel _ -> "Level of logging for the callback server."
            | ClientId _ -> "Client ID of the OAuth Application."
            | ClientSecret _ -> "Client Secret of the OAuth Application."

[<RequireSubcommand>]
type AuthArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Fitbit of
        ParseResults<FitbitAuthArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit _ -> "Authenticates with Fitbit."
