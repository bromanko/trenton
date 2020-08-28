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
open Trenton.Common.Json

module Extensions =
    [<Literal>]
    let private Readiness = "readiness"

    [<Literal>]
    let private Liveness = "liveness"

    let private readinessTags = [ Readiness ]
    let private livenessTags = [ Liveness ]

    type IServiceCollection with
        member this.AddTrentonHealthChecks() =
            this.AddHttpClient() |> ignore
            this.AddHealthChecks().AddCheck<FitbitHealthCheck>
                ("fitbit", tags = readinessTags)

    type IApplicationBuilder with
        member private this.JsonWriter =
            Func<HttpContext, HealthReport, Task>(fun ctx report ->
                let body = report
                ctx.Response.ContentType <- MediaTypeNames.Application.Json

                let opts =
                    JsonSerializerOptions
                        (PropertyNamingPolicy = SnakeCaseNamingPolicy())

                JsonSerializer.SerializeAsync(ctx.Response.Body, body, opts))

        member this.UseTrentonHealthChecks(readinessPath, livenessPath) =
            this.UseHealthChecks
                (readinessPath,
                 HealthCheckOptions
                     (ResponseWriter = this.JsonWriter,
                      Predicate = fun check -> check.Tags.Contains Readiness))
            |> ignore
            this.UseHealthChecks
                (livenessPath,
                 HealthCheckOptions
                     (ResponseWriter = this.JsonWriter,
                      Predicate = fun check -> check.Tags.Contains Liveness))
