namespace Trenton.Web.Server.Routes

open Giraffe
open Giraffe.Razor

module Dashboard =
    let handler<'a> =
        GET >=> route "/"
        >=> razorHtmlView "Dashboard" None None None


