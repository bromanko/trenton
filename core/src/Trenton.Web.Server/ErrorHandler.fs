namespace Trenton.Web.Server

open Giraffe
open Giraffe.Razor
open Microsoft.Extensions.Logging
open System
open Trenton.Common
open Trenton.Web.Server.Config
open Trenton.Web.Server.ViewModels

[<AutoOpen>]
module ErrorHandler =
    let badRequestErr msg =
        RequestErrors.badRequest
        <| razorHtmlView "Error" (Some { Message = msg }) None None

    let internalError msg =
        ServerErrors.internalError
        <| razorHtmlView "Error" (Some { Message = msg }) None None

    let giraffeErrHandler config (ex: Exception) (logger: ILogger) =
        logErrL
            logger
            ex
            "An unhandled exception occurred while processing request."

        let msg =
            if config.IsDevelopment
            then ex.ToString()
            else "An unhandled exception occurred while processing request."

        clearResponse >=> internalError msg
