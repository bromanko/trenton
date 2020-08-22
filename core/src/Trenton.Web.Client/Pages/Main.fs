namespace Trenton.Web.Client.Pages

open System
open Bolero
open Elmish
open Bolero.Remoting.Client
open Bolero.Templating.Client

module Main =
    type Page = | [<EndPoint "/">] Home

    type Model = { page: Page }

    let initModel = { page = Home }

    type Message = SetPage of Page

    let update message model: Model * Cmd<Message> =
        match message with
        | SetPage page -> { model with page = page }, Cmd.none


    let router =
        Router.infer SetPage (fun model -> model.page)

    type Main = Template<"wwwroot/main.html">

    type DesktopSidebar = Template<"wwwroot/components/desktop-sidebar.html">
    type MobileSidebar = Template<"wwwroot/components/mobile-sidebar.html">

    type Header = Template<"wwwroot/components/header.html">

    let view _ _ =
        let sidebar =
            Concat [ DesktopSidebar().Elt()
                     MobileSidebar().Elt() ]

        Main().Sidebar(sidebar).Header(Header().Elt()).Elt()

    type Config = { IsDevelopment: bool }

    type App() =
        inherit ProgramComponent<Model, Message>()

        let getCfg (services: IServiceProvider) =
            match services.GetService(typeof<Config>) with
            | :? Config as cfg -> cfg
            | _ -> { IsDevelopment = false }

        override this.Program =
            let cfg = getCfg this.Services

            let program =
                Program.mkProgram (fun _ -> initModel, Cmd.none) update view
                |> Program.withRouter router

            if cfg.IsDevelopment then
                Program.withHotReload program
            else
                program
