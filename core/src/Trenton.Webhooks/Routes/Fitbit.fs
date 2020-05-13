namespace Trenton.Webhooks.Routes

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Trenton.Webhooks.Logging

module Fitbit =
    let private getAccessToken =
        fun (next: HttpFunc) (ctx: HttpContext) -> task { return! next ctx }


    //    https://www.fitbit.com/oauth2/authorize?response_type=token&client_id=22CNRY&redirect_uri=http%3A%2F%2Flocalhost%3A5000%2Ffitbit%2Fcallback&scope=activity%20heartrate%20location%20nutrition%20profile%20settings%20sleep%20social%20weight&expires_in=2592000000
    let authCallbackHandler<'a> =
        GET >=> route "/fitbit/callback" >=> getAccessToken >=> Successful.NO_CONTENT
