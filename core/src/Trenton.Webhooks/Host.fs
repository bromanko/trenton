namespace Trenton.Webhooks

open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Webhooks.Config
open Trenton.Webhooks.Health

module Host =
    let private route (path: PathString) =
        Giraffe.Routing.route path.Value

    let private webApp compRoot =
        choose
            [ Routes.Index.handler
              Routes.Fitbit.authCallbackHandler compRoot.FitbitClient ]

    let private addHealthChecks (services: IServiceCollection) =
        services.AddTrentonHealthChecks() |> ignore

    let private configureServices (services: IServiceCollection) =
        addHealthChecks services
        services
            .AddGiraffe()
            .AddHttpContextAccessor()
            .AddSingleton<IJsonSerializer>
            (Utf8JsonSerializer(Utf8JsonSerializer.DefaultResolver)) |> ignore
        ()

    let private configureApp compRoot =
        fun (app: IApplicationBuilder) ->
            app.UseSerilogRequestLogging()
               .UseTrentonHealthChecks(PathString "/healthz")
               .UseGiraffe(webApp compRoot)

    let createHostBuilder argv config compRoot =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseSerilog().ConfigureServices(configureServices)
              .UseUrls(config.Server.Urls)
              .UseEnvironment(config.Server.Environment)
              .Configure(configureApp compRoot) |> ignore)
