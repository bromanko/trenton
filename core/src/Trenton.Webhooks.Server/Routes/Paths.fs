namespace Trenton.Webhooks.Server.Routes

module Paths =
    let Root = "/"

    let Healthz = "/healthz"

    let Fitbit =
        {| VerifySubscriber = "/fitbit"
           Webhook = "/fitbit" |}

    let Location = {| Webhook = "/locations" |}
