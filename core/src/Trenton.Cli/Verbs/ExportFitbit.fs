namespace Trenton.Cli.Verbs

open Argu
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open Trenton.Cli
open Trenton.Common

module ExportFitbit =
    type private ExportConfig =
        { AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T option
          StartDate: Date.T
          EndDate: Date.T option }

    let private parseNes msg a =
        match NonEmptyString.create a with
        | None -> Result.Error <| ArgParseError msg
        | Some s -> Ok s

    let private parseOptionalNes msg a =
        // ToDO fix this mess
        match a with
        | Some s ->
            match parseNes msg s with
            | Result.Error e -> Result.Error e
            | Ok s -> Ok(Some s)
        | None -> Ok None

    let private parseDate msg a =
        match Date.tryParse a with
        | None -> Result.Error <| ArgParseError msg
        | Some d -> Ok d

    let private parseOptionalDate msg a =
        match a with
        | Some d ->
            match parseDate msg d with
            | Result.Error e -> Result.Error e
            | Ok d -> Ok <| Some d
        | None -> Ok None

    let private mkConfig accessToken refreshToken startDate endDate =
        { AccessToken = accessToken
          RefreshToken = refreshToken
          StartDate = startDate
          EndDate = endDate }

    let private parseConfig (args: ParseResults<FitbitExportArgs>) =
        mkConfig
        <!> (parseNes "Access token must be specified."
             <| args.GetResult <@ AccessToken @>)
        <*> (parseOptionalNes "Refresh token must be a valid string."
             <| args.TryGetResult <@ RefreshToken @>)
        <*> (parseDate "Start date must be a valid date."
             <| args.GetResult <@ StartDate @>)
        <*> (parseOptionalDate "End date must be a valid date."
             <| args.TryGetResult <@ EndDate @>)

    let exec (args: ParseResults<FitbitExportArgs>) =
        parseConfig args >>= (fun _ -> Ok())
