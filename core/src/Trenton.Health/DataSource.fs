namespace Trenton.Health

module DataSource =
    type FitbitDataSource =
        { LogId: int64
          Source: string option }
