namespace Trenton.Webhooks.Server.Health

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open System
open System.Net.Mime
open System.Threading.Tasks
open Giraffe
open Trenton.Webhooks.Server
open Trenton.Webhooks.Server.Config

module Extensions =
    [<Literal>]
    let private Readiness = "readiness"

    [<Literal>]
    let private Liveness = "liveness"

    let private readinessTags = [ Readiness ]
    let private livenessTags = [ Liveness ]

    type IServiceCollection with
        member this.AddTrentonHealthChecks(compRoot: CompositionRoot,
                                           cfg: AppConfig) =
            let csCheck =
                Checks.googleCloudStorage
                    compRoot.GoogleCloudStorageClient
                    cfg.Location.BucketName

            this.AddHealthChecks()
                .AddAsyncCheck("googleCloudStorage",
                               csCheck,
                               tags = Seq.append readinessTags livenessTags)

    type IApplicationBuilder with

        member private this.JsonWriter =
            Func<HttpContext, HealthReport, Task>(fun ctx report ->
                let body = report
                ctx.Response.ContentType <- MediaTypeNames.Application.Json
                ctx.WriteJsonAsync body :> Task)

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
