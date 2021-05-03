namespace Trenton.Cli.Verbs.Export.Whoop

open Argu

open System.IO

open FsToolkit.ErrorHandling.Operator.AsyncResult
open Trenton.Cli
open Trenton.Cli.Verbs
open Trenton.Cli.FileHelpers
open Trenton.Health
open Trenton.Common

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

module Execution =
    type private ExportConfig =
        { UserId: int
          AccessToken: NonEmptyString.T
          RefreshToken: NonEmptyString.T option
          StartDate: Date.T
          EndDate: Date.T
          OutputDirectory: FilePath.T
          Debug: bool }

    type ExecutionError =
        | Exception of exn
        | WhoopApiError of WhoopClient.WhoopApiError

    module private Parsing =
        let mkConfig
            debug
            accessToken
            refreshToken
            userId
            startDate
            endDate
            outDir
            =
            { Debug = debug
              AccessToken = accessToken
              RefreshToken = refreshToken
              UserId = userId
              StartDate = startDate
              EndDate = endDate
              OutputDirectory = outDir }

        let parseEndDate d =
            match d with
            | None -> Date.today () |> Ok
            | Some d -> parseDate "End date must be a valid date." d


        let parse (args: ParseResults<WhoopExportArgs>) =
            mkConfig (GlobalConfig.ParseArgs args).Debug
            <!> (parseNes "Access Token must be a valid string."
                 <| args.GetResult WhoopExportArgs.AccessToken)
            <*> (parseOptionalNes "Refresh token must be a valid string."
                 <| args.TryGetResult WhoopExportArgs.RefreshToken)
            <*> (args.GetResult WhoopExportArgs.UserId |> Ok)
            <*> (parseDate "Start date must be a valid date."
                 <| args.GetResult StartDate)
            <*> (parseEndDate <| args.TryGetResult EndDate)
            <*> (parsePath "The output directory must be a valid path."
                 <| args.GetResult OutputDirectory)

    module private Execution =
        let getDatesInRange (startDate: Date.T) (endDate: Date.T) =
            let mutable currDate = startDate

            seq {
                while currDate <= endDate do
                    yield currDate
                    currDate <- Date.addDays currDate 1.0
            }

        type LogsForDate = { Date: Date.T; Logs: string }

        let getCycles
            (client: WhoopClient.AuthenticatedApi)
            userId
            (startDate: Date.T)
            (endDate: Date.T)
            =
            { WhoopClient.UserId = userId
              WhoopClient.StartDate = startDate.ToDateTime()
              WhoopClient.EndDate = endDate.ToDateTime() }
            |> client.GetCycles
            |> AsyncResult.mapError WhoopApiError

        let getCyclesLogged log client userId s e =
            $"Getting cycles for User {userId} for range {s} {e}"
            |> log

            getCycles client userId s e

        let saveData cfg data =
            let fName =
                Path.Join(
                    cfg.OutputDirectory.FullPath,
                    getFilenameForDateRange $"whoop-cycles-{cfg.UserId}" cfg.StartDate cfg.EndDate "json"
                )

            File.WriteAllTextAsync(fName, data)
            |> AsyncResult.ofTaskAction
            |> AsyncResult.mapError Exception

        let saveDataLogged log cfg data =
            data |> log
            saveData cfg data

        let collectResults r =
            match Seq.tryFind Result.isError r with
            | Some _ ->
                ExecError.UnknownError "One or more errors occurred."
                |> Error
            | None -> Ok()

        let errStr =
            function
            | ExecutionError.Exception e -> e.Message
            | ExecutionError.WhoopApiError e ->
                match e with
                | WhoopClient.WhoopApiError.Error e -> e
                | WhoopClient.WhoopApiError.Exception e -> e.Message

        let toExecErr =
            function
                | ExecutionError.Exception e -> ExecError.Exception e
                | ExecutionError.WhoopApiError e ->
                    match e with
                    | WhoopClient.WhoopApiError.Error e -> ExecError.UnknownError e
                    | WhoopClient.WhoopApiError.Exception e -> ExecError.Exception e

        let export (console: #IConsole) cfg =
            let client =
                NonEmptyString.value cfg.AccessToken
                |> (WhoopClient.getClient WhoopClient.defaultConfig)
                    .Authenticated

            let dlog =
                if cfg.Debug then
                    console.Error.WriteLine
                else
                    fun _ -> ()

            getCyclesLogged dlog client cfg.UserId cfg.StartDate cfg.EndDate
            |> AsyncResult.bind (saveDataLogged dlog cfg)
            |> (AsyncResult.teeError (fun e -> errStr e |> dlog))
//            |> fun o -> Async.Parallel(o, 2)
//            |> Async.map collectResults
            |> AsyncResult.mapError toExecErr
            |> AsyncResult.ignore
            |> Async.RunSynchronously

    let Exec console args =
        Parsing.parse args >>= Execution.export console
