namespace Trenton.Web.Server.Routes

open Giraffe
open Giraffe.Razor

module Health =
    let path = "/health"

    let handler<'a> =
        GET
        >=> route path
        >=> razorHtmlView "Health" None None None
