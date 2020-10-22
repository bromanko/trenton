namespace Trenton.Location

open FsToolkit.ErrorHandling
open Google.Cloud.Storage.V1
open System.IO

module LocationService =
    type Error = Exception of exn

    type T =
        { StoreLocationData: string -> Stream -> Async<Result<unit, Error>> }

    let private storeLocationData (client: StorageClient) bucket fName stream =
        asyncResult {
            let! res =
                client.UploadObjectAsync(bucket, fName, "text/json", stream)
                |> Async.AwaitTask

            return! Ok()
        }

    let gcsSvc bucket =
        let client = StorageClient.Create()
        { T.StoreLocationData = storeLocationData client bucket }
