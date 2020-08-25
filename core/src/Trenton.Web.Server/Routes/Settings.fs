namespace Trenton.Web.Server.Routes

open Giraffe
open Giraffe.Razor

module Settings =
    let path = "/settings"

    let handler<'a> =
        GET
        >=> route path
        >=> razorHtmlView "Settings" None None None
