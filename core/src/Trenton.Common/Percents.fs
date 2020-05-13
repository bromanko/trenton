namespace Trenton.Common

[<Measure>]
type percent

module PositivePercent =
    type T = private PositiveCent of int64<percent>

    let create =
        function
        | x when x < 0L -> None
        | x -> Some <| PositiveCent(x * 1L<percent>)

    let value x =
        match x with
        | PositiveCent x -> x
