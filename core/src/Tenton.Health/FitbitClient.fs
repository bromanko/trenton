namespace Trenton.Health

module FitbitClient =
    type FitbitClientConfig =
        { BaseUrl: string }

    type FitbitAuthenticatedApi =
        { GetBodyFat: unit -> Async<Result<CustomerDto [], exn>> }

    type T =
        { GetAccessToken: string -> Async<Result<AccessTokenDto, exn>>
          AuthenticatedApi: string -> FitbitAuthenticatedApi }

    module private Impl =
        let getAccessToken code = ()

        let getBodyFat baseUrl accessToken = ()

    let getClient cfg =
        { GetAccessToken = fun code -> Impl.getAccessTokenDto cfg.BaseUrl code
          AuthenticatedApi = fun accessToken ->
              { GetBodyFat = fun () -> Impl.getBodyFatDto cfg.BaseUrl accessToken } }
