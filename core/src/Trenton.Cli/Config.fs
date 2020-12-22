namespace Trenton.Cli

open System.IO
open FsToolkit.ErrorHandling.Operator.Result
open Microsoft.Extensions.Configuration
open System
open FsConfig
open FsToolkit.ErrorHandling

type ConfigFileError =
    | LoadException of exn
    | ParseError of ConfigParseError
    | NotFound of string

type FitbitConfig =
    { ClientId: string
      ClientSecret: string }

type FitbitAuthConfig =
    { AccessToken: string
      RefreshToken: string }

type ServerConfig =
    { [<DefaultValue("9032")>]
      Port: int }

type AppConfig =
    { Fitbit: FitbitConfig
      FitbitAuth: FitbitAuthConfig option
      Server: ServerConfig }

[<RequireQualifiedAccess>]
module Config =
    let DefaultConfigRoot =
        Path.Join [| (Environment.GetFolderPath
                          Environment.SpecialFolder.ApplicationData)
                     "trenton" |]

    [<Literal>]
    let DefaultConfigFilename = "config.ini"

    let DefaultConfigPath =
        Path.Combine(DefaultConfigRoot, DefaultConfigFilename)

    let private loadConfigFile path =
        if File.Exists path then
            try
                ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName path)
                    .AddIniFile(Path.GetFileName path, optional = true)
                    .Build()
                |> AppConfig
                |> Ok

            with e -> LoadException e |> Error
        else
            NotFound path |> Error

    let private parseConfig (c: FsConfig.AppConfig) =
        c.Get<AppConfig>() |> Result.mapError ParseError

    let load path =
        loadConfigFile path
        >>= parseConfig
        |> Result.tee (printfn "%O")

//    let save path cfg =
//        try
//            serializeToFile path cfg
//            |> Ok
//        with e -> Error e
