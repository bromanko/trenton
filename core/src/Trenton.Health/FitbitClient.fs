namespace Trenton.Health

open FSharp.Data
open Hopac
open HttpFs.Client
open System
open System.Text
open Trenton.Common
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
        | AuthorizationCodeWithPkce of
            AuthorizationCodeWithPkceAccessTokenRequest

    type RefreshAccessTokenRequest = { RefreshToken: string }

    type IntrospectTokenRequest = { Token: string }

    type AccessTokenDto =
        { AccessToken: string
          ExpiresInSeconds: int
          RefreshToken: string }

    type ActiveTokenStateDto = { UserId: string }

    type TokenStateDto =
        | Active of ActiveTokenStateDto
        | Inactive

    type GetBodyWeightLogsRequest = { BaseDate: Date.T }

    type BodyWeightLogDto = { Bmi: decimal }

    module private Http =
        let authHeader clientId clientSecret =
            $"%s{clientId}:%s{clientSecret}"
            |> Encoding.UTF8.GetBytes
            |> Convert.ToBase64String
            |> sprintf "Basic %s"

        let userAuthHeader accessToken = $"Bearer %s{accessToken}"

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

        let httpGet url =
            Uri url
            |> Request.create Get
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

        type BodyWeightLogResponseTypeProvider =
            JsonProvider<"Samples/get_weight_logs_response.json", EmbeddedResource="Trenton.Health, get_weight_logs_response.json">

        type BodyWeightLogResponse = BodyWeightLogResponseTypeProvider.Root

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
            |> List.map NameValue

        let getAccessToken config req =
            let form = getAccessTokenForm config.ClientId req

            $"%s{config.BaseUrl}/oauth2/token"
            |> Http.httpPost
            |> Http.setAuthHeader config
            |> Request.body (BodyForm form)
            |> execReq

        let refreshAccessTokenForm (req: RefreshAccessTokenRequest) =
            [ "grant_type", "refresh_token"
              "refresh_token", req.RefreshToken
              "expires_in", "28800" ]
            |> List.map NameValue

        let refreshAccessToken config req =
            let form = refreshAccessTokenForm req

            $"%s{config.BaseUrl}/oauth2/token"
            |> Http.httpPost
            |> Http.setAuthHeader config
            |> Request.body (BodyForm form)
            |> execReq

        let introspectToken config accessToken req =
            $"%s{config.BaseUrl}/1.1/oauth2/introspect"
            |> Http.httpPost
            |> Http.setUserAuthHeader accessToken
            |> Request.body (BodyForm [ NameValue("token", req.Token) ])
            |> execReq

        let getBodyWeightLogs config accessToken (date: Date.T) =
            date.ToString("s")
            |> sprintf "%s/1/user/-/body/log/weight/date/%s.json" config.BaseUrl
            |> Http.httpGet
            |> Http.setUserAuthHeader accessToken
            |> execReq


    module private Parse =
        let mapDto mapFn op =
            Job.map mapFn op
            |> Job.catch
            |> Job.map (function
                | Choice1Of2 o -> Result.Ok o
                | Choice2Of2 e -> Result.Error e)
            |> Job.toAsync

        let accessTokenRespToDto (resp: Api.AccessTokenResponse) =
            { AccessToken = resp.AccessToken
              ExpiresInSeconds = resp.ExpiresIn
              RefreshToken = resp.RefreshToken }

        let firstErrorMessage (resp: Api.ErrorResponse) =
            let first = resp.Errors |> Seq.head
            first.Message

        let parseApiError r =
            Api.ErrorResponseTypeProvider.Parse r.Body
            |> firstErrorMessage
            |> FitbitApiError.Error

        let rawResponse r: Result<string, FitbitApiError> =
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

        let introspectTokenRespToDto (resp: Api.IntrospectTokenResponse) =
            match resp.Active with
            | false -> Inactive
            | true ->
                match resp.UserId with
                | None -> Inactive
                | Some uId -> Active { UserId = uId }

        let tokenState =
            function
            | Ok r ->
                Api.IntrospectTokenResponseTypeProvider.Parse r.Body
                |> introspectTokenRespToDto
                |> Ok
            | Result.Error r -> parseApiError r |> Result.Error

        let bodyWeightLogsRespToDto (resp: Api.BodyWeightLogResponse) =
            Array.map (fun (l: Api.BodyWeightLogResponseTypeProvider.Weight) ->
                { Bmi = l.Bmi }) resp.Weight

        let bodyWeightLogs =
            function
            | Ok r ->
                Api.BodyWeightLogResponseTypeProvider.Parse r.Body
                |> bodyWeightLogsRespToDto
                |> Result.Ok
            | Result.Error r -> parseApiError r |> Result.Error

    let private toAsync job =
        Job.catch job
        |> Job.map (function
            | Choice1Of2 r -> r
            | Choice2Of2 ex -> Exception ex |> Result.Error)
        |> Job.toAsync

    let private getAccessToken config req =
        Api.getAccessToken config req
        |> Job.map Parse.accessToken
        |> toAsync

    let private refreshAccessToken config req =
        Api.refreshAccessToken config req
        |> Job.map Parse.accessToken
        |> toAsync

    let private introspectToken config accessToken req =
        Api.introspectToken config accessToken req
        |> Job.map Parse.tokenState
        |> toAsync

    let private getBodyWeightLogs cfg accessToken parseFn req =
        Api.getBodyWeightLogs cfg accessToken req.BaseDate
        |> Job.map parseFn
        |> toAsync

    type BodyApi =
        { GetWeightLogs: GetBodyWeightLogsRequest -> Async<Result<BodyWeightLogDto [], FitbitApiError>>
          Raw: {| GetWeightLogs: GetBodyWeightLogsRequest -> Async<Result<string, FitbitApiError>> |} }

    type FitbitAuthenticatedApi =
        { IntrospectToken: IntrospectTokenRequest -> Async<Result<TokenStateDto, FitbitApiError>>
          Body: BodyApi }

    type T =
        { GetAccessToken: AccessTokenRequest -> Async<Result<AccessTokenDto, FitbitApiError>>
          RefreshAccessToken: RefreshAccessTokenRequest -> Async<Result<AccessTokenDto, FitbitApiError>>
          Authenticated: string -> FitbitAuthenticatedApi }

    let getClient cfg =
        { T.GetAccessToken = getAccessToken cfg
          T.RefreshAccessToken = refreshAccessToken cfg
          T.Authenticated =
              fun accessToken ->
                  { IntrospectToken = introspectToken cfg accessToken
                    Body =
                        { GetWeightLogs =
                              getBodyWeightLogs
                                  cfg
                                  accessToken
                                  Parse.bodyWeightLogs
                          Raw =
                              {| GetWeightLogs =
                                     getBodyWeightLogs
                                         cfg
                                         accessToken
                                         Parse.rawResponse |} } } }
