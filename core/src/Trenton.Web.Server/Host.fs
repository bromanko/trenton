namespace Trenton.Web.Server

open System.IO
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero.Server.RazorHost
open Bolero.Templating.Server
open Bolero.Remoting.Server
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
        services.AddAuthorization() |> ignore
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
        |> ignore
        //        services.AddRemoting<>() |> ignore
        services.AddBoleroHost() |> ignore
        services.AddTrentonHealthChecks() |> ignore
        services.AddHttpContextAccessor() |> ignore

        if cfg.Server.IsDevelopment then
            services.AddHotReload
                (templateDir = __SOURCE_DIRECTORY__
                 + "/../Trenton.Web.Client")
            |> ignore

        services.AddSingleton<Trenton.Web.Client.Pages.Main.Config>
            ({ Trenton.Web.Client.Pages.Main.Config.IsDevelopment =
                   cfg.Server.IsDevelopment })
        |> ignore


    let private configureApp _ cfg (app: IApplicationBuilder) =
        app.UseAuthentication() |> ignore
        app.UseRemoting() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseBlazorFrameworkFiles() |> ignore
        app.UseEndpoints(fun endpoints ->
            if cfg.Server.IsDevelopment then endpoints.UseHotReload() |> ignore
            endpoints.MapBlazorHub() |> ignore
            endpoints.MapFallbackToPage("/_Host") |> ignore)
        |> ignore
        app.UseSerilogRequestLogging() |> ignore
        app.UseTrentonHealthChecks(PathString "/healthz")
        |> ignore

    let createHostBuilder argv config compRoot =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseContentRoot(Directory.GetCurrentDirectory()) |> ignore
            wb.UseStaticWebAssets() |> ignore
            wb.UseSerilog() |> ignore
            wb.ConfigureServices(configureServices config)
            |> ignore
            wb.UseUrls(config.Server.Urls) |> ignore
            wb.UseEnvironment(config.Server.Environment)
            |> ignore
            wb.Configure(configureApp compRoot config)
            |> ignore)
