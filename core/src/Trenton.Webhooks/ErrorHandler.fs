namespace Trenton.Webhooks

open Giraffe
open Microsoft.Extensions.Logging
open System
open Trenton.Webhooks.Config

type ErrorResponse =
    { Message: string }

[<AutoOpen>]
module ErrorHandler =
    let badRequestErr msg =
        RequestErrors.BAD_REQUEST { Message = msg }

    let internalError msg =
        ServerErrors.INTERNAL_ERROR { Message = msg }

    let giraffeErrHandler config (ex: Exception) (logger: ILogger) =
        logErrL logger ex
            "An unhandled exception occurred while processing request."

        let msg =
            if config.IsDevelopment
            then ex.ToString()
            else "An unhandled exception occurred while processing request."

        clearResponse >=> internalError msg

