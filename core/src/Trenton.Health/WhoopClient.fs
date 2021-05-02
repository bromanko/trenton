namespace Trenton.Health

open FSharp.Data
open FSharp.Data.Runtime.BaseTypes
open Hopac
open HttpFs.Client
open System
open Trenton.Common
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

    type WhoopUser = { Id: int }

    type AccessToken = string

    type AccessTokenDto =
        { AccessToken: AccessToken
          ExpiresInSeconds: int
          RefreshToken: string
          User: WhoopUser }

    type GetCyclesRequest =
        { UserId: int
          StartDate: DateTime
          EndDate: DateTime }

    type HeartRateGranularity =
        | SixSeconds
        | SixtySeconds
        | SixHundredSeconds
        member x.Value =
            match x with
            | SixSeconds -> 6
            | SixtySeconds -> 60
            | SixHundredSeconds -> 600

    type GetHeartRateRequest =
        { UserId: int
          Granularity: HeartRateGranularity
          StartDate: DateTime
          EndDate: DateTime }

    type GetSleepRequest =
        { UserId: int
          SleepId: int }

    type GetSleepSurveyResponseRequest =
        { UserId: int
          SleepId: int }

    type GetWorkoutRequest =
        { UserId: int
          WorkoutId: int }

    type GetWorkoutSurveyResponseRequest =
        { UserId: int
          WorkoutId: int }

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

            $"%s{config.BaseUrl}/oauth/token"
            |> Http.post
            |> Http.jsonBody body
            |> execReq

        let getCycles config accessToken (req: GetCyclesRequest) =
            $"%s{config.BaseUrl}/users/{req.UserId}/cycles"
            |> HttpUtils.appendQueryToUrl ["start", req.StartDate.ToString()
                                           "end", req.EndDate.ToString()]
            |> Http.get
            |> Http.setAuthHeader accessToken
            |> execReq

        let getHeartRate config accessToken (req: GetHeartRateRequest) =
            $"%s{config.BaseUrl}/users/{req.UserId}/metrics/heart_rate"
            |> HttpUtils.appendQueryToUrl [ "step", req.Granularity.Value.ToString()
                                            "start", req.StartDate.ToString()
                                            "end", req.EndDate.ToString() ]
            |> Http.get
            |> Http.setAuthHeader accessToken
            |> execReq

        let getSleep config accessToken (req: GetSleepRequest) =
            $"%s{config.BaseUrl}/users/{req.UserId}/sleeps/${req.SleepId}"
            |> Http.get
            |> Http.setAuthHeader accessToken
            |> execReq

        let getSleepSurveyResponse config accessToken (req: GetSleepSurveyResponseRequest) =
            $"%s{config.BaseUrl}/users/{req.UserId}/sleeps/{req.SleepId}/survey-response"
            |> Http.get
            |> Http.setAuthHeader accessToken
            |> execReq

        let getWorkout config accessToken (req: GetWorkoutRequest) =
            $"%s{config.BaseUrl}/users/{req.UserId}/workouts/{req.WorkoutId}"
            |> Http.get
            |> Http.setAuthHeader accessToken
            |> execReq

        let getWorkoutSurveyResponses config accessToken (req: GetWorkoutSurveyResponseRequest) =
            $"%s{config.BaseUrl}/users/{req.UserId}/workouts/{req.WorkoutId}/survey-response"
            |> Http.get
            |> Http.setAuthHeader accessToken
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
              RefreshToken = resp.RefreshToken
              User = { Id = resp.User.Id } }

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

    let private getCycles config accessToken req =
        Api.getCycles config accessToken req
        |> Job.map Parse.rawResponse
        |> toAsync

    let private getHeartRate config accessToken req =
        Api.getHeartRate config accessToken req
        |> Job.map Parse.rawResponse
        |> toAsync

    let private getSleep config accessToken req =
        Api.getSleep config accessToken req
        |> Job.map Parse.rawResponse
        |> toAsync

    let private getSleepSurveyResponse config accessToken req =
        Api.getSleepSurveyResponse config accessToken req
        |> Job.map Parse.rawResponse
        |> toAsync

    let private getWorkout config accessToken req =
        Api.getWorkout config accessToken req
        |> Job.map Parse.rawResponse
        |> toAsync

    let private getWorkoutSurveyResponse config accessToken req =
        Api.getWorkoutSurveyResponses config accessToken req
        |> Job.map Parse.rawResponse
        |> toAsync

    type AuthenticatedApi =
        { GetCycles: GetCyclesRequest -> Async<Result<string, WhoopApiError>>
          GetHeartRate: GetHeartRateRequest -> Async<Result<string, WhoopApiError>>
          GetSleep: GetSleepRequest -> Async<Result<string, WhoopApiError>>
          GetSleepSurveyResponse: GetSleepSurveyResponseRequest -> Async<Result<string, WhoopApiError>>
          GetWorkout: GetWorkoutRequest -> Async<Result<string, WhoopApiError>>
          GetWorkoutSurveyResponse: GetWorkoutSurveyResponseRequest -> Async<Result<string, WhoopApiError>> }

    type T =
        { GetAccessToken: OAuthTokenRequest -> Async<Result<AccessTokenDto, WhoopApiError>>
          Authenticated: AccessToken -> AuthenticatedApi }

    let getClient cfg =
        { T.GetAccessToken = getAccessToken cfg
          Authenticated = fun at ->
              { GetCycles = getCycles cfg at
                GetHeartRate = getHeartRate cfg at
                GetSleep= getSleep cfg at
                GetSleepSurveyResponse= getSleepSurveyResponse cfg at
                GetWorkout= getWorkout cfg at
                GetWorkoutSurveyResponse= getWorkoutSurveyResponse cfg at } }
