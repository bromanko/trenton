namespace Trenton.Cli

type ExecError =
    | UnknownVerb of string
    | ArgParseError of string
    | Exception of exn
    | UnknownError of string
