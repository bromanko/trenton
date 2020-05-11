namespace Trenton.Webhooks

open Serilog

/// Formatting target
type LogTarget =
    | LiterateConsole
    | Json

/// Logging level
type LogLevel =
    | Verbose
    | Debug
    | Information
    | Warning
    | Error
    | Fatal

[<AutoOpen>]
module Logging =
    let inline logInfoL (logger: ILogger) (tmpl: string) =
        logger.Information(tmpl)

    let inline logInfoL1 (logger: ILogger) (tmpl: string) (arg:'a) =
        logger.Information(tmpl, arg)

    let inline logInfoL2 (logger: ILogger) (tmpl:string) a1 a2 =
        logger.Information(tmpl, a1, a2 )

    let inline logInfoL3 (logger: ILogger) (tmpl: string) a1 a2 a3 =
        logger.Information(tmpl,  a1, a2, a3 )

    let inline logInfoL4 (logger: ILogger) (tmpl: string) a1 a2 a3 a4 =
        logger.Information(tmpl,  a1, a2, a3, a4 )

    let inline logInfoL5 (logger: ILogger) (tmpl: string) a1 a2 a3 a4 a5 =
        logger.Information(tmpl,  a1, a2, a3, a4, a5 )

    let inline logInfo (tmpl: string) = logInfoL Log.Logger tmpl

    let inline logInfo1 (tmpl: string) (a1: 'a) = logInfoL1 Log.Logger (tmpl: string) a1

    let inline logInfo2 (tmpl: string) a1 a2 = logInfoL2 Log.Logger (tmpl: string) a1 a2

    let inline logInfo3 (tmpl: string) a1 a2 a3 = logInfoL3 Log.Logger (tmpl: string) a1 a2 a3

    let inline logInfo4 (tmpl: string) a1 a2 a3 a4 =
        logInfoL4 Log.Logger (tmpl: string) a1 a2 a3 a4

    let inline logInfo5 (tmpl: string) a1 a2 a3 a4 a5 =
        logInfoL5 Log.Logger (tmpl: string) a1 a2 a3 a4 a5


    let inline logErrL (logger: ILogger) (ex: exn) (tmpl: string) =
        logger.Error(ex, tmpl)

    let inline logErrL1 (logger: ILogger) (ex: exn) (tmpl: string) (a1:'a) =
        logger.Error(ex, tmpl, a1)

    let inline logErrL2 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 =
        logger.Error(ex, tmpl, a1, a2)

    let inline logErrL3 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 a3 =
        logger.Error(ex, tmpl, a1, a2, a3)

    let inline logErrL4 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 a3 a4 =
        logger.Error(ex, tmpl, a1, a2, a3, a4)

    let inline logErrL5 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 a3 a4 a5 =
        logger.Error(ex, tmpl, a1, a2, a3, a4, a5)


    let inline logFatalL (logger: ILogger) (ex: exn) (tmpl: string) =
        logger.Fatal(ex, tmpl)

    let inline logFatal ex (tmpl: string) = logFatalL Log.Logger ex tmpl

    let inline logVerboseL (logger: ILogger) (tmpl: string) =
        logger.Verbose(tmpl)

