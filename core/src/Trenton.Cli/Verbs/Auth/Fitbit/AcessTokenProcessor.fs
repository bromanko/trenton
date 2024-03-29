namespace Trenton.Cli.Verbs.Auth.Fitbit

open System.Threading
open Trenton.Cli
open Trenton.Cli.LogFormatters
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

type AccessTokenCode = { Code: string; RedirectUri: string }

type private AccessTokenMessage = AccessTokenRetrieved of AccessTokenCode

type private ProcessingError =
    | FitbitApiError of string
    | Exception of exn

type AccessTokenProcessor(fitbitClient: FitbitClient.T,
                          console: IConsole,
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

    let getAndStoreAccessToken code =
        getAccessToken code
        |> Async.RunSynchronously
        |> Result.fold (logDto console) (logError console)

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
