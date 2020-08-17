namespace Trenton.Web.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Bolero.Remoting.Client
open Trenton.Web.Client.Pages

module Program =
    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.App>("#main")
        builder.Services.AddRemoting(builder.HostEnvironment) |> ignore
        builder.Build().RunAsync() |> ignore
        0
