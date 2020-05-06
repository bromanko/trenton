namespace Trenton.Webhooks

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

module Host =
    let private webApp =
        choose [ GET >=> choose [ route "/" >=> text "index" ] ]

    let private configureApp =
        fun (app: IApplicationBuilder) -> app.UseGiraffe webApp

    let createHostBuilder argv =
        Host.CreateDefaultBuilder(argv)
            .ConfigureWebHostDefaults(fun wb ->
            wb.Configure(configureApp) |> ignore)
