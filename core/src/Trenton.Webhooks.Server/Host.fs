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
open Trenton.Webhooks.Server.Health.Extensions

module Host =
    let private webApp compRoot cfg =
        choose [ Routes.Index.handler
                 Routes.Fitbit.VerifySubscriber.handler cfg.Fitbit.Subscriber
                 Routes.Fitbit.Webhook.handler compRoot.FitbitService
                 Routes.Location.Webhook.handler
                     cfg.Location.AccessToken
                     compRoot.LocationService ]

    let private configureServices compRoot cfg (services: IServiceCollection) =
        services.AddTrentonHealthChecks(compRoot, cfg)
        |> ignore
        services.AddGiraffe().AddHttpContextAccessor().AddSingleton<IJsonSerializer>
            (Utf8JsonSerializer(Utf8JsonSerializer.DefaultResolver))
        |> ignore

    let private configureApp compRoot cfg =
        fun (app: IApplicationBuilder) ->
            app.UseSerilogRequestLogging()
               .UseTrentonHealthChecks(PathString Routes.Paths.Healthz.Readiness,
                                       PathString Routes.Paths.Healthz.Liveness)
               .UseGiraffeErrorHandler(giraffeErrHandler)
               .UseGiraffe(webApp compRoot cfg)

    let createHostBuilder argv config compRoot =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseSerilog().ConfigureServices(configureServices compRoot config)
              .UseUrls(config.Server.Urls)
              .UseEnvironment(config.Server.Environment)
              .Configure(configureApp compRoot config)
            |> ignore)
