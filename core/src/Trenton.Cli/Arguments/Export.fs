namespace Trenton.Cli

open Argu

type FitbitExportArgs =
    | [<Unique; Mandatory; CustomCommandLine("--client_id")>] ClientId of string
    | [<Unique; Mandatory; CustomCommandLine("--client_secret")>] ClientSecret of
        string
    | [<CustomCommandLine("--access_token"); AltCommandLine("-a"); Unique>] AccessToken of
        string
    | [<CustomCommandLine("--refresh_token"); AltCommandLine("-r"); Unique>] RefreshToken of
        string
    | [<CustomCommandLine("--start"); AltCommandLine("-s"); Mandatory; Unique>] StartDate of
        string
    | [<CustomCommandLine("--end"); AltCommandLine("-e"); Unique>] EndDate of
        string
    | [<CustomCommandLine("--out"); Unique; Mandatory>] OutputDirectory of
        string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | ClientId _ -> "Client ID of the OAuth Application."
            | ClientSecret _ -> "Client Secret of the OAuth Application."
            | AccessToken _ -> "A user OAuth access token."
            | RefreshToken _ -> "An OAuth refresh token."
            | StartDate _ -> "Export data starting from this date."
            | EndDate _ -> "Export data ending on this date."
            | OutputDirectory _ -> "Output directory for exported data."

type WhoopExportArgs =
    | [<CustomCommandLine("--access_token"); AltCommandLine("-a"); Unique>] AccessToken of
        string
    | [<CustomCommandLine("--refresh_token"); AltCommandLine("-r"); Unique>] RefreshToken of
        string
    | [<CustomCommandLine("--user_id"); AltCommandLine("-u"); Unique>] UserId of
        int
    | [<CustomCommandLine("--start"); AltCommandLine("-s"); Mandatory; Unique>] StartDate of
        string
    | [<CustomCommandLine("--end"); AltCommandLine("-e"); Unique>] EndDate of
        string
    | [<CustomCommandLine("--out"); Unique; Mandatory>] OutputDirectory of
        string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | AccessToken _ -> "A user OAuth access token."
            | RefreshToken _ -> "An OAuth refresh token."
            | UserId _ -> "User ID to export data for."
            | StartDate _ -> "Export data starting from this date."
            | EndDate _ -> "Export data ending on this date."
            | OutputDirectory _ -> "Output directory for exported data."

type ExportArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Fitbit of
        ParseResults<FitbitExportArgs>
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Whoop of
        ParseResults<WhoopExportArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit _ -> "Export data from Fitbit."
            | Whoop _ -> "Export data from Whoop."
