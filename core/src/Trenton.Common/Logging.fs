namespace Trenton.Common

open Microsoft.Extensions.Logging

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
        logger.LogInformation(tmpl)

    let inline logInfoL1 (logger: ILogger) (tmpl: string) (arg: 'a) =
        logger.LogInformation(tmpl, arg)

    let inline logInfoL2 (logger: ILogger) (tmpl: string) a1 a2 =
        logger.LogInformation(tmpl, a1, a2)

    let inline logInfoL3 (logger: ILogger) (tmpl: string) a1 a2 a3 =
        logger.LogInformation(tmpl, a1, a2, a3)

    let inline logInfoL4 (logger: ILogger) (tmpl: string) a1 a2 a3 a4 =
        logger.LogInformation(tmpl, a1, a2, a3, a4)

    let inline logInfoL5 (logger: ILogger) (tmpl: string) a1 a2 a3 a4 a5 =
        logger.LogInformation(tmpl, a1, a2, a3, a4, a5)


    let inline logErrL (logger: ILogger) (ex: exn) (tmpl: string) =
        logger.LogError(ex, tmpl)

    let inline logErrL1 (logger: ILogger) (ex: exn) (tmpl: string) (a1: 'a) =
        logger.LogError(ex, tmpl, a1)

    let inline logErrL2 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 =
        logger.LogError(ex, tmpl, a1, a2)

    let inline logErrL3 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 a3 =
        logger.LogError(ex, tmpl, a1, a2, a3)

    let inline logErrL4 (logger: ILogger) (ex: exn) (tmpl: string) a1 a2 a3 a4 =
        logger.LogError(ex, tmpl, a1, a2, a3, a4)

    let inline logErrL5 (logger: ILogger)
                        (ex: exn)
                        (tmpl: string)
                        a1
                        a2
                        a3
                        a4
                        a5
                        =
        logger.LogError(ex, tmpl, a1, a2, a3, a4, a5)


    let inline logFatalL (logger: ILogger) (ex: exn) (tmpl: string) =
        logger.LogCritical(ex, tmpl)


    let inline logVerboseL (logger: ILogger) (tmpl: string) =
        logger.LogTrace(tmpl)
