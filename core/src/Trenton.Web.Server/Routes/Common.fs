namespace Trenton.Web.Server.Routes

open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Trenton.Web.Server

module Common =
    let earlyReturn: HttpFunc = Some >> Task.FromResult

    let bindQueryOrErr<'T> f =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let result = ctx.TryBindQueryString<'T>()
            match result with
            | Ok o -> f o next ctx
            | Result.Error r -> badRequestErr r earlyReturn ctx
