namespace Trenton.Web.Server

open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Common.Config
open Trenton.Web.Server.Config
open Trenton.Web.Server.Host

module Program =
    [<EntryPoint>]
    let main argv =
        let config = loadAppConfig<AppConfig> ()
        Log.Logger <-
            getSerilog config.Logging.LogTarget config.Logging.LogLevel
        Log.Logger.Information("Configured with {@AppConfig}", config)

        try
            try
                let compRoot = defaultRoot config
                (createHostBuilder argv config compRoot).Build().Run()
            with ex -> Log.Logger.Fatal(ex, "Server startup failed")
        finally
            Log.CloseAndFlush()
        0
