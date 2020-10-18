namespace Trenton.Location

open FsToolkit.ErrorHandling

module LocationService =
    type Error = Exception of exn

    type T =
        { StoreLocationData: unit -> Async<Result<unit, Error>> }

    let private storeLocationData () = asyncResult { return! Ok() }

    let defaultSvc =
        { T.StoreLocationData = storeLocationData }
