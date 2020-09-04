namespace Trenton.Webhooks.Server

open FsConfig
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open Trenton.Common
open Trenton.Common.Config

module Config =
    type LoggingConfig =
        { [<DefaultValue("Information")>]
          LogLevel: LogLevel
          [<DefaultValue("Json")>]
          LogTarget: LogTarget }

    type ServerConfig =
        { [<DefaultValue("true")>]
          IsDevelopment: bool
          [<DefaultValue("0.0.0.0")>]
          Address: string
          [<DefaultValue("5000")>]
          HttpPort: int
          HttpsPort: int option }

        member this.Environment =
            if this.IsDevelopment then "Development" else "Deployed"

        member this.Urls =
            let httpUrl =
                [| sprintf "http://%s:%d" this.Address this.HttpPort |]

            match this.HttpsPort with
            | None -> httpUrl
            | Some port ->
                Array.append
                    httpUrl
                    [| sprintf "https://%s:%d" this.Address port |]

    type FitbitSubscriberConfig =
        { VerificationCode: string }

    type FitbitConfig =
        { BaseUrl: string option
          ClientId: string
          ClientSecret: string
          Subscriber: FitbitSubscriberConfig }

    type FirestoreEmulatorConfig = { Host: string }

    type FirestoreConfig =
        | Emulator of FirestoreEmulatorConfig
        | Cloud

    type GoogleCloudConfig =
        { ProjectId: string
          [<DefaultValue("Emulator")>]
          Firestore: FirestoreConfig }

    [<Convention("TRENTON_WEBHOOKS")>]
    type AppConfig =
        { Logging: LoggingConfig
          Server: ServerConfig
          Fitbit: FitbitConfig
          GoogleCloud: GoogleCloudConfig }

    let private envConfigGetOrFail (key: string) =
        EnvConfig.Get<'T>(key)
        |> Result.valueOr failInvalidConfig

    let private loadFirestoreConfig config =
        match config.GoogleCloud.Firestore with
        | Cloud -> Ok config
        | Emulator _ ->
            let host =
                envConfigGetOrFail
                    "TRENTON_WEBHOOKS_GOOGLE_CLOUD_FIRESTORE_EMULATOR_HOST"

            let gcc =
                { config.GoogleCloud with
                      Firestore = Emulator { Host = host } }

            Ok { config with GoogleCloud = gcc }

    let loadAppConfig () =
        EnvConfig.Get<AppConfig>()
        >>= loadFirestoreConfig
        |> Result.valueOr failInvalidConfig
