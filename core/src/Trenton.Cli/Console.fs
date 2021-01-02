namespace Trenton.Cli

open System
open System.IO

type IStandardStreamWriter =
    abstract Write: string -> unit

type IStandardError =
    abstract Error: IStandardStreamWriter

type IStandardOut =
    abstract Out: IStandardStreamWriter

type IConsole =
    inherit IStandardError
    inherit IStandardOut

type StandardStreamWriter(writer: TextWriter) =
    interface IStandardStreamWriter with
        member x.Write str = writer.Write str

type SystemConsole() =
    let error = StandardStreamWriter(Console.Error)
    let out = StandardStreamWriter(Console.Out)

    interface IConsole with
        member x.Error = error :> IStandardStreamWriter
        member x.Out = out :> IStandardStreamWriter
