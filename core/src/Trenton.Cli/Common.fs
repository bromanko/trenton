namespace Trenton.Cli

type ExecError =
    | ConfigFileError of ConfigFileError
    | UnknownVerb of string
    | ArgParseError of string
    | Exception of exn

