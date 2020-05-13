namespace Trenton.Health

open System
open Trenton.Common

module BodyFat =
    type BodyFatPercentage = PositivePercent.T

    type FitbitDataSource =
        { LogId: int64
          Source: string }

    type BodyFatSource =
        | Fitbit of FitbitDataSource

    type T =
        { Timestamp: DateTime
          Fat: BodyFatPercentage
          Source: BodyFatSource }
