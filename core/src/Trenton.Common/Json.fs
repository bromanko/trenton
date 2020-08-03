namespace Trenton.Common

open System
open System.Globalization
open System.Text
open System.Text.Json

module Json =
    type private SnakeCaseState =
        | Start
        | Lower
        | Upper
        | NewWord

    type SnakeCaseNamingPolicy() =
        inherit JsonNamingPolicy()

        override this.ConvertName s =
            if String.IsNullOrEmpty s then
                s
            else
                let sb = StringBuilder()
                let mutable state = SnakeCaseState.Start

                let nameSpan = s.AsSpan()

                for i = 0 to nameSpan.Length - 1 do
                    if nameSpan.[i] = ' ' then
                        if state <> SnakeCaseState.Start then
                            state <- SnakeCaseState.NewWord
                        else
                            ()
                    else if Char.IsUpper nameSpan.[i] then
                        match state with
                        | SnakeCaseState.Upper ->
                            let hasNext = i + 1 < nameSpan.Length
                            if i > 0 && hasNext then
                                let nextChar = nameSpan.[i + 1]
                                if not (Char.IsUpper nextChar)
                                   && nextChar <> '_' then
                                    sb.Append '_' |> ignore
                        | SnakeCaseState.Lower
                        | SnakeCaseState.NewWord ->
                            sb.Append '_' |> ignore
                        | SnakeCaseState.Start -> ignore ()

                        Char.ToLower(nameSpan.[i], CultureInfo.InvariantCulture)
                        |> sb.Append
                        |> ignore

                        state <- SnakeCaseState.Upper
                    else if nameSpan.[i] = '_' then
                        sb.Append '_' |> ignore
                        state <- SnakeCaseState.Start
                    else
                        if state = SnakeCaseState.NewWord then
                            sb.Append '_' |> ignore

                        sb.Append nameSpan.[i] |> ignore
                        state <- SnakeCaseState.Lower

                sb.ToString()
