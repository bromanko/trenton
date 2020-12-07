namespace Trenton.Cli.Verbs.AuthFitbit

open Trenton.Health

module ConfigFileAuthRepository =
    let private tryGetAccessToken userId = None |> Async.result

    let private upsertAccessToken userId accessToken = Ok() |> Async.result

    let configFileAuthRepository =
        { FitbitAuthRepository.T.TryGetAccessToken = tryGetAccessToken
          FitbitAuthRepository.T.UpsertAccessToken = upsertAccessToken }

