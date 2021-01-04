namespace Trenton.Cli.Verbs.Export.Fitbit

open Argu
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Common
open FsToolkit.ErrorHandling.Operator.Result

module Execution =
    type private ExportConfig =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T option
          StartDate: Date.T
          EndDate: Date.T option }


    module private Parsing =
        //
//    let private parseAccessToken appCfg =
//        parseNesWithFallback (fun () ->
//            match appCfg.Fitbit.Auth with
//            | None -> None
//            | Some a -> Some a.AccessToken)
//
//    let private parseRefreshToken appCfg =
//        parseOptionalNesWithFallback (fun () ->
//            match appCfg.Fitbit.Auth with
//            | None -> None
//            | Some a -> Some a.RefreshToken)
//
        let mkConfig clientId
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

        let parse (args: ParseResults<FitbitExportArgs>) =
            mkConfig
            <!> (parseNes "Client ID must be a valid string."
                 <| args.GetResult FitbitExportArgs.ClientId)
            <*> (parseNes "Client Secret must be a valid string."
                 <| args.GetResult FitbitExportArgs.ClientSecret)
            <*> (parseNes "Access Token must be a valid string."
                 <| args.GetResult AccessToken)
            <*> (parseOptionalNes "Refresh token must be a valid string."
                 <| args.TryGetResult RefreshToken)
            <*> (parseDate "Start date must be a valid date."
                 <| args.GetResult StartDate)
            <*> (parseOptionalDate "End date must be a valid date."
                 <| args.TryGetResult EndDate)

    let private export (console: #IConsole) cfg =
        sprintf "%O" cfg |> console.Out.Write
        Ok()

    let Exec console args = Parsing.parse args >>= export console
