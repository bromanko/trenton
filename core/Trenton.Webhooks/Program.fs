open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting
open Giraffe


let configureApp webApp (app: IApplicationBuilder) =
    app.UseGiraffe webApp

let private configureApp = ()

[<EntryPoint>]
let main argv =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
        webHostBuilder.Configure(configureApp webApp) |> ignore).Build().Run()
    0
