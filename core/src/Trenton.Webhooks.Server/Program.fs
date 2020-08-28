namespace Trenton.Webhooks.Server

open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Common.Config
open Trenton.Webhooks.Server.Config
open Trenton.Webhooks.Server.Host

module Program =
    [<EntryPoint>]
    let main argv =
        let config = loadAppConfig ()
        Log.Logger <-
            getSerilog config.Logging.LogTarget config.Logging.LogLevel
        Log.Logger.Information("Configured with {@AppConfig}", config)

        try
            try
                let compRoot = getCompRoot config
                (createHostBuilder argv config compRoot).Build().Run()
            with ex -> Log.Logger.Fatal(ex, "Server startup failed")
        finally
            Log.CloseAndFlush()
        0
