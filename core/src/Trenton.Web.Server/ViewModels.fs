namespace Trenton.Web.Server

module ViewModels =
    type Error =
        { Message: string }

    type MenuItem =
        { Title: string
          Href: string
          Icon: Icon }

    type Settings =
        { FitbitAuthUri: string }
