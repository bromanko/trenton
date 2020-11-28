namespace Trenton.Cli

open System.IO
open FsConfig
open FsToolkit.ErrorHandling
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

    let private getAppConfig path filename =
        if File.Exists <| Path.Combine(path, filename) then
            ConfigurationBuilder().SetBasePath(path)
                .AddJsonFile(filename, optional = true).Build()
            |> AppConfig
            |> Some
        else
            None

    let load path filename =
        getAppConfig path filename
        |> Option.traverseResult (fun c -> c.Get<AppConfig>())
