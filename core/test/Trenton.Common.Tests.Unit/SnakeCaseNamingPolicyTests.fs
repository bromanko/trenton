namespace Trenton.Common.Tests.Unit

open Bogus
open Expecto
open FsCheck
open System
open Trenton.Common.Json

module SnakeCaseNamingPolicyTests =
    type PropertyNameGen() =
        static member Name(): Arbitrary<string> =
            let f = Faker()
            gen { return f.Lorem.Lines(Nullable 1) } |> Arb.fromGen

    let config =
        { FsCheckConfig.defaultConfig with arbitrary = [ typeof<PropertyNameGen> ] }

    [<Tests>]
    let tests =
        testList "SnakeCaseNamingPolicy"
            [ let policy = SnakeCaseNamingPolicy()

              testPropertyWithConfig config "Creates snake case strings" <| fun s ->
                  let snakeS = policy.ConvertName <| s
                  let hasNoSpaces =
                      not <| String.exists (fun c -> c = ' ') snakeS
                  let hasNoUppercase =
                      not
                      <| String.exists (fun c -> c >= 'A' && c <= 'Z') snakeS
                  hasNoSpaces && hasNoUppercase ]
