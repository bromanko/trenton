namespace Trenton.Webhooks.Server.Routes

module Paths =
    let Root = "/"

    let Healthz =
        {| Liveness = "/healthz/liveness"
           Readiness = "/healthz/readiness" |}

    let Fitbit =
        {| VerifySubscriber = "/fitbit"
           Webhook = "/fitbit" |}

    let Location = {| Webhook = "/locations" |}
