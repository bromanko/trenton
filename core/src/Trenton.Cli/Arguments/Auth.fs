namespace Trenton.Cli

open Argu

type FitbitLoginArgs =
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

type FitbitRefreshTokenArgs =
    | [<Unique; Mandatory; CustomCommandLine("--client_id")>] ClientId of string
    | [<Unique; Mandatory; CustomCommandLine("--client_secret")>] ClientSecret of
        string
    | [<Unique; CustomCommandLine("--access_token")>] AccessToken of string
    | [<Unique; CustomCommandLine("--refresh_token")>] RefreshToken of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | AccessToken _ -> "Fitbit access token to be refreshed."
            | RefreshToken _ -> "Refresh token."
            | ClientId _ -> "Client ID of the OAuth Application."
            | ClientSecret _ -> "Client Secret of the OAuth Application."

[<RequireSubcommand>]
type FitbitAuthArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Login of
        ParseResults<FitbitLoginArgs>
    | [<SubCommand;
        CliPrefix(CliPrefix.None);
        CustomCommandLine("refresh-token")>] RefreshToken of
        ParseResults<FitbitRefreshTokenArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Login _ -> "Login to Fitbit."
            | RefreshToken _ -> "Refresh an access token."

type WhoopLoginArgs =
    | [<Unique; CustomCommandLine("--username")>] UserName of string
    | [<Unique; CustomCommandLine("--password")>] Password of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | UserName _ -> "Whoop username."
            | Password _ -> "Whoop password."

type WhoopRefreshTokenArgs =
    | [<Unique; CustomCommandLine("--access_token")>] AccessToken of string
    | [<Unique; CustomCommandLine("--refresh_token")>] RefreshToken of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | AccessToken _ -> "Whoop access token to be refreshed."
            | RefreshToken _ -> "Refresh token."

[<RequireSubcommand>]
type WhoopAuthArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Login of
        ParseResults<WhoopLoginArgs>
    | [<SubCommand;
        CliPrefix(CliPrefix.None);
        CustomCommandLine("refresh-token")>] RefreshToken of
        ParseResults<WhoopRefreshTokenArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Login _ -> "Login to Whoop."
            | RefreshToken _ -> "Refresh an access token."

[<RequireSubcommand>]
type AuthArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Fitbit of
        ParseResults<FitbitAuthArgs>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Whoop of
        ParseResults<WhoopAuthArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit _ -> "Fitbit authentication commands."
            | Whoop _ -> "Whoop authentication commands."
