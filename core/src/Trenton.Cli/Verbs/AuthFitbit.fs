namespace Trenton.Cli.Verbs

open Trenton.Common
open Trenton.Cli
open FsToolkit.ErrorHandling.Operator.Result

module AuthFitbit =
    type private AuthConfig =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T }

    let private mkConfig clientId clientSecret =
        { ClientId = clientId
          ClientSecret = clientSecret }

    let private parseCfg (cfg: AppConfig) =
        mkConfig
        <!> (parseNes "ClientId must be specified in app configuration file"
             <| cfg.Fitbit.ClientId)
        <*> (parseNes
                 "Client secret must be specified in app configuration file"
             <| cfg.Fitbit.ClientSecret)

    let exec cfg =
        parseCfg cfg
        >>= (fun _ -> Result.Error ConfigFileNotFound)
