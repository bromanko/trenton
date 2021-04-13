namespace Trenton.Cli.Verbs.Auth.Fitbit

open Argu
open Trenton.Cli

module SubCommands =
    let Exec console (args: ParseResults<FitbitAuthArgs>) =
        match args.GetSubCommand() with
        | FitbitAuthArgs.Login f -> Login.Exec console f
        | FitbitAuthArgs.RefreshToken f -> RefreshToken.Exec console f
