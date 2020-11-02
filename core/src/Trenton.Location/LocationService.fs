namespace Trenton.Location

open FsToolkit.ErrorHandling
open Google.Cloud.Storage.V1
open System.IO

module LocationService =
    type T =
        { StoreLocationData: string -> Stream -> Async<Result<unit, exn>> }

    let private storeLocationData (client: StorageClient) bucket fName stream =
        client.UploadObjectAsync(bucket, fName, "text/json", stream)
        |> AsyncResult.ofTask
        |> AsyncResult.foldResult (fun _ -> Ok()) (fun e ->
               Result.Error e)

    let gcsSvc client bucket =
        { T.StoreLocationData = storeLocationData client bucket }
