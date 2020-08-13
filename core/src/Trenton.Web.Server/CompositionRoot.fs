namespace Trenton.Web.Server

open Trenton.Web.Server.Config

type CompositionRoot = unit

[<AutoOpen>]
module ComponentRoot =
    let getCompRoot (_: AppConfig) = ()
