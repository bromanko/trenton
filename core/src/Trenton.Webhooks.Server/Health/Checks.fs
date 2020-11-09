namespace Trenton.Webhooks.Server.Health

open System
open System.Threading
open Google
open Google.Cloud.Storage.V1
open Microsoft.Extensions.Diagnostics.HealthChecks
open FSharp.Control.Tasks.V2.ContextInsensitive

module Checks =
    let private (|ErrorWithCode|) (ex: Exception) =
        match ex with
        | :? GoogleApiException as ex ->
            match ex.Error with
            | null -> None
            | _ -> Some ex.Error.Code
        | _ -> None

    let googleCloudStorage (storageClient: StorageClient)
                           bucketName
                           (_: CancellationToken)
                           =
        task {
            try
                storageClient.ListObjects(bucketName)
                |> Seq.first
                |> ignore

                return HealthCheckResult.Healthy()
            with
            | ErrorWithCode (c) when c.IsSome && c.Value = 404 ->
                return HealthCheckResult.Unhealthy("Bucket not found")
            | ErrorWithCode (c) when c.IsSome && c.Value = 401 ->
                return HealthCheckResult.Unhealthy("Authentication error")
            | ex -> return HealthCheckResult.Degraded(ex.Message, ex)
        }
