namespace Trenton.Cli

open System.IO
open FsConfig
open FsToolkit.ErrorHandling.Operator.Result
open Microsoft.Extensions.Configuration
open System

type FitbitConfig =
    { ClientId: string
      ClientSecret: string
      AccessToken: string option
      RefreshToken: string option }

type AppConfig = { Fitbit: FitbitConfig }

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
