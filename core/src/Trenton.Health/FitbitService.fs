namespace Trenton.Health

open Trenton.Health.FitbitAuthRepository
open Trenton.Health.FitbitClient
open Trenton.Health
open FsToolkit.ErrorHandling

module FitbitService =
    type Error =
        | FitbitApiError of string
        | Exception of exn

    type T =
        { GetAndStoreAccessToken: string -> string -> Async<Result<unit, Error>> }

    let private mapFitbitApiResp res =
        match res with
        | Ok t -> Ok t
        | Result.Error err ->
            match err with
            | FitbitApiError.Error e -> Error.FitbitApiError e |> Result.Error
            | FitbitApiError.Exception e -> Error.Exception e |> Result.Error

    let private getAccessToken fitbitClient code redirectUri =
        async {
            let! res =
                AuthorizationCodeWithPkce
                    { Code = code
                      RedirectUri = Some redirectUri
                      State = None
                      CodeVerifier = None }
                |> fitbitClient.GetAccessToken

            return mapFitbitApiResp res
        }

    let private getTokenState fitbitClient token =
        async {
            let aClient = fitbitClient.Authenticated token
            let! res = aClient.IntrospectToken { Token = token }

            return match mapFitbitApiResp res with
                   | Result.Error e -> Result.Error e
                   | Ok r ->
                       match r with
                       | Active t -> Ok t
                       | Inactive _ ->
                           Error.FitbitApiError
                               "The token specified is inactive."
                           |> Result.Error
        }

    let private upsertToken (repository: FitbitAuthRepository.T) token state =
        async {
            let! res = repository.upsertAccessToken token state

            return match res with
                   | Ok _ -> Ok()
                   | Result.Error e ->
                       match e with
                       | FitbitAuthRepositoryError.Exception e ->
                           Error.Exception e |> Result.Error
        }

    let private getAndStoreAccessToken fitbitClient repository code redirectUri =
        asyncResult {
            let! token = getAccessToken fitbitClient code redirectUri
            let! state = getTokenState fitbitClient token.AccessToken
            return! upsertToken repository token state
        }

    let defaultSvc fitbitClient repository =
        { T.GetAndStoreAccessToken =
              getAndStoreAccessToken fitbitClient repository }
