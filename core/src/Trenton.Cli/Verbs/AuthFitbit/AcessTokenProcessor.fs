namespace Trenton.Cli.Verbs.AuthFitbit

open System.Threading
open Trenton.Health

type private AccessTokenMessage = AccessTokenRetrieved of string

type AccessTokenProcessor(fitBitSvc: FitbitService.T,
                          cts: CancellationTokenSource) =
    let body =
        (fun (inbox: MailboxProcessor<AccessTokenMessage>) ->
            let rec messageLoop () =
                printfn "looping fitbit"
                async {
                    match! inbox.Receive() with
                    | AccessTokenRetrieved code ->
                        printfn "got access token"
                        cts.Cancel()
                }

            messageLoop ())

    let agent =
        new MailboxProcessor<AccessTokenMessage>(body, cts.Token)

    member x.Start() = agent.Start()

    member x.Process code = AccessTokenRetrieved code |> agent.Post
