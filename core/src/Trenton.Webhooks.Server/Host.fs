namespace Trenton.Webhooks.Server

open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Webhooks.Server.Config
open Trenton.Webhooks.Server.Health

module Host =
    let private webApp compRoot cfg =
        choose [ Routes.Index.handler
                 Routes.Fitbit.VerifySubscriber.handler cfg.Fitbit.Subscriber
                 Routes.Fitbit.Webhook.handler compRoot.FitbitService ]

    let private configureServices (services: IServiceCollection) =
        services.AddTrentonHealthChecks() |> ignore
        services.AddGiraffe().AddHttpContextAccessor().AddSingleton<IJsonSerializer>
            (Utf8JsonSerializer(Utf8JsonSerializer.DefaultResolver))
        |> ignore

    let private configureApp compRoot cfg =
        fun (app: IApplicationBuilder) ->
            app.UseSerilogRequestLogging()
               .UseTrentonHealthChecks(PathString "/healthz")
               .UseGiraffeErrorHandler(giraffeErrHandler cfg.Server)
               .UseGiraffe(webApp compRoot cfg)

    let createHostBuilder argv config compRoot =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseSerilog().ConfigureServices(configureServices)
              .UseUrls(config.Server.Urls)
              .UseEnvironment(config.Server.Environment)
              .Configure(configureApp compRoot config)
            |> ignore)
