namespace Trenton.Cli.Verbs.Export.Fitbit

open Argu

open System.IO

open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Cli.FileHelpers
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
          OutputDirectory: FilePath.T
          Debug: bool }

    type ExecutionError =
        | Exception of exn
        | FitbitApiError of FitbitClient.FitbitApiError

    module private Parsing =

        let mkConfig
            debug
            clientId
            clientSecret
            accessToken
            refreshToken
            startDate
            endDate
            outDir
            =
            { Debug = debug
              ClientId = clientId
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
            mkConfig <!> (Ok true)
            <*> (parseNes "Client ID must be a valid string."
                 <| args.GetResult FitbitExportArgs.ClientId)
            <*> (parseNes "Client Secret must be a valid string."
                 <| args.GetResult FitbitExportArgs.ClientSecret)
            <*> (parseNes "Access Token must be a valid string."
                 <| args.GetResult FitbitExportArgs.AccessToken)
            <*> (parseOptionalNes "Refresh token must be a valid string."
                 <| args.TryGetResult FitbitExportArgs.RefreshToken)
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

        let getDatesInRange (startDate: Date.T) (endDate: Date.T) =
            let mutable currDate = startDate

            seq {
                while currDate <= endDate do
                    yield currDate
                    currDate <- Date.addDays currDate 1.0
            }

        type LogsForDate = { Date: Date.T; Logs: string }

        let getBodyWeightLogs
            (client: FitbitClient.FitbitAuthenticatedApi)
            date
            =
            { FitbitClient.BaseDate = date }
            |> client.Body.Raw.GetWeightLogs
            |> AsyncResult.map (fun r -> { Date = date; Logs = r })
            |> AsyncResult.mapError FitbitApiError

        let getBodyWeightLogsLogged log client date =
            $"Getting body weight logs for {date}" |> log
            getBodyWeightLogs client date

        let saveData outDir (data: LogsForDate) =
            let fName =
                Path.Join(
                    outDir,
                    getFilenameForDate "fitbit-body-weight" data.Date "json"
                )

            File.WriteAllTextAsync(fName, data.Logs)
            |> AsyncResult.ofTaskAction
            |> AsyncResult.mapError Exception

        let saveDataLogged log outDir data =
            $"Saving data for {data.Date}" |> log
            saveData outDir data

        let collectResults r =
            match Seq.tryFind Result.isError r with
            | Some _ ->
                ExecError.UnknownError "One or more errors occurred."
                |> Error
            | None -> Ok()

        let errStr =
            function
            | ExecutionError.Exception e -> e.Message
            | ExecutionError.FitbitApiError e ->
                match e with
                | FitbitClient.FitbitApiError.Error e -> e
                | FitbitClient.FitbitApiError.Exception e -> e.Message

        let export (console: #IConsole) cfg =
            let client =
                NonEmptyString.value cfg.AccessToken
                |> (mkClient cfg).Authenticated

            let dlog =
                if cfg.Debug then
                    console.Error.WriteLine
                else
                    fun _ -> ()

            getDatesInRange cfg.StartDate cfg.EndDate
            |> Seq.map (getBodyWeightLogsLogged dlog client)
            |> Seq.map (
                saveDataLogged dlog cfg.OutputDirectory.FullPath
                |> AsyncResult.bind
            )
            |> Seq.map (AsyncResult.teeError (fun e -> errStr e |> dlog))
            |> fun o -> Async.Parallel(o, 2)
            |> Async.map collectResults
            |> Async.RunSynchronously

    let Exec console args =
        Parsing.parse args >>= Execution.export console
