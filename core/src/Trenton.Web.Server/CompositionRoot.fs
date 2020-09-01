namespace Trenton.Web.Server

open Trenton.Web.Server.Config
open Trenton.Health
open System
open Google.Cloud.Firestore

type CompositionRoot = { FitbitService: FitbitService.T }

[<AutoOpen>]
module ComponentRoot =
    let private getFitbitClient cfg =
        FitbitClient.defaultConfig cfg.ClientId cfg.ClientSecret
        |> FitbitClient.getClient

    let private getNow () = DateTime.Now

    let private getFitbitAuthRepo cfg =
        FitbitAuthRepository.firestoreAuthRepository
            (FirestoreDbBuilder(ProjectId = cfg.ProjectId))
            getNow

    let defaultRoot (config: AppConfig) =
        let fitbitClient = getFitbitClient config.Fitbit
        let fitbitAuthRepo = getFitbitAuthRepo config.GoogleCloud
        { FitbitService = FitbitService.defaultSvc fitbitClient fitbitAuthRepo }
