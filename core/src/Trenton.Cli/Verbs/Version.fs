namespace Trenton.Cli.Verbs

open System.Reflection

module Version =
    let exec _ =
        Assembly.GetExecutingAssembly().GetName().Version.ToString()
        |> printf "%s"
        Ok()
