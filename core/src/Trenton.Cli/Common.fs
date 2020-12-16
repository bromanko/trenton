namespace Trenton.Cli

type ExecError =
    | ConfigLoadError of ConfigLoadError
    | UnknownVerb of string
    | ArgParseError of string
    | Exception of exn
