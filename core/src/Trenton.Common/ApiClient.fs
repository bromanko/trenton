namespace Trenton.Common

open Hopac
open HttpFs.Client

module ApiClient =
    type ApiResponseDetails = { StatusCode: int; Body: string }

    type ApiResponse = Result<ApiResponseDetails, ApiResponseDetails>

    let private mapToApiResponse (resp: Response) =
        Response.readBodyAsString resp
        |> Job.map (fun b ->
            match resp.statusCode with
            | x when x < 300 -> Ok
            | _ -> Error
            <| { Body = b
                 StatusCode = resp.statusCode })

    let execReq req =
        req |> getResponse |> Job.bind mapToApiResponse
