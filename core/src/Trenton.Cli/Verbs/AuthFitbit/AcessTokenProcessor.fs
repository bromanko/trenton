namespace Trenton.Cli.Verbs.AuthFitbit

open System.Threading
open Trenton.Health
open Trenton.Iam

type AccessTokenCode = { Code: string; RedirectUri: string }

type private AccessTokenMessage = AccessTokenRetrieved of AccessTokenCode

type AccessTokenProcessor(fitBitSvc: FitbitService.T,
                          cts: CancellationTokenSource) =
    let userId = (UserId.create "bromanko").Value

    let getAndStoreAccessToken code =
        fitBitSvc.GetAndStoreAccessToken userId code.Code code.RedirectUri

    let body =
        (fun (inbox: MailboxProcessor<AccessTokenMessage>) ->
            let rec messageLoop () =
                async {
                    match! inbox.Receive() with
                    | AccessTokenRetrieved code ->
                        match! getAndStoreAccessToken code with
                        | Error e -> printfn "An error has occurred.\n\n%O" e
                        | Ok _ -> printfn "Access token saved."
                        cts.Cancel()
                }

            messageLoop ())

    let agent =
        new MailboxProcessor<AccessTokenMessage>(body, cts.Token)

    member x.Start() = agent.Start()

    member x.Process code = AccessTokenRetrieved code |> agent.Post
