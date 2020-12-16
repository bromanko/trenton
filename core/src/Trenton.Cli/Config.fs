namespace Trenton.Cli

open System.IO
open System.Text.Json
open FsConfig
open FsToolkit.ErrorHandling.Operator.Result
open Microsoft.Extensions.Configuration
open System

type ConfigLoadError =
    | LoadException of exn
    | ParseError of ConfigParseError
    | NotFound

type FitbitAuthConfig =
    { AccessToken: string
      RefreshToken: string
      ExpiresInSeconds: int32 }

type FitbitConfig =
    { ClientId: string
      ClientSecret: string
      Auth: FitbitAuthConfig option }

type ServerConfig =
    { [<DefaultValue("9032")>]
      Port: int }

type AppConfig =
    { Fitbit: FitbitConfig
      Server: ServerConfig }

[<RequireQualifiedAccess>]
module Config =
    let DefaultConfigRoot =
        Path.Join [| (Environment.GetFolderPath
                          Environment.SpecialFolder.ApplicationData)
                     "trenton" |]

    [<Literal>]
    let DefaultConfigFilename = "config.json"

    let DefaultConfigPath =
        Path.Combine(DefaultConfigRoot, DefaultConfigFilename)

    let private loadConfigFile path =
        if File.Exists path then
            try
                ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName path)
                    .AddJsonFile(Path.GetFileName path, optional = true)
                    .Build()
                |> AppConfig
                |> Ok

            with e -> Error <| LoadException e
        else
            Error NotFound

    let parseConfig (c: FsConfig.AppConfig) =
        c.Get<AppConfig>() |> Result.mapError ParseError

    let load path = loadConfigFile path >>= parseConfig

    let save path cfg =
        try
            File.WriteAllText(path, JsonSerializer.Serialize cfg)
            |> Ok
        with e -> Error e
