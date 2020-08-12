namespace Trenton.Webhooks.ITests

open FsConfig
open System

type ServerConfig =
    { Base: string
      Port: int }
    member this.AddressUri = Uri(sprintf "%s:%i" this.Base this.Port)

type FitbitConfig =
    { SubscriberVerificationCode: string }

type FirestoreConfig =
    { Host: string
      Project: string }

[<Convention("TEST")>]
type Config =
    { [<DefaultValue("false")>]
      LoggingEnabled: bool
      Server: ServerConfig
      Fitbit: FitbitConfig
      Firestore: FirestoreConfig }

module Config =
    let private failWithInvalidConfig error =
        match error with
        | NotFound envVarName ->
            failwithf "Environment variable %s not found" envVarName
        | BadValue (envVarName, value) ->
            failwithf "Environment variable %s has invalid value %s" envVarName
                value
        | NotSupported msg -> failwith msg

    let loadConfig () =
        match EnvConfig.Get<Config>() with
        | Ok config -> config
        | Error error -> failWithInvalidConfig error
