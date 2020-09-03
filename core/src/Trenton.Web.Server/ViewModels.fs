namespace Trenton.Web.Server

open System

module ViewModels =
    type Error = { Message: string }

    type MenuItem =
        { Title: string
          Href: string
          Icon: Icon }

    type FitbitDisconnectedSettings = { AuthUri: string }
    type FitbitConnectedSettings = { LastUpdated: DateTime }

    type FitbitSettings =
        | Disconnected of FitbitDisconnectedSettings
        | Connected of FitbitConnectedSettings

    type Settings = { Fitbit: FitbitSettings }
