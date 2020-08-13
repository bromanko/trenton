namespace Trenton.Common

open FsConfig

module Config =
    let private failInvalidConfig error =
        match error with
        | NotFound envVarName ->
            failwithf "Environment variable %s not found" envVarName
        | BadValue (envVarName, value) ->
            failwithf "Environment variable %s has invalid value %s" envVarName
                value
        | NotSupported msg -> failwith msg

    let loadAppConfig<'TConfig when 'TConfig: not struct> () =
        match EnvConfig.Get<'TConfig>() with
        | Ok config -> config
        | ConfigParseResult.Error error -> failInvalidConfig error
