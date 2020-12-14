namespace Trenton.Cli.Verbs

open Argu
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Common
open Trenton.Health.FitbitService
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module ExportFitbit =
    type FitbitUserAuth =
        { AccessToken: NonEmptyString.T
          ExpiresInSeconds: int32
          RefreshToken: NonEmptyString.T }

    type private ExportConfig =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          UserAuth: FitbitUserAuth option
          StartDate: Date.T
          EndDate: Date.T option }


        let parseExpiresIn e = if e < 0 then Ok 0 else Ok e

        let parseUserAuth (cfg: FitbitConfig) =
            ResultOption.bind (fun (a: FitbitAuthConfig) ->
                mkUserAuth
                <!> (parseNes "Access token is not valid." a.AccessToken)
                <*> (parseExpiresIn a.ExpiresInSeconds)
                <*> (parseNes "Refresh token is not valid." a.RefreshToken))
                (Ok cfg.Auth)

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

    let private parseCfg (cfg: AppConfig) (args: ParseResults<FitbitExportArgs>) =
        mkConfig
        <!> (parseNes "ClientId must be specified in app configuration file"
             <| cfg.Fitbit.ClientId)
        <*> (parseNes
                 "Client secret must be specified in app configuration file"
             <| cfg.Fitbit.ClientSecret)
        <*> (parseNes "Access token must be specified."
             <| args.GetResult AccessToken)
        <*> (parseOptionalNes "Refresh token must be a valid string."
             <| args.TryGetResult RefreshToken)
        <*> (parseDate "Start date must be a valid date."
             <| args.GetResult StartDate)
        <*> (parseOptionalDate "End date must be a valid date."
             <| args.TryGetResult EndDate)

    let private export cfg = Ok()

    let exec cfg args =
        parseCfg cfg args >>= export
