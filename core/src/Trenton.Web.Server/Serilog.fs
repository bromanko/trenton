namespace Trenton.Web.Server

open System
open System.Net
open Serilog
open Serilog.Events
open Serilog.Formatting.Compact
open Serilog.Sinks.SystemConsole.Themes
open Trenton.Common
open Trenton.Web.Server.Config

module Serilog =
    [<Literal>]
    let private consoleTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}<t:{TraceId}><s:{SpanId}><r:{RequestId}>{NewLine}{Exception}"

    let private setFormat format (lc: LoggerConfiguration) =
        match format with
        | LiterateConsole ->
            lc.WriteTo.Console
                (outputTemplate = consoleTemplate,
                 theme = AnsiConsoleTheme.Literate)
        | Json ->
            lc.WriteTo.Console(RenderedCompactJsonFormatter())

    let private logEventLevel =
        function
        | Verbose -> LogEventLevel.Verbose
        | Debug -> LogEventLevel.Debug
        | Information -> LogEventLevel.Information
        | Warning -> LogEventLevel.Warning
        | Error -> LogEventLevel.Error
        | Fatal -> LogEventLevel.Fatal

    let private setMinLevel level (lc: LoggerConfiguration) =
        match level with
        | Verbose -> lc.MinimumLevel.Verbose()
        | Debug -> lc.MinimumLevel.Debug()
        | Information -> lc.MinimumLevel.Information()
        | Warning -> lc.MinimumLevel.Warning()
        | Error -> lc.MinimumLevel.Error()
        | Fatal -> lc.MinimumLevel.Fatal()

    let private destructureAppConfig =
        Func<AppConfig, obj>(fun config ->
            {| Server = config.Server
               GoogleCloudConfig = config.GoogleCloud |} :> obj)

    let getSerilog format (minLevel: LogLevel) =
        let lc =
            LoggerConfiguration()
                .Destructure.FSharpTypes()
                .Destructure.ByTransforming(destructureAppConfig)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("App", "Trenton Server")
                .Enrich.WithProperty("Host", Dns.GetHostName())
            |> setFormat format
            |> setMinLevel minLevel
        lc.CreateLogger()

