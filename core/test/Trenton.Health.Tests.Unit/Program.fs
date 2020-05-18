namespace Trenton.Health.Tests.Unit

open Expecto
open Hopac
open Logary.Configuration
open Logary.Targets
open Logary.Adapters.Facade
open Trenton.Health.Tests.Unit

module Program =
    let private configLogging config =
        if config.LoggingEnabled then
            let logary =
                Config.create "Trenton.Health.Tests.Unit" "localhost"
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
