namespace Trenton.Cli

open Trenton.Common

module FileHelpers =
    let getFilenameForDate prefix (date: Date.T) ext =
        sprintf "%s-%s.%s" prefix (date.ToString "yyyyMMddHHmmssZ") ext

    let getFilenameForDateRange
        prefix
        (startDate: Date.T)
        (endDate: Date.T)
        ext
        =
        sprintf
            "%s-%s-%s.%s"
            prefix
            (startDate.ToString "yyyyMMddHHmmssZ")
            (endDate.ToString "yyyyMMddHHmmssZ")
            ext
