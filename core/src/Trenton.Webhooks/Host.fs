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

    let private webApp =
        choose
            [ GET
              >=> choose [ route <| Paths.index () >=> Routes.Meta.index () ] ]

    let private addHealthChecks (services: IServiceCollection) =
        services.AddTrentonHealthChecks() |> ignore

    let private configureServices (services: IServiceCollection) =
        services.AddHttpContextAccessor() |> ignore
        addHealthChecks services
        services.AddSingleton<IJsonSerializer>
            (Utf8JsonSerializer(Utf8JsonSerializer.DefaultResolver)) |> ignore
        ()

    let private configureApp =
        fun (app: IApplicationBuilder) ->
            app.UseSerilogRequestLogging()
               .UseTrentonHealthChecks(Paths.health ()).UseGiraffe(webApp)

    let createHostBuilder argv config =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseSerilog().ConfigureServices(configureServices)
              .UseUrls(config.Server.Urls)
              .UseEnvironment(config.Server.Environment).Configure(configureApp)
            |> ignore)
