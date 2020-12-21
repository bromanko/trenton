namespace Trenton.Cli.Verbs.Auth.Fitbit

open System
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

type ServerConfig = { Port: int; LogLevel: LogLevel }

type CompositionRoot =
    { ProcessAccessToken: AccessTokenCode -> unit }

module Routes =
    let earlyReturn: HttpFunc = Some >> Task.FromResult

    let bindQueryOrErr<'T> f =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let result = ctx.TryBindQueryString<'T>()
            match result with
            | Ok o -> f o next ctx
            | Result.Error r ->
                (RequestErrors.badRequest <| text r) earlyReturn ctx

    module Fitbit =
        [<CLIMutable>]
        type Query = { code: string }


        let private getRedirectUri (req: HttpRequest) =
            let uri =
                Uri
                    (sprintf
                        "%s://%s%s"
                         req.Scheme
                         (req.Host.ToString())
                         (req.Path.ToString()))

            uri.ToString()

        let private processAccessToken (atProcessor: AccessTokenCode -> unit)
                                       qry
                                       =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task {
                    atProcessor
                        { Code = qry.code
                          RedirectUri = getRedirectUri ctx.Request }
                    return! next ctx
                }

        let authCallbackHandler atProcessor =
            GET
            >=> route "/fitbit/callback"
            >=> bindQueryOrErr<Query> (processAccessToken atProcessor)
            >=> Successful.ok
                    (htmlString
                        "<p>Authentication successful. You may close this browser window.</p>")


module Host =
    let private errorHandler (ex: Exception) (logger: ILogger) =
        logger.LogError
            (EventId(),
             ex,
             "An unhandled exception has occurred while executing the request.")
        clearResponse
        >=> ServerErrors.INTERNAL_ERROR(text ex.Message)

    let private webApp _ compRoot =
        choose [ Routes.Fitbit.authCallbackHandler compRoot.ProcessAccessToken ]

    let private configureServices _ (services: IServiceCollection) =
        services.AddGiraffe() |> ignore

    let private configureApp cfg compRoot (app: IApplicationBuilder) =
        app.UseRouting() |> ignore
        app.UseGiraffeErrorHandler(errorHandler) |> ignore
        app.UseGiraffe(webApp cfg compRoot)

    let createHostBuilder cfg compRoot =
        Host.CreateDefaultBuilder()
            .ConfigureLogging(fun l -> l.SetMinimumLevel cfg.LogLevel |> ignore)
            .ConfigureWebHostDefaults(fun wb ->
            wb.ConfigureServices(configureServices cfg)
            |> ignore
            wb.UseUrls(sprintf "http://localhost:%d" cfg.Port)
            |> ignore
            wb.Configure(configureApp cfg compRoot) |> ignore)
