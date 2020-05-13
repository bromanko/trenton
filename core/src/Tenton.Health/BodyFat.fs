namespace Trenton.Health

open System
open Trenton.Common

module BodyFat =
    type BodyFatPercentage = PositivePercent.T

    type BodyFatSource =
        | Fitbit of DataSource.FitbitDataSource

    type T =
        { Timestamp: DateTime
          Fat: BodyFatPercentage
          Source: BodyFatSource }


module Weight =
    type Bmi = PositiveDecimal.T

    type Weight = PositiveDecimal.T

    type WeightSource =
        | Fitbit of DataSource.FitbitDataSource

    type T =
        { Timestamp: DateTime
          Bmi: Bmi
          Weight: Weight
          Source: WeightSource }
