namespace Trenton.Cli

open FsConfig

type ExecError =
    | ConfigFileNotFound
    | ConfigParseError of ConfigParseError
    | UnknownVerb of string
    | ArgParseError of string
    | Exception of exn
