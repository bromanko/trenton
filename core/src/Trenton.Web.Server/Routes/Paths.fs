namespace Trenton.Web.Server.Routes

module Paths =
    let Dashboard = {| View = "/" |}

    let Healthz =
        {| Liveness = "/healthz/liveness"
           Readiness = "/healthz/readiness" |}

    let Health = {| View = "/health" |}

    let Settings = {| View = "/settings" |}

    let Fitbit = {| AuthCallback = "/fitbit/callback" |}
