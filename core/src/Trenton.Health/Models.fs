namespace Trenton.Common

open System

[<RequireQualifiedAccess>]
module Email =
    type T =
        private
        | Email of string
        override this.ToString() =
            match this with
            | Email s -> s.ToString()

    let create str =
        if String.IsNullOrEmpty(str) then None else Some(Email str)

    let value (Email str) = str


[<RequireQualifiedAccess>]
module NonEmptyString =
    type T =
        private
        | NonEmptyString of string
        override this.ToString() =
            match this with
            | NonEmptyString s -> s.ToString()

    let create str =
        if String.IsNullOrEmpty(str) then None else Some(NonEmptyString str)

    let value (NonEmptyString str) = str
