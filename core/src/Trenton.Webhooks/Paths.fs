namespace Trenton.Webhooks

open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module Paths =
    let Index () = PathString "/"

    let Healthz () = PathString "/healthz"
