namespace Trenton.Cli.Verbs

open System.Reflection

module Version =
    let Exec () =
        Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString()
        |> printf "%s"

        Ok()
