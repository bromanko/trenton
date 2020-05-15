namespace Trenton.Common.Tests.Unit

open Expecto
open Hopac
open Logary.Configuration
open Logary.Targets
open Logary.Adapters.Facade
open Trenton.Common.Tests.Unit

module Program =
    let private configLogging config =
        if config.LoggingEnabled then
            let logary =
                Config.create "Trenton.Common.Tests.Unit" "localhost"
                |> Config.targets
                    [ LiterateConsole.create LiterateConsole.empty "console" ]
                |> Config.processing
                    (Events.events |> Events.sink [ "console" ])
                |> Config.build
                |> Hopac.run
            LogaryFacadeAdapter.initialise<Expecto.Logging.Logger> logary

    [<EntryPoint>]
    let main argv =
        let testCfg = Config.loadConfig()
        configLogging testCfg

        Tests.runTestsInAssembly defaultConfig argv
