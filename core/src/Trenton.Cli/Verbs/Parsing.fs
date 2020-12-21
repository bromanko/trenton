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

    let parseOptionalNes msg = Option.traverseResult (parseNes msg)

    let parseOptionalDate msg =
        function
        | Some d ->
            match parseDate msg d with
            | Result.Error e -> Result.Error e
            | Ok d -> Ok <| Some d
        | None -> Ok None

    let parseNesWithFallback getFallback msg =
        function
        | Some s -> parseNes msg s
        | None ->
            match getFallback () with
            | None -> ArgParseError msg |> Result.Error
            | Some a -> parseNes msg a

    let parseOptionalNesWithFallback getFallback msg a =
        match a with
        | Some s -> Some s
        | None -> getFallback ()
        |> parseOptionalNes msg
