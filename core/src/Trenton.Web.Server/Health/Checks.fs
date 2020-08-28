namespace Trenton.Web.Server.Health

open System.Net.Http
open Microsoft.Extensions.Diagnostics.HealthChecks
open FSharp.Control.Tasks.V2.ContextInsensitive
open System
open Trenton.Health.FitbitClient

type FitbitHealthCheck(clientFactory: IHttpClientFactory) =
    interface IHealthCheck with
        member x.CheckHealthAsync(_, cxToken) =
            task {
                let client = clientFactory.CreateClient()
                let! resp = client.GetAsync(Uri FitbitApiBaseUrl, cxToken)

                match int resp.StatusCode with
                // 401 is expected since it is an authenticated API
                | r when r = 401 -> return HealthCheckResult.Healthy()
                | _ -> return HealthCheckResult.Degraded()
            }
