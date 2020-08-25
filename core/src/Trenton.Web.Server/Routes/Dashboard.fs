namespace Trenton.Web.Server.Routes

open Giraffe
open Giraffe.Razor

module Dashboard =
    let path = "/"

    let handler<'a> =
        GET
        >=> route path
        >=> razorHtmlView "Dashboard" None None None
