namespace Trenton.Iam

open System
open Trenton.Common

/// A hard-coded user
[<RequireQualifiedAccess>]
module Bromanko =
    let user =
        { CreatedAt = DateTime(2020, 9, 2, 7, 35, 0)
          UserId = ( UserId.create <| "044FE77D-E824-4E85-97C9-8F6D53A40FA8" ).Value
          Email = ( Email.create "hello@bromanko.com" ).Value }

