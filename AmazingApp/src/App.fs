module AmazingApp

open Fable.Core.JsInterop
open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Elmish.React
open Elmish.Debug
open Elmish.HMR
open Fable.PowerPack

// register CSS
importAll "../sass/main.scss"

// start our app only if our service worker registered well
promise {

    try
      // register service worker
      //#if DEBUG
      //printfn "DEBUG MODE - NO Service Worker"
      //#else
      let! _ = "./sw.js" |> Fable.Import.Browser.navigator.serviceWorker.register
      //#endif

      Program.mkProgram Main.State.init Main.State.update Main.View.root
      |> Program.toNavigable (parseHash Router.pageParser) Main.State.setRoute
      |> Program.withReact "elmish-app"
      //|> Program.withHMR
      |> Program.withConsoleTrace
      |> Program.run

    with exn -> printfn "%s" exn.Message
}
|> Promise.start
