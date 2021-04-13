namespace Trenton.Cli

module LogFormatters =
    let logDto (console: IConsole) dto =
        dto
        |> System.Text.Json.JsonSerializer.Serialize
        |> console.Out.Write

    let logError (console: IConsole) e =
        console.Error.Write "An unexpected error occurred."
        console.Error.Write ""
        $"{e}" |> console.Error.Write
