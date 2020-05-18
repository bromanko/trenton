namespace Trenton.Health

open FSharp.Data
open Hopac
open HttpFs.Client
open System
open System.Text
open Trenton.Common.ApiClient

module FitbitClient =
    type FitbitClientConfig =
        { BaseUrl: string
          ClientId: string
          ClientSecret: string }

    type FitbitApiError =
        | Error of string
        | Exception of exn

    let defaultConfig clientId clientSecret =
        { BaseUrl = "https://api.fitbit.com"
          ClientId = clientId
          ClientSecret = clientSecret }

    type AuthorizationCodeAccessTokenRequest =
        { Code: string
          RedirectUri: string option
          State: string option }

    type AuthorizationCodeWithPkceAccessTokenRequest =
        { Code: string
          RedirectUri: string option
          State: string option
          CodeVerifier: string option }

    type AccessTokenRequest =
        | AuthorizationCode of AuthorizationCodeAccessTokenRequest
        | AuthorizationCodeWithPkce of AuthorizationCodeWithPkceAccessTokenRequest

    type RefreshAccessTokenRequest =
        { RefreshToken: string }

    type AccessTokenDto =
        { AccessToken: string
          ExpiresInSeconds: int
          RefreshToken: string }

    module private Http =
        let authHeader clientId clientSecret =
            sprintf "%s:%s" clientId clientSecret
            |> Encoding.UTF8.GetBytes
            |> Convert.ToBase64String
            |> sprintf "Basic %s"

        let setAuthHeader (config: FitbitClientConfig) =
            Request.setHeader
                (Authorization(authHeader config.ClientId config.ClientSecret))

        let httpPost url =
            Uri url
            |> Request.create Post
            |> Request.setHeader (Accept HttpContentTypes.Json)

        let optionToString =
            function
            | None -> null
            | Some (s: string) -> s

    module private Api =
        type ErrorResponseTypeProvider =
            JsonProvider<"Samples/error_response.json", EmbeddedResource="Trenton.Health, error_response.json">

        type ErrorResponse = ErrorResponseTypeProvider.Root

        type AccessTokenResponseTypeProvider =
            JsonProvider<"Samples/get_access_token_response.json", EmbeddedResource="Trenton.Health, get_access_token_response.json">

        type AccessTokenResponse = AccessTokenResponseTypeProvider.Root

        let getAccessTokenForm clientId req =
            match req with
            | AuthorizationCode r ->
                [ "grant_type", "authorization_code"
                  "code", r.Code
                  "client_id", clientId
                  "redirect_uri", Http.optionToString r.RedirectUri
                  "state", Http.optionToString r.State
                  "expires_in", "28800" ]
            | AuthorizationCodeWithPkce r ->
                [ "grant_type", "authorization_code"
                  "code", r.Code
                  "client_id", clientId
                  "redirect_uri", Http.optionToString r.RedirectUri
                  "state", Http.optionToString r.State
                  "code_verifier", Http.optionToString r.CodeVerifier
                  "expires_in", "28800" ]
            |> List.map (fun (k, v) -> NameValue(k, v))

        let getAccessToken config req =
            let form = getAccessTokenForm config.ClientId req

            sprintf "%s/oauth2/token" config.BaseUrl
            |> Http.httpPost
            |> Http.setAuthHeader config
            |> Request.body (BodyForm form)
            |> execReq

        let refreshAccessTokenForm (req: RefreshAccessTokenRequest) =
            [ "grant_type", "refresh_token"
              "refresh_token", req.RefreshToken
              "expires_in", "28800" ]
            |> List.map (fun (k, v) -> NameValue(k, v))

        let refreshAccessToken config req =
            let form =
                refreshAccessTokenForm req

            sprintf "%s/oauth2/token" config.BaseUrl
            |> Http.httpPost
            |> Http.setAuthHeader config
            |> Request.body (BodyForm form)
            |> execReq

    module private Parse =
        let accessTokenRespToDto (resp: Api.AccessTokenResponse) =
            { AccessToken = resp.AccessToken
              ExpiresInSeconds = resp.ExpiresIn
              RefreshToken = resp.RefreshToken }

        let firstErrorMessage (resp: Api.ErrorResponse) =
            let first = resp.Errors |> Seq.head
            first.Message

        let parseAccessToken =
            function
            | Ok r ->
                Api.AccessTokenResponseTypeProvider.Parse r.Body
                |> accessTokenRespToDto
                |> Result.Ok
            | ApiResponse.Error r ->
                Api.ErrorResponseTypeProvider.Parse r.Body
                |> firstErrorMessage
                |> FitbitApiError.Error
                |> Result.Error

    let toAsync job =
        async {
            let! resp =
                job
                |> Job.catch
                |> Job.toAsync
            return match resp with
                   | Choice1Of2 r -> r
                   | Choice2Of2 err ->
                       Exception err |> Result.Error
        }

    let private getAccessToken
        config
        req
        : Async<Result<AccessTokenDto, FitbitApiError>>
        =
        Api.getAccessToken config req
        |> Job.map Parse.parseAccessToken
        |> toAsync

    let private refreshAccessToken config req =
        Api.refreshAccessToken config req
        |> Job.map Parse.parseAccessToken
        |> toAsync

    //    type FitbitAuthenticatedApi =
    //        { GetBodyFat: unit -> Async<Result<CustomerDto [], exn>> }

    type T =
        { GetAccessToken: AccessTokenRequest -> Async<Result<AccessTokenDto, FitbitApiError>>
          RefreshAccessToken: RefreshAccessTokenRequest -> Async<Result<AccessTokenDto, FitbitApiError>> }

    let getClient cfg =
        { GetAccessToken = fun req -> getAccessToken cfg req
          RefreshAccessToken = fun req -> refreshAccessToken cfg req }
