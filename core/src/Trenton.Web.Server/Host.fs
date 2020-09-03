namespace Trenton.Web.Server

open System.IO
open Giraffe
open Giraffe.Razor
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Serilog
open Trenton.Web.Server
open Trenton.Web.Server.Config
open Trenton.Web.Server.Health.Extensions
open Westwind.AspNetCore.LiveReload

module Host =
    let private contentRootPath = Directory.GetCurrentDirectory()

    let private webRootPath = Path.Combine(contentRootPath, "WebRoot")

    let private webApp compRoot cfg =
        choose [ Routes.Dashboard.View.handler
                 Routes.Health.View.handler
                 Routes.Fitbit.AuthCallback.handler compRoot.FitbitService
                 Routes.Settings.View.handler cfg compRoot.FitbitService ]

    let private isWebRootFile = fun f -> String.startsWith webRootPath f

    let private configureServices _ cfg _ (services: IServiceCollection) =
        if cfg.Server.IsDevelopment then
            services.AddLiveReload(fun rc ->
                rc.FolderToMonitor <- contentRootPath
                rc.FileIncludeFilter <- System.Func<string, bool> isWebRootFile
                rc.ClientFileExtensions <-
                    String.concat "," [ ".css"; ".js"; ".cshtml" ])
            |> ignore

        services.AddGiraffe() |> ignore
        services.AddRazorEngine webRootPath |> ignore
        services.AddAuthorization() |> ignore
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
        |> ignore

        services.AddTrentonHealthChecks() |> ignore
        services.AddHttpContextAccessor() |> ignore


    let private configureApp compRoot cfg (app: IApplicationBuilder) =
        app.UseLiveReload() |> ignore
        app.UseAuthentication() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseSerilogRequestLogging() |> ignore
        app.UseTrentonHealthChecks
            (PathString "/healthz/readiness", PathString "/healthz/liveness")
        |> ignore
        app.UseGiraffeErrorHandler(giraffeErrHandler cfg.Server)
        |> ignore
        app.UseGiraffe(webApp compRoot cfg) |> ignore

    let createHostBuilder argv config compRoot =

        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseContentRoot(contentRootPath) |> ignore
            wb.UseWebRoot(webRootPath) |> ignore
            wb.UseStaticWebAssets() |> ignore
            wb.UseSerilog() |> ignore
            wb.ConfigureServices(configureServices compRoot config)
            |> ignore
            wb.UseUrls(config.Server.Urls) |> ignore
            wb.UseEnvironment(config.Server.Environment)
            |> ignore
            wb.Configure(configureApp compRoot config)
            |> ignore)
