namespace Trenton.Web.Server.Health

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open System
open System.Net.Mime
open System.Threading.Tasks
open System.Text.Json

module Extensions =
    type IServiceCollection with
        member this.AddTrentonHealthChecks() = this.AddHealthChecks()

    type IApplicationBuilder with

        member private this.JsonWriter =
            Func<HttpContext, HealthReport, Task>(fun ctx report ->
                let body = report
                ctx.Response.ContentType <- MediaTypeNames.Application.Json
                JsonSerializer.SerializeAsync(ctx.Response.Body, body))

        member this.UseTrentonHealthChecks(path) =
            this.UseHealthChecks
                (path, HealthCheckOptions(ResponseWriter = this.JsonWriter))
