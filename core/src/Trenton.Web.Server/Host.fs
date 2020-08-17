namespace Trenton.Web.Server

open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero.Server.RazorHost
open Bolero.Templating.Server
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Web.Server.Config
open Trenton.Web.Server.Health

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


    let private configureApp _ cfg (app: IApplicationBuilder) =
        app.UseSerilogRequestLogging()
           .UseTrentonHealthChecks(PathString "/healthz").UseStaticFiles()
           .UseRouting().UseBlazorFrameworkFiles()
           .UseEndpoints(fun endpoints ->
           if cfg.Server.IsDevelopment then endpoints.UseHotReload() |> ignore
           endpoints.MapBlazorHub() |> ignore
           endpoints.MapFallbackToPage("/_Host") |> ignore)
        |> ignore

    let createHostBuilder argv config compRoot =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseSerilog().ConfigureServices(configureServices config)
              .UseUrls(config.Server.Urls)
              .UseEnvironment(config.Server.Environment)
              .Configure(configureApp compRoot config)
            |> ignore)
