open Microsoft.Extensions.Hosting
open Trenton.Webhooks.Host
open Trenton.Webhooks.Config

[<EntryPoint>]
let main argv =
    let config = loadAppConfig()
    (createHostBuilder argv config).Build().Run()
    0
