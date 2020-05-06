open Microsoft.Extensions.Hosting
open Trenton.Webhooks.Host

[<EntryPoint>]
let main argv =
    (createHostBuilder argv).Build().Run()
    0
