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

    [<Literal>]
    let FitbitApiBaseUrl = "https://api.fitbit.com"

    let defaultConfig clientId clientSecret =
        { BaseUrl = FitbitApiBaseUrl
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

    type IntrospectTokenRequest =
        { Token: string }

    type AccessTokenDto =
        { AccessToken: string
          ExpiresInSeconds: int
          RefreshToken: string }

    type ActiveTokenStateDto =
        { UserId: string }

    type TokenStateDto =
        | Active of ActiveTokenStateDto
        | Inactive

    module private Http =
        let authHeader clientId clientSecret =
            sprintf "%s:%s" clientId clientSecret
            |> Encoding.UTF8.GetBytes
            |> Convert.ToBase64String
            |> sprintf "Basic %s"

        let userAuthHeader accessToken =
            sprintf "Bearer %s" accessToken

        let setAuthHeader (config: FitbitClientConfig) =
            Request.setHeader
                (Authorization(authHeader config.ClientId config.ClientSecret))

        let setUserAuthHeader accessToken =
            userAuthHeader accessToken
            |> Authorization
            |> Request.setHeader

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

        type IntrospectTokenResponseTypeProvider =
            JsonProvider<"Samples/introspect_token_response.json", EmbeddedResource="Trenton.Health, introspect_token_response.json", SampleIsList=true>

        type IntrospectTokenResponse = IntrospectTokenResponseTypeProvider.Root

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

        let introspectToken config accessToken req =
            sprintf "%s/oauth2/introspect" config.BaseUrl
            |> Http.httpPost
            |> Http.setUserAuthHeader accessToken
            |> Request.body (BodyForm [ NameValue("token", req.Token) ])
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

        let introspectTokenRespToDto (resp: Api.IntrospectTokenResponse) =
            match resp.Active with
            | false -> Inactive
            | true ->
                match resp.UserId with
                | None -> Inactive
                | Some uId -> Active { UserId = uId }

        let parseTokenState =
            function
            | Ok r ->
                Api.IntrospectTokenResponseTypeProvider.Parse r.Body
                |> introspectTokenRespToDto
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

    let private introspectToken
        config
        accessToken
        req
        : Async<Result<TokenStateDto, FitbitApiError>>
        =
        Api.introspectToken config accessToken req
        |> Job.map Parse.parseTokenState
        |> toAsync

    type FitbitAuthenticatedApi =
        { IntrospectToken: IntrospectTokenRequest -> Async<Result<TokenStateDto, FitbitApiError>> }
    //        { GetBodyFat: unit -> Async<Result<CustomerDto [], exn>> }

    type T =
        { GetAccessToken: AccessTokenRequest -> Async<Result<AccessTokenDto, FitbitApiError>>
          RefreshAccessToken: RefreshAccessTokenRequest -> Async<Result<AccessTokenDto, FitbitApiError>>
          Authenticated: string -> FitbitAuthenticatedApi }

    let getClient cfg =
        { T.GetAccessToken = getAccessToken cfg
          T.RefreshAccessToken = refreshAccessToken cfg
          T.Authenticated =
              fun accessToken ->
                  { IntrospectToken = introspectToken cfg accessToken } }
