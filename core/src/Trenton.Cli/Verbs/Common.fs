namespace Trenton.Cli.Verbs

type ExecError =
    | UnknownVerb of string
    | Exception of exn
