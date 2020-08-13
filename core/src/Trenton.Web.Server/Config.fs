namespace Trenton.Web.Server

open FsConfig
open Trenton.Common

module Config =
    type LoggingConfig =
        { [<DefaultValue("Information")>]
          LogLevel: LogLevel
          [<DefaultValue("Json")>]
          LogTarget: LogTarget }

    type ServerConfig =
        { [<DefaultValue("true")>]
          IsDevelopment: bool
          [<DefaultValue("0.0.0.0")>]
          Address: string
          [<DefaultValue("5002")>]
          HttpPort: int
          HttpsPort: int option }

        member this.Environment =
            if this.IsDevelopment then "Development" else "Deployed"

        member this.Urls =
            let httpUrl =
                [| sprintf "http://%s:%d" this.Address this.HttpPort |]
            match this.HttpsPort with
            | None -> httpUrl
            | Some port ->
                Array.append httpUrl
                    [| sprintf "https://%s:%d" this.Address port |]

    [<Convention("TRENTON_WEB")>]
    type AppConfig =
        { Logging: LoggingConfig
          Server: ServerConfig }
