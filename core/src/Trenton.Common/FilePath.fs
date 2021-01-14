namespace Trenton.Common

open System.IO

[<RequireQualifiedAccess>]
module FilePath =
    type T =
        private
        | FilePath of string

        override this.ToString() =
            match this with
            | FilePath p -> p

        member this.FullPath =
            match this with
            | FilePath p -> Path.GetFullPath p

    let create p =
        try
            Path.GetFullPath p |> ignore
            FilePath p |> Some
        with _ -> None
