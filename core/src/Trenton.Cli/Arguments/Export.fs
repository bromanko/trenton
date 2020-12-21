namespace Trenton.Cli

open Argu

type FitbitExportArgs =
    | [<CustomCommandLine("--access_token"); AltCommandLine("-a"); Unique>] AccessToken of
        string
    | [<CustomCommandLine("--refresh_token"); AltCommandLine("-r"); Unique>] RefreshToken of
        string
    | [<CustomCommandLine("--start"); AltCommandLine("-s"); Mandatory; Unique>] StartDate of
        string
    | [<CustomCommandLine("--end"); AltCommandLine("-e"); Unique>] EndDate of
        string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | AccessToken _ -> "A user OAuth access token."
            | RefreshToken _ -> "An OAuth refresh token."
            | StartDate _ -> "Export data starting from this date."
            | EndDate _ -> "Export data ending on this date."

type ExportArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Fitbit of
        ParseResults<FitbitExportArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit _ -> "Exports data from Fitbit."
