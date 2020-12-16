namespace Trenton.Cli.Verbs.AuthFitbit

open System.Threading
open Trenton.Cli
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

type AccessTokenCode = { Code: string; RedirectUri: string }

type private AccessTokenMessage = AccessTokenRetrieved of AccessTokenCode

type private ProcessingError =
    | FitbitApiError of string
    | ConfigLoadError of ConfigLoadError
    | Exception of exn

type AccessTokenProcessor(fitbitClient: FitbitClient.T,
                          cfgPath,
                          cts: CancellationTokenSource) =
    let getAccessToken code =
        (FitbitClient.AuthorizationCodeWithPkce
            { Code = code.Code
              RedirectUri = Some code.RedirectUri
              State = None
              CodeVerifier = None })
        |> fitbitClient.GetAccessToken
        |> AsyncResult.mapError (function
            | FitbitClient.Error e -> FitbitApiError e
            | FitbitClient.Exception e -> Exception e)

    let loadConfig cfgPath =
        Config.load cfgPath
        |> Result.mapError ConfigLoadError

    let saveConfig cfgPath cfg =
        Config.save cfgPath cfg
        |> Result.mapError ProcessingError.Exception

    let updateConfig (dto: FitbitClient.AccessTokenDto) (cfg: AppConfig) =
        { cfg with
              Fitbit =
                  { cfg.Fitbit with
                        Auth =
                            Some
                                { FitbitAuthConfig.AccessToken = dto.AccessToken
                                  RefreshToken = dto.RefreshToken
                                  ExpiresInSeconds = dto.ExpiresInSeconds } } }
        |> Ok

    let saveAccessToken dto =
        loadConfig cfgPath
        >>= updateConfig dto
        >>= saveConfig cfgPath

    let logResult =
        Result.fold
            (fun _ -> eprintfn "Your access token has been saved.")
            (fun e ->
                eprintfn "An unexpected error occurred."
                eprintfn ""
                eprintfn "%O" e)

    let getAndStoreAccessToken code =
        getAccessToken code
        |> Async.RunSynchronously
        >>= saveAccessToken
        |> logResult

    let body =
        (fun (inbox: MailboxProcessor<AccessTokenMessage>) ->
            let rec messageLoop () =
                async {
                    match! inbox.Receive() with
                    | AccessTokenRetrieved code ->
                        getAndStoreAccessToken code

                        cts.Cancel()
                }

            messageLoop ())

    let agent =
        new MailboxProcessor<AccessTokenMessage>(body, cts.Token)

    member x.Start() = agent.Start()

    member x.Process code = AccessTokenRetrieved code |> agent.Post
