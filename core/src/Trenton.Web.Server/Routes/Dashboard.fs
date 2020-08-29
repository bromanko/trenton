namespace Trenton.Web.Server.Routes

open Giraffe
open Giraffe.Razor

module Dashboard =
    module View =
        let handler<'a> =
            GET
            >=> route Paths.Dashboard.View
            >=> razorHtmlView "Dashboard" None None None
