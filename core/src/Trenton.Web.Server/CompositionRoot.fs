namespace Trenton.Server

open Trenton.Server.Config

type CompositionRoot = unit

[<AutoOpen>]
module ComponentRoot =
    let getCompRoot (_: AppConfig) = ()
