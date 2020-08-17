namespace Trenton.Web.Client.Pages

open System
open Bolero
open Elmish
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client

module Main =
    type Page = | [<EndPoint "/">] Home

    type Model = { page: Page }

    let initModel = { page = Home }

    type Message =
        | SetPage of Page

    let update message model: Model * Cmd<Message> =
        match message with
        | SetPage page -> { model with page = page }, Cmd.none


    let router =
        Router.infer SetPage (fun model -> model.page)

    type Main = Template<"wwwroot/main.html">

    let homePage _ _ =
        Main.Home().Elt()

    let menuItem (model: Model) (page: Page) (text: string) =
        Main.MenuItem().Active(if model.page = page then "is-active" else "")
            .Url(router.Link page).Text(text).Elt()

    let view model dispatch =
        Main()
            .Menu(concat [ menuItem model Home "Home" ])
            .Body(cond model.page
                 <| function
                 | Home -> homePage model dispatch)
            .Elt()

    type Config = { IsDevelopment: bool }

    type App() =
        inherit ProgramComponent<Model, Message>()

        let getCfg (services : IServiceProvider) =
            match services.GetService(typeof<Config>) with
            | :? Config as cfg -> cfg
            | _ -> { IsDevelopment = false }

        override this.Program =
            let cfg = getCfg this.Services

            let program =
                Program.mkProgram (fun _ -> initModel, Cmd.none)
                    update view
                |> Program.withRouter router

            if cfg.IsDevelopment then Program.withHotReload program |> ignore

            program
