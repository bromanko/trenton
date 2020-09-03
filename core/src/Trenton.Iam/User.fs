namespace Trenton.Iam

open System
open Trenton.Common

[<RequireQualifiedAccess>]
module UserId =
    type T =
        private
        | UserId of NonEmptyString.T

        override this.ToString() =
            match this with
            | UserId i -> i.ToString()

    let create id =
        match NonEmptyString.create id with
        | None -> None
        | Some id -> Some <| UserId id

    let value (UserId t) = NonEmptyString.value t

type User =
    { CreatedAt: DateTime
      UserId: UserId.T
      Email: Email.T }
