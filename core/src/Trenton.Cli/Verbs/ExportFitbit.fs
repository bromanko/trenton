namespace Trenton.Cli.Verbs

open Argu
open FsToolkit.ErrorHandling.Operator.Result
open Trenton.Cli
open Trenton.Common
open System

module ExportFitbit =
    type private ExportConfig =
        { AccessToken: NonEmptyString.T
          RefreshToken: string
          StartDate: Date.T
          EndDate: Date.T option }

    let private parseNonEmptyString msg a =
        match NonEmptyString.create a with
        | None -> Result.Error <| ArgParseError msg
        | Some s -> Ok s

    let private parseDate msg a =
        match Date.tryParse a with
        | None -> Result.Error <| ArgParseError msg
        | Some d -> Ok d

    let private tryParseDate a = Date.tryParse a |> Ok

    let private mkConfig accessToken refreshToken startDate endDate =
        { AccessToken = accessToken
          RefreshToken = refreshToken
          StartDate = startDate
          EndDate = endDate }

    let private parseConfig (args: ParseResults<FitbitExportArgs>) =
        mkConfig
        <!> (parseNonEmptyString "Access token must be specified."
             <| args.GetResult <@ AccessToken @>)
        <*> (Ok <| args.GetResult <@ RefreshToken @>)
        <*> (parseDate "Start date must be a valid date."
             <| args.GetResult <@ StartDate @>)
        <*> (tryParseDate <| args.GetResult <@ EndDate @>)

    let exec (args: ParseResults<FitbitExportArgs>) =
        parseConfig args >>= (fun _ -> Ok())
