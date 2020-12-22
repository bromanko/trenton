namespace Trenton.Cli

open Argu

type FitbitAuthArgs =
    | [<Unique>] ServerPort of int
    | [<Unique>] ClientId of string
    | [<Unique>] ClientSecret of string
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | ServerPort _ -> "The port to run the auth callback server on."
            | ClientId _ -> "Client ID of the OAuth Application."
            | ClientSecret _ -> "Client Secret of the OAuth Application."

type AuthArgs =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Fitbit of
        ParseResults<FitbitAuthArgs>
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Fitbit _ -> "Authenticates with Fitbit."
