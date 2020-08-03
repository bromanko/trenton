namespace Trenton.Common

open Hopac
open HttpFs.Client

module ApiClient =
    type ApiResponseDetails =
        { StatusCode: int
          Body: string }

    type ApiResponse =
        | Ok of ApiResponseDetails
        | Error of ApiResponseDetails

    let private getApiResp resp =
        job {
            let! body = Response.readBodyAsString resp
            let details =
                { Body = body
                  StatusCode = resp.statusCode }
            match details.StatusCode with
            | x when x < 300 ->
                return Ok details
            | _ ->
                return Error details
        }

    let execReq req =
        req
        |> getResponse
        |> Job.bind getApiResp

