namespace Trenton.Cli

open System.IO
open System.Text.Json
open FsConfig
open FsToolkit.ErrorHandling.Operator.Result
open Microsoft.Extensions.Configuration
open System

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

    let private loadConfigFile path filename =
        if File.Exists <| Path.Combine(path, filename) then
            ConfigurationBuilder().SetBasePath(path)
                .AddJsonFile(filename, optional = true).Build()
            |> AppConfig
            |> Ok
        else
            Error ConfigFileNotFound

    let parseConfig (c: FsConfig.AppConfig) =
        c.Get<AppConfig>()
        |> Result.mapError ConfigParseError

    let load path filename =
        loadConfigFile path filename >>= parseConfig

    let save cfg path =
        File.WriteAllText(path, JsonSerializer.Serialize cfg)
