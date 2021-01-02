namespace Trenton.Cli.Verbs

open Trenton.Cli
open System.Reflection

module Version =
    let Exec (console: #IConsole) =
        Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString()
        |> console.Out.Write

        Ok()
