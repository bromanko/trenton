namespace Trenton.Cli

module LogFormatters =
    let logDto (console: IConsole) dto =
        dto
        |> System.Text.Json.JsonSerializer.Serialize
        |> console.Out.WriteLine

    let logError (console: IConsole) e =
        console.Error.WriteLine "An unexpected error occurred."
        console.Error.WriteLine ""
        $"{e}" |> console.Error.WriteLine
