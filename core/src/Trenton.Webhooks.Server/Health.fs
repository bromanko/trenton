namespace Trenton.Webhooks.Server

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open System
open System.Net.Mime
open System.Threading.Tasks
open Giraffe

module Health =
    type IServiceCollection with
        member this.AddTrentonHealthChecks() = this.AddHealthChecks()

    type IApplicationBuilder with

        member private this.JsonWriter =
            Func<HttpContext, HealthReport, Task>(fun ctx report ->
                let body = report
                ctx.Response.ContentType <- MediaTypeNames.Application.Json
                ctx.WriteJsonAsync body :> Task)

        member this.UseTrentonHealthChecks(path) =
            this.UseHealthChecks
                (path, HealthCheckOptions(ResponseWriter = this.JsonWriter))
