namespace Trenton.Health

open Trenton.Health.FitbitAuthRepository
open Trenton.Health.FitbitClient
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open System
open Trenton.Iam

module FitbitService =
    type Error =
        | FitbitApiError of string
        | Exception of exn

    type T =
        { GetAndStoreAccessToken: UserId.T -> string -> string -> Async<Result<unit, Error>>
          TryGetLastTokenUpdateDate: UserId.T -> Async<DateTime option> }

    let private mapFitbitApiErr =
        function
        | FitbitApiError.Error e -> Error.FitbitApiError e
        | FitbitApiError.Exception e -> Error.Exception e

    let private getAccessToken fitbitClient code redirectUri =
        asyncResult {
            return! (AuthorizationCodeWithPkce
                         { Code = code
                           RedirectUri = Some redirectUri
                           State = None
                           CodeVerifier = None })
                    |> fitbitClient.GetAccessToken
                    |> AsyncResult.mapError mapFitbitApiErr
        }

    let private mapAuthRepositoryError =
        function
        | FitbitAuthRepositoryError.Exception e -> Error.Exception e

    let private upsertToken (repository: FitbitAuthRepository.T) token state =
        asyncResult {
            return! repository.UpsertAccessToken token state
                    |> AsyncResult.mapError mapAuthRepositoryError
        }

    let private getAndStoreAccessToken fitbitClient
                                       repository
                                       userId
                                       code
                                       redirectUri
                                       =
        asyncResult {
            return! getAccessToken fitbitClient code redirectUri
                    >>= upsertToken repository userId
        }

    let private tryGetLastTokenUpdateDate (repository: FitbitAuthRepository.T)
                                          userId
                                          =
        asyncOption {
            let! at = repository.TryGetAccessToken userId
            return at.UpdatedAt
        }

    let defaultSvc fitbitClient repository =
        { T.GetAndStoreAccessToken =
              getAndStoreAccessToken fitbitClient repository
          T.TryGetLastTokenUpdateDate = tryGetLastTokenUpdateDate repository }
