namespace Trenton.Webhooks

open FsConfig

module Config =
    type LoggingConfig =
        { [<DefaultValue("Information")>]
          LogLevel: LogLevel
          [<DefaultValue("Json")>]
          LogTarget: LogTarget }

    type ServerConfig =
        { [<DefaultValue("true")>]
          Development: bool
          [<DefaultValue("0.0.0.0")>]
          Address: string
          [<DefaultValue("5000")>]
          HttpPort: int
          [<DefaultValue("5001")>]
          HttpsPort: int }

        member this.Environment =
            if this.Development then "Development" else "Deployed"

        member this.Urls =
            [| sprintf "http://%s:%d" this.Address this.HttpPort
               sprintf "https://%s:%d" this.Address this.HttpsPort |]

    [<Convention("TRENTON")>]
    type AppConfig =
        { Logging: LoggingConfig
          Server: ServerConfig }

    let failInvalidConfig error =
        match error with
        | NotFound envVarName ->
            failwithf "Environment variable %s not found" envVarName
        | BadValue (envVarName, value) ->
            failwithf "Environment variable %s has invalid value %s" envVarName
                value
        | NotSupported msg -> failwith msg

    let loadAppConfig () =
        match EnvConfig.Get<AppConfig>() with
        | Ok config -> config
        | ConfigParseResult.Error error -> failInvalidConfig error
