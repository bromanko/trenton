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
open Trenton.Web.Server.Health

module Host =
    let private route (path: PathString) = Routing.route path.Value

    let private webApp _ _ = choose [ Routes.Dashboard.handler ]

    let private configureServices _
                                  _
                                  (context: WebHostBuilderContext)
                                  (services: IServiceCollection)
                                  =
//        services.AddMvc().AddRazorRuntimeCompilation()
//        |> ignore

        services.AddGiraffe() |> ignore

        Path.Combine(context.HostingEnvironment.ContentRootPath, "Views")
        |> services.AddRazorEngine
        |> ignore

        services.AddAuthorization() |> ignore
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
        |> ignore

        services.AddTrentonHealthChecks() |> ignore
        services.AddHttpContextAccessor() |> ignore


    let private configureApp compRoot cfg (app: IApplicationBuilder) =
        app.UseAuthentication() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseSerilogRequestLogging() |> ignore
        app.UseTrentonHealthChecks(PathString "/healthz")
        |> ignore
        app.UseGiraffeErrorHandler(giraffeErrHandler cfg.Server)
        |> ignore
        app.UseGiraffe(webApp compRoot cfg) |> ignore

    let createHostBuilder argv config compRoot =
        let contentRoot = Directory.GetCurrentDirectory()

        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.UseContentRoot(contentRoot) |> ignore
            wb.UseWebRoot(Path.Combine(contentRoot, "WebRoot"))
            |> ignore
            wb.UseStaticWebAssets() |> ignore
            wb.UseSerilog() |> ignore
            wb.ConfigureServices(configureServices compRoot config)
            |> ignore
            wb.UseUrls(config.Server.Urls) |> ignore
            wb.UseEnvironment(config.Server.Environment)
            |> ignore
            wb.Configure(configureApp compRoot config)
            |> ignore)
