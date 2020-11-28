namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Common
open Trenton.Health.FitbitService
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module ExportFitbit =
    type private ExportConfig =
        { AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T option
          StartDate: Date.T
          EndDate: Date.T option }

    let private mkConfig accessToken refreshToken startDate endDate =
        { AccessToken = accessToken
          RefreshToken = refreshToken
          StartDate = startDate
          EndDate = endDate }

    let parseClientId cfg (args: ParseResults<FitbitExportArgs>) =
        let argVal = args.TryGetResult ClientId
        let cfgVal = cfg.Fitbit
        ()

    //         match cfg with
//         | None ->
    // check args

    let private parseCfg (cfg: AppConfig option)
                         (args: ParseResults<FitbitExportArgs>)
                         =
        printfn "%O" cfg
        mkConfig
        <!> (parseNes "Access token must be specified."
             <| args.GetResult AccessToken)
        <*> (parseOptionalNes "Refresh token must be a valid string."
             <| args.TryGetResult RefreshToken)
        <*> (parseDate "Start date must be a valid date."
             <| args.GetResult StartDate)
        <*> (parseOptionalDate "End date must be a valid date."
             <| args.TryGetResult EndDate)

    let private export cfg args = Ok()

    let exec (cfg: AppConfig option) (args: ParseResults<FitbitExportArgs>) =
        parseCfg cfg args >>= export cfg
