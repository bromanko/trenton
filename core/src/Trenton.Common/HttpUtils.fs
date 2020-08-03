namespace Trenton.Common

open System

module HttpUtils =
    // EscapeUriString doesn't encode the & and # characters which cause issues, but EscapeDataString encodes too much making the url hard to read
    // So we use EscapeUriString and manually replace the two problematic characters
    let encodeUrlParam param =
        (Uri.EscapeUriString param).Replace("&", "%26").Replace("#", "%23")

    /// Appends the query parameters to the url, taking care of proper escaping
    let appendQueryToUrl query (url: string) =
        match query with
        | [] -> url
        | query ->
            url + if url.Contains "?" then "&" else "?"
            + String.concat "&"
                  [ for k, v in query ->
                      encodeUrlParam k + "=" + encodeUrlParam v ]

