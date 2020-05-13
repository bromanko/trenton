namespace Trenton.Common

module PositiveDecimal =
    type T = private PositiveDecimal of decimal

    let create =
        function
        | x when x < 0m -> None
        | x -> Some <| PositiveDecimal(x * 1m)

    let value x =
        match x with
        | PositiveDecimal x -> x
