namespace Trenton.Cli

open Argu

type ConfigArgs =
    | [<CustomCommandLine("--client_id"); Mandatory; Unique>] ClientId of string
    | [<CustomCommandLine("--client_secret"); Mandatory; Unique>] ClientSecret of string
    | [<CustomCommandLine("--access_token");
        AltCommandLine("-a");
        Mandatory;
        Unique>] AccessToken of string
    | [<CustomCommandLine("--refresh_token"); AltCommandLine("-r"); Unique>] RefreshToken of string
    | [<CustomCommandLine("--start"); AltCommandLine("-s"); Mandatory; Unique>] StartDate of string
    | [<CustomCommandLine("--end"); AltCommandLine("-e"); Unique>] EndDate of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | ClientId _ -> "Client ID of the Fitbit app."
            | ClientSecret _ -> "Client Secret of the Fitbit app."
            | AccessToken _ -> "A user OAuth access token."
            | RefreshToken _ -> "An OAuth refresh token."
            | StartDate _ -> "Export data starting from this date."
            | EndDate _ -> "Export data ending on this date."



