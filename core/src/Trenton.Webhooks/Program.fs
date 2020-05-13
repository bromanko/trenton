namespace Trenton.Webhooks

open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Webhooks.Config
open Trenton.Webhooks.Host

module Program =
    [<EntryPoint>]
    let main argv =
        let config = loadAppConfig ()
        Log.Logger <-
            getSerilog config.Logging.LogTarget config.Logging.LogLevel
        Log.Logger.Information("Configured with {@AppConfig}", config)

        try
            try
                (createHostBuilder argv config).Build().Run()
            with ex -> Log.Logger.Fatal(ex, "Server startup failed")
        finally
            Log.CloseAndFlush()
        0
