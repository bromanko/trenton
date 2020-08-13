namespace Trenton.Web.Server

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.Cookies
open Serilog
open Trenton.Web.Server.Config
open Trenton.Web.Server.Health
open Bolero.Server.RazorHost
open Bolero.Templating.Server

module Host =
    let private configureServices cfg (services: IServiceCollection) =
        services.AddMvc().AddRazorRuntimeCompilation()
        |> ignore
        services.AddServerSideBlazor() |> ignore
        services.AddTrentonHealthChecks() |> ignore
        services.AddAuthorization()
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie().Services.AddBoleroHost().AddHttpContextAccessor()
        |> ignore

        if cfg.Server.IsDevelopment then
            services.AddHotReload
                (templateDir = __SOURCE_DIRECTORY__
                 + "/../Trenton.Web.Client")
            |> ignore


    let private configureApp _ _ (app: IApplicationBuilder) =
        app.UseSerilogRequestLogging()
           .UseTrentonHealthChecks(PathString "/healthz")
        |> ignore

    let createHostBuilder argv config compRoot =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseSerilog().ConfigureServices(configureServices config)
              .UseUrls(config.Server.Urls)
              .UseEnvironment(config.Server.Environment)
              .Configure(configureApp compRoot config)
            |> ignore)
