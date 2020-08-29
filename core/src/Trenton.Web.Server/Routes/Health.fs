namespace Trenton.Web.Server.Routes

open Giraffe
open Giraffe.Razor

module Health =
    module View =
        let handler<'a> =
            GET
            >=> route Paths.Health.View
            >=> razorHtmlView "Health" None None None
