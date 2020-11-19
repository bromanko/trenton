namespace Trenton.Cli.Verbs

type ExecError =
    | UnknownVerb of string
    | ArgParseError of string
    | Exception of exn
