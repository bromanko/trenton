namespace Trenton.Cli.Verbs

open Trenton.Common
open Trenton.Cli
open FsToolkit.ErrorHandling

[<AutoOpen>]
module Parsing =
    let parse f msg a =
        match f a with
        | None -> Result.Error <| ArgParseError msg
        | Some s -> Ok s

    let parseNes = parse NonEmptyString.create

    let parseDate = parse Date.tryParse

    let parseOptionalNes msg a = Option.traverseResult (parseNes msg) a

    let parseOptionalDate msg a =
        match a with
        | Some d ->
            match parseDate msg d with
            | Result.Error e -> Result.Error e
            | Ok d -> Ok <| Some d
        | None -> Ok None
