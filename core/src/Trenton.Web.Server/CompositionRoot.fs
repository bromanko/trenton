namespace Trenton.Web.Server

open Trenton.Web.Server.Config
open Trenton.Health
open System
open Google.Cloud.Firestore
open Grpc.Core

type CompositionRoot = { FitbitService: FitbitService.T }

[<AutoOpen>]
module CompositionRoot =
    let private getFitbitClient cfg =
        FitbitClient.defaultConfig cfg.ClientId cfg.ClientSecret
        |> FitbitClient.getClient

    let private getNow () = DateTime.Now

    let private firestoreDbBuilder cfg =
        match cfg.Firestore with
        | Emulator fb ->
            FirestoreDbBuilder
                (ProjectId = cfg.ProjectId,
                 Endpoint = fb.Host,
                 ChannelCredentials = ChannelCredentials.Insecure)
        | Cloud -> FirestoreDbBuilder(ProjectId = cfg.ProjectId)

    let private getFitbitAuthRepo cfg =
        FitbitAuthRepository.firestoreAuthRepository
            (firestoreDbBuilder cfg)
            getNow

    let defaultRoot (config: AppConfig) =
        let fitbitClient = getFitbitClient config.Fitbit
        let fitbitAuthRepo = getFitbitAuthRepo config.GoogleCloud
        { FitbitService = FitbitService.defaultSvc fitbitClient fitbitAuthRepo }
