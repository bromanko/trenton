namespace Trenton.Cli.Verbs.Export.Fitbit

open Argu

open System.IO

open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Health
open Trenton.Common

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module Execution =
    type private ExportConfig =
        { ClientId: NonEmptyString.T
          ClientSecret: NonEmptyString.T
          AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T option
          StartDate: Date.T
          EndDate: Date.T
          OutputDirectory: FilePath.T }

    type ExecutionError =
        | Exception of exn
        | FitbitApiError of FitbitClient.FitbitApiError

    module private Parsing =

        let mkConfig clientId
                     clientSecret
                     accessToken
                     refreshToken
                     startDate
                     endDate
                     outDir
                     =
            { ClientId = clientId
              ClientSecret = clientSecret
              AccessToken = accessToken
              RefreshToken = refreshToken
              StartDate = startDate
              EndDate = endDate
              OutputDirectory = outDir }

        let parseEndDate d =
            match d with
            | None -> Date.today () |> Ok
            | Some d -> parseDate "End date must be a valid date." d


        let parse (args: ParseResults<FitbitExportArgs>) =
            mkConfig
            <!> (parseNes "Client ID must be a valid string."
                 <| args.GetResult FitbitExportArgs.ClientId)
            <*> (parseNes "Client Secret must be a valid string."
                 <| args.GetResult FitbitExportArgs.ClientSecret)
            <*> (parseNes "Access Token must be a valid string."
                 <| args.GetResult AccessToken)
            <*> (parseOptionalNes "Refresh token must be a valid string."
                 <| args.TryGetResult RefreshToken)
            <*> (parseDate "Start date must be a valid date."
                 <| args.GetResult StartDate)
            <*> (parseEndDate <| args.TryGetResult EndDate)
            <*> (parsePath "The output directory must be a valid path."
                 <| args.GetResult OutputDirectory)

    module private Execution =
        let mkClient cfg =
            FitbitClient.defaultConfig
                (NonEmptyString.value cfg.ClientId)
                (NonEmptyString.value cfg.ClientSecret)
            |> FitbitClient.getClient

        let getDates (startDate: Date.T) (endDate: Date.T) =
            printfn "%O %O" startDate endDate
            let mutable currDate = startDate

            seq {
                while currDate <= endDate do
                    printfn "%O" currDate
                    yield currDate
                    currDate <- Date.addDays currDate 1.0
            }

        let getBodyWeightLogs (client: FitbitClient.FitbitAuthenticatedApi) date =
            { FitbitClient.BaseDate = date }
            |> client.Body.Raw.GetWeightLogs
            |> AsyncResult.mapError FitbitApiError

        let saveData outDir data =
            File.WriteAllTextAsync(outDir, data)
            |> AsyncResult.ofTaskAction
            |> AsyncResult.mapError Exception

        let exToExErr =
            function
            | Exception e -> ExecError.Exception e
            | FitbitApiError e ->
                match e with
                | FitbitClient.Exception e -> ExecError.Exception e
                | FitbitClient.Error e -> ExecError.UnknownError e

        let export (console: #IConsole) cfg =
            let client =
                NonEmptyString.value cfg.AccessToken
                |> (mkClient cfg).Authenticated

            getDates cfg.StartDate cfg.EndDate
            |> Seq.map (getBodyWeightLogs client)
            |> Seq.map
                (AsyncResult.bind
                 <| saveData cfg.OutputDirectory.FullPath)
            |> Async.Parallel
            |> Async.map Seq.head
            |> AsyncResult.mapError exToExErr
            |> Async.RunSynchronously

    let Exec console args =
        Parsing.parse args >>= Execution.export console