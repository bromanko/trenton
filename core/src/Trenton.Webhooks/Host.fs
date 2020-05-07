namespace Trenton.Webhooks

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

module Host =
    let private route (path: PathString) =
        Giraffe.Routing.route path.Value

    let private webApp =
        choose [ GET >=> choose [ route <| Paths.Index() >=> text "index" ] ]

    let private addHealthChecks (services: IServiceCollection) =
        services.AddHealthChecks() |> ignore

    let private configureServices (services: IServiceCollection) =
        //        if config.Server.Development then
        //            IdentityModelEventSource.ShowPII <- true

        services.AddHttpContextAccessor() |> ignore
        addHealthChecks services
        ()

    let private configureApp =
        fun (app: IApplicationBuilder) ->
            app.UseHealthChecks(Paths.Healthz()).UseGiraffe(webApp)

    let createHostBuilder argv =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.ConfigureServices(configureServices).Configure(configureApp)
            |> ignore)
