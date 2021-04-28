namespace Trenton.Health

open FSharp.Data
open FSharp.Data.Runtime.BaseTypes
open Hopac
open HttpFs.Client
open System
open Trenton.Common.ApiClient

module WhoopClient =
    type WhoopClientConfig = { BaseUrl: string }

    type WhoopApiError =
        | Error of string
        | Exception of exn

    [<Literal>]
    let WhoopApiBaseUrl = "https://api-7.whoop.com"

    let defaultConfig = { BaseUrl = WhoopApiBaseUrl }

    type OAuthTokenPasswordRequest =
        { Username: string
          Password: string
          IssueRefresh: bool }

    type OAuthTokenRefreshTokenRequest = { RefreshToken: string }

    type OAuthTokenRequest =
        | Password of OAuthTokenPasswordRequest
        | RefreshToken of OAuthTokenRefreshTokenRequest

    type AccessTokenDto =
        { AccessToken: string
          ExpiresInSeconds: int
          RefreshToken: string }

    module private Http =
        let private userAuthHeader accessToken = $"Bearer %s{accessToken}"

        let setAuthHeader accessToken =
            userAuthHeader accessToken
            |> Authorization
            |> Request.setHeader

        let post url =
            Uri url
            |> Request.create Post
            |> Request.setHeader (Accept HttpContentTypes.Json)

        let get url =
            Uri url
            |> Request.create Get
            |> Request.setHeader (Accept HttpContentTypes.Json)

        let optionToString =
            function
            | None -> null
            | Some (s: string) -> s

        let jsonBody (body: IJsonDocument) req =
            req
            |> Request.setHeader (
                ContentType(ContentType.create ("application", "json"))
            )
            |> Request.body (BodyString(body.JsonValue.ToString()))

    module private Api =
        type ErrorResponseTypeProvider =
            JsonProvider<"Samples/whoop/error_response.json">

        type ErrorResponse = ErrorResponseTypeProvider.Root

        type AccessTokenPasswordRequestTypeProvider =
            JsonProvider<"Samples/whoop/get_access_token_password_request.json">

        type AccessTokenPasswordRequest =
            AccessTokenPasswordRequestTypeProvider.Root

        type AccessTokenPasswordRefreshTypeProvider =
            JsonProvider<"Samples/whoop/get_access_token_refresh_request.json">

        type AccessTokenRefreshRequest =
            AccessTokenPasswordRefreshTypeProvider.Root

        type AccessTokenResponseTypeProvider =
            JsonProvider<"Samples/whoop/get_access_token_response.json">

        type AccessTokenResponse = AccessTokenResponseTypeProvider.Root

        let getOauthToken config req =
            let body =
                match req with
                | Password r ->
                    AccessTokenPasswordRequest(
                        r.Username,
                        r.Password,
                        "password",
                        r.IssueRefresh
                    )
                    :> IJsonDocument
                | RefreshToken r ->
                    AccessTokenRefreshRequest(r.RefreshToken, "refresh_token")
                    :> IJsonDocument

            $"%s{config.BaseUrl}/oauth2/token"
            |> Http.post
            |> Http.jsonBody body
            |> execReq

    module private Parse =
        let mapDto mapFn op =
            Job.map mapFn op
            |> Job.catch
            |> Job.map
                (function
                | Choice1Of2 o -> Result.Ok o
                | Choice2Of2 e -> Result.Error e)
            |> Job.toAsync

        let accessTokenRespToDto (resp: Api.AccessTokenResponse) =
            { AccessToken = resp.AccessToken
              ExpiresInSeconds = resp.ExpiresIn
              RefreshToken = resp.RefreshToken }

        let errorMessage (resp: Api.ErrorResponse) = resp.Message

        let parseApiError r =
            Api.ErrorResponseTypeProvider.Parse r.Body
            |> errorMessage
            |> WhoopApiError.Error

        let rawResponse r : Result<string, WhoopApiError> =
            match r with
            | Ok r -> r.Body |> Result.Ok
            | Result.Error r -> parseApiError r |> Result.Error

        let accessToken =
            function
            | Ok r ->
                Api.AccessTokenResponseTypeProvider.Parse r.Body
                |> accessTokenRespToDto
                |> Ok
            | Result.Error r -> parseApiError r |> Result.Error

    let private toAsync job =
        Job.catch job
        |> Job.map
            (function
            | Choice1Of2 r -> r
            | Choice2Of2 ex -> Exception ex |> Result.Error)
        |> Job.toAsync

    let private getAccessToken config req =
        Api.getOauthToken config req
        |> Job.map Parse.accessToken
        |> toAsync

    type T =
        { GetAccessToken: OAuthTokenRequest -> Async<Result<AccessTokenDto, WhoopApiError>> }

    let getClient cfg =
        { T.GetAccessToken = getAccessToken cfg }
