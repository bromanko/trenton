namespace Trenton.Web.Server

open Trenton.Web.Server.Config
open Trenton.Health

type CompositionRoot =
    { FitbitClient: FitbitClient.T }

[<AutoOpen>]
module ComponentRoot =
    let private getFitbitClient cfg =
        FitbitClient.defaultConfig cfg.ClientId cfg.ClientSecret
        |> FitbitClient.getClient

    let defaultRoot (config: AppConfig) =
        { FitbitClient = getFitbitClient config.Fitbit }

