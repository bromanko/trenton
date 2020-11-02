namespace Trenton.Webhooks.Server

open Giraffe
open Microsoft.Extensions.Logging
open System
open Trenton.Common
open Trenton.Webhooks.Server.Config

[<CLIMutable>]
type ErrorResponse = { Message: string }

[<AutoOpen>]
module ErrorHandler =
    let private config = loadAppConfig ()

    let badRequestErr msg =
        RequestErrors.BAD_REQUEST { Message = msg }

    let internalError msg =
        ServerErrors.INTERNAL_ERROR { Message = msg }

    let internalErrorEx (logger: ILogger) ex =
        logErrL
            logger
            ex
            "An unhandled exception occurred while processing request."

        if config.Server.IsDevelopment
        then ex.ToString()
        else "An unhandled exception occurred while processing request."
        |> internalError

    let unauthorizedErr scheme realm msg =
        RequestErrors.UNAUTHORIZED scheme realm { Message = msg }

    let giraffeErrHandler (ex: Exception) (logger: ILogger) =
        clearResponse >=> internalErrorEx logger ex
