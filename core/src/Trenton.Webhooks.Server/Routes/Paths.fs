namespace Trenton.Webhooks.Server.Routes

module Paths =
    let Root = "/"

    let Fitbit =
        {| VerifySubscriber = "/fitbit"
           Webhook = "/fitbit" |}
