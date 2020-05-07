namespace Trenton.Webhooks

open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module Paths =
    let index () = PathString "/"

    let health () = PathString "/healthz"
