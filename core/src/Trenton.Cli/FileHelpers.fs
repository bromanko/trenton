namespace Trenton.Cli

open Trenton.Common

module FileHelpers =
    let getFilenameForDate prefix (date: Date.T) ext =
        sprintf "%s-%s.%s" prefix (date.ToString "yyyyMMddHHmmssZ") ext
