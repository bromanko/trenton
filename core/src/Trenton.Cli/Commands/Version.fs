namespace Trenton.Cli.Commands

open System.Reflection

module Version =
    let exec =
        Assembly.GetExecutingAssembly().GetName().Version.ToString()
        |> printf "%s"
