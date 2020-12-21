namespace Trenton.Cli.Verbs.Export.Fitbit

open Argu
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Common
open Trenton.Health.FitbitService
open FsToolkit.ErrorHandling.Operator.Result

module Execution =
    type private ExportConfig =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T option
          StartDate: Date.T
          EndDate: Date.T option }

    let private parseAccessToken appCfg =
        parseNesWithFallback (fun () ->
            match appCfg.Fitbit.Auth with
            | None -> None
            | Some a -> Some a.AccessToken)

    let private parseRefreshToken appCfg =
        parseOptionalNesWithFallback (fun () ->
            match appCfg.Fitbit.Auth with
            | None -> None
            | Some a -> Some a.RefreshToken)

    let private mkConfig clientId
                         clientSecret
                         accessToken
                         refreshToken
                         startDate
                         endDate
                         =
        { ClientId = clientId
          ClientSecret = clientSecret
          AccessToken = accessToken
          RefreshToken = refreshToken
          StartDate = startDate
          EndDate = endDate }

    let private parseCfg appCfg (args: ParseResults<FitbitExportArgs>) =
        mkConfig
        <!> (parseNes "ClientId must be specified in app configuration file"
             <| appCfg.Fitbit.ClientId)
        <*> (parseNes
                 "Client secret must be specified in app configuration file"
             <| appCfg.Fitbit.ClientSecret)
        <*> (parseAccessToken
                 appCfg
                 "Access token must be specified as an argument or in app configuration file."
             <| args.TryGetResult AccessToken)
        <*> (parseRefreshToken appCfg "Refresh token must be a valid string."
             <| args.TryGetResult RefreshToken)
        <*> (parseDate "Start date must be a valid date."
             <| args.GetResult StartDate)
        <*> (parseOptionalDate "End date must be a valid date."
             <| args.TryGetResult EndDate)

    let private export cfg =
        printfn "%O" cfg
        Ok()

    let exec cfg _ args = parseCfg cfg args >>= export
