module Main

module Types = 

  open Types

  type ActivePage =
      | Welcome of Page.Welcome.Types.Model    

  type Msg = 
    | WelcomeMsg of Page.Welcome.Types.Msg

  type Model = 
    {
      ActivePage:ActivePage option
      CurrentRoute:Route option
      Count:int
      IsLoading:bool
    }

module State = 

  open Types
  open Elmish
  open Elmish.Browser.Navigation
  open Elmish.Browser.UrlParser

  module Update =

    let load model =
      {model with IsLoading=true}

    let activePage activePage route model =
      {
        model with
          IsLoading=false
          ActivePage=Some activePage
      },Router.newUrl route

    let routeMessage activePage mapper command model =
      { model with ActivePage=Some activePage}, Cmd.map mapper command


  let setRoute (optRoute: Option<Route>) model =
      let model = { model with CurrentRoute = optRoute }
      match optRoute with 
        | Some ( Route.Welcome )->
          model
            |> Update.activePage
              (ActivePage.Welcome (Page.Welcome.Types.initialModel))
              Route.Welcome
        | None ->
          model
            |> Update.activePage
              (ActivePage.Welcome (Page.Welcome.Types.initialModel))
              Route.Welcome
        

  let init location : Model * Cmd<Msg> = 
    
    printfn "***Awesome APP %s***" Version.number

    let (model, cmd) =
      setRoute location
        {
          Count = 0
          ActivePage=None
          CurrentRoute=None
          IsLoading=false
        }
    
    model, cmd

  let nextPage route model =
    setRoute (Some route) model

  let update msg model = 
      match model.ActivePage, msg with
      | Some page, msg ->
        match page,msg with      
        | Welcome md, WelcomeMsg msg -> 
          match msg with 
          | _ -> 
              let updated, cmd = Page.Welcome.State.update msg md
              model
                |> Update.routeMessage (ActivePage.Welcome updated) WelcomeMsg cmd        
      | _ -> 
        model, Cmd.none

module View = 

  open Types
  open Fulma
  open Fable.Core.JsInterop
  open Fable.Helpers.React
  open Fable.Helpers.React.Props
  
  let root (model:Model) dispatch = 
    div[] [ str "ok"]
