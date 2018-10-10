module Router

open Types
open Fable.Import
open Fable.Helpers.React.Props
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
let private toHash page =

  match page with
  | Welcome -> "#welcome"
//  | ViewArticle id-> sprintf "#view/%s" id

let pageParser: Parser<Route->Route,_> =
  oneOf [
    map Welcome (s "welcome")
//    map ViewArticle (s "view" </> str)
  ]

let href route =
  Href (toHash route)

let modifyUrl route =
  route |> toHash |> Navigation.modifyUrl

let newUrl route =
//  route |> toHash |> Navigation.modifyUrl
  route |> toHash |> Navigation.newUrl

let modifyLocation route =
  Browser.window.location.href <- toHash route
