namespace Trenton.Webhooks.Routes

open Giraffe

module Fitbit =
    let private action = json {||}

    let handler<'a> = POST >=> route "/fitbit" >=> action
