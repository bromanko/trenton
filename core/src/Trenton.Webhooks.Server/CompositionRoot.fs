namespace Trenton.Webhooks.Server

open Trenton.Health
open Trenton.Location
open Trenton.Webhooks.Server.Config
open Google.Cloud.Firestore
open Google.Cloud.Storage.V1
open System
open Grpc.Core

type CompositionRoot =
    { FitbitService: FitbitService.T
      GoogleCloudStorageClient: StorageClient
      LocationService: LocationService.T }

[<AutoOpen>]
module CompositionRoot =
    let private getFitbitClient cfg =
        FitbitClient.defaultConfig cfg.ClientId cfg.ClientSecret
        |> FitbitClient.getClient

    let private getCloudStorageClient _ = StorageClientBuilder().Build()

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
        let csClient = getCloudStorageClient config.GoogleCloud

        { FitbitService = FitbitService.defaultSvc fitbitClient fitbitAuthRepo
          GoogleCloudStorageClient = csClient
          LocationService =
              LocationService.gcsSvc csClient config.Location.BucketName }
