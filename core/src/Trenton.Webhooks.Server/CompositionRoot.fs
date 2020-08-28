namespace Trenton.Webhooks.Server

open Trenton.Health
open Trenton.Webhooks.Server.Config

type CompositionRoot =
    { FitbitClient: FitbitClient.T }

[<AutoOpen>]
module ComponentRoot =
    let private getFitbitClient cfg =
        FitbitClient.defaultConfig cfg.ClientId cfg.ClientSecret
        |> FitbitClient.getClient

    let getCompRoot (config: AppConfig) =
        { FitbitClient = getFitbitClient config.Fitbit }

