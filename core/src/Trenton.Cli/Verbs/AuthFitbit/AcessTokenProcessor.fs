namespace Trenton.Cli.Verbs.AuthFitbit

open System.Threading
open Trenton.Cli
open Trenton.Health
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult

type AccessTokenCode = { Code: string; RedirectUri: string }

type private AccessTokenMessage = AccessTokenRetrieved of AccessTokenCode

type AccessTokenProcessor(fitbitClient: FitbitClient.T,
                          cfg: AppConfig,
                          cts: CancellationTokenSource) =
    let getAccessToken code =
        (FitbitClient.AuthorizationCodeWithPkce
            { Code = code.Code
              RedirectUri = Some code.RedirectUri
              State = None
              CodeVerifier = None })
        |> fitbitClient.GetAccessToken

    let logResult =
        AsyncResult.foldResult
            (fun _ -> printfn "Your access token has been saved.")
            (fun e ->
                printfn "An unexpected error occurred."
                printfn ""
                printfn "%O" e)

    let getAndStoreAccessToken code =
        getAccessToken code
        |> logResult

    let body =
        (fun (inbox: MailboxProcessor<AccessTokenMessage>) ->
            let rec messageLoop () =
                async {
                    match! inbox.Receive() with
                    | AccessTokenRetrieved code ->
                        getAndStoreAccessToken code
                        |> Async.RunSynchronously

                        cts.Cancel()
                }

            messageLoop ())

    let agent =
        new MailboxProcessor<AccessTokenMessage>(body, cts.Token)

    member x.Start() = agent.Start()

    member x.Process code = AccessTokenRetrieved code |> agent.Post
