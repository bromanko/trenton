namespace Trenton.Cli.Verbs.Auth.Whoop

open Argu
open Trenton.Cli

module SubCommands =
    let Exec console (args: ParseResults<WhoopAuthArgs>) =
        match args.GetSubCommand() with
        | WhoopAuthArgs.Login f -> Login.Exec console f
        | WhoopAuthArgs.RefreshToken f -> RefreshToken.Exec console f
