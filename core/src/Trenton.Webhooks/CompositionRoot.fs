namespace Trenton.Webhooks

open Trenton.Health
open Trenton.Webhooks.Config

type CompositionRoot =
    { FitbitClient: FitbitClient.T }

[<AutoOpen>]
module ComponentRoot =
    let private getFitbitClient _ =
        FitbitClient.getClient FitbitClient.defaultConfig

    let getCompRoot (config: AppConfig) =
        { FitbitClient = getFitbitClient config }

