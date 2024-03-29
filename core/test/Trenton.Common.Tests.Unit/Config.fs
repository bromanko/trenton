namespace Trenton.Common.Tests.Unit

open FsConfig

[<Convention("TEST")>]
type Config = {
    [<DefaultValue("false")>]
    LoggingEnabled: bool
}

module Config =
    let private failWithInvalidConfig error =
        match error with
            | NotFound envVarName ->
                failwithf "Environment variable %s not found" envVarName
            | BadValue(envVarName, value) ->
                failwithf "Environment variable %s has invalid value %s" envVarName value
            | NotSupported msg -> failwith msg

    let loadConfig () =
        match EnvConfig.Get<Config>() with
        | Ok config -> config
        | Error error -> failWithInvalidConfig error
