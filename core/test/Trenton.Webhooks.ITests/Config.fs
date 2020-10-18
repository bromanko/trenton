namespace Trenton.Webhooks.ITests

open FsConfig
open System

type ServerConfig =
    { Host: string }
    member this.AddressUri = Uri(sprintf "http://%s" this.Host)

type FitbitConfig = { SubscriberVerificationCode: string }

type LocationConfig = { AccessToken: string }

[<Convention("TEST")>]
type Config =
    { [<DefaultValue("false")>]
      LoggingEnabled: bool
      Server: ServerConfig
      Fitbit: FitbitConfig
      Location: LocationConfig }

module Config =
    let private failWithInvalidConfig error =
        match error with
        | NotFound envVarName ->
            failwithf "Environment variable %s not found" envVarName
        | BadValue (envVarName, value) ->
            failwithf
                "Environment variable %s has invalid value %s"
                envVarName
                value
        | NotSupported msg -> failwith msg

    let loadConfig () =
        match EnvConfig.Get<Config>() with
        | Ok config -> config
        | Error error -> failWithInvalidConfig error
