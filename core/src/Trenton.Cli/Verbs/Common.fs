namespace Trenton.Cli.Verbs

open FsConfig

type ExecError =
    | ConfigParseError of ConfigParseError
    | UnknownVerb of string
    | ArgParseError of string
    | Exception of exn
