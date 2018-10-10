module PouchDB.Helpers

open PouchDB
open Fable.PowerPack
open Thoth.Json
open Fable.Core
open Fable.Core.JsInterop
open Types
open Fable

type RevNumber = string

type Msg =
  | Load of Core.Response
  | IndexCreated of Find.CreateIndexResponse
  | GetDocument of Core.Document
  | PouchError of Http.HttpCode
  | Unauthorized
  | ConnectionFailed of string
  | MissingPassword
  | Conflict //of RevNumber
  | Basic of Core.BasicResponse
  | Sync of Core.Document
  | SessionInformation of SessionDocument
  | CouchUser of CouchUserDocument
//  | Sync of SyncChanges
  | ServerError of string

module Selectors =
    let byDefault : PouchDB.Find.FindRequest = jsOptions( fun r ->
      (*
      r.selector <- jsOptions( fun s ->
        s.Item("name") <- !^jsOptions<PouchDB.Find.IConditionOperators>( fun c ->
          c.gt <- null
        )
      )*)
      r.sort <- Some !![|"_id"|]
//      r.sort <- Some !![|"name"|]
      r.limit <- Some 100.
    )

    let retrieveDocs : PouchDB.Core.AllDocsOptions = jsOptions( fun opt ->
      opt.include_docs <- Some true
    )

/// Helpers to abstract the whole decoding of the docs
let decodeDocs<'T> wanted ignored stringToDecode : Result<'T list, string> =
    ((Decode.field "docs" (Decode.list (InterType<'T>.Decoder wanted ignored)), stringToDecode))
    ||> Decode.decodeString
    |> function
        | Ok result ->
            result
            |> List.filter (function | ToRemove -> false | ToKeep _ -> true)
            |> List.map (function | ToRemove -> failwith "Should be removed by filter before" | ToKeep v -> v)
            |> Ok
        | Error error -> Error error

let decodeAllDocs<'T> (customDecoder:Decode.Decoder<'T>) stringToDecode : Result<InnerDocument<'T> list, string> =
    //failwith "decoding process"
    Decode.decodeString (AllDocsDocument.Decoder customDecoder) stringToDecode
    |> function
      | Ok result -> Ok result.Rows
      | Error error ->
        Error error


let decodeDocument (originRequest:OriginRequest) (customDecoder,fieldToCheck) document : 'T list=
  let strDoc = document.ToString()

  match originRequest with
  | SyncDoc ->
    let decoded = Decode.decodeString customDecoder strDoc
    match decoded with
    | Ok o ->
      [o]
    | Error s ->
      printfn "Sync message decoding error %s: %s" s strDoc
      [] //

  | AllDocs ->
     decodeAllDocs customDecoder strDoc
     |> function
        | Ok innerDocuments ->
          innerDocuments |> List.map( fun d -> d.Doc)

        | Error s ->
          printfn "WARNING: this database seems to have no documents."
          printfn "Reported Error was: %s" s
          []

  (*
  | Find ->
    let wantedObject =
        Decode.field fieldToCheck Decode.string
        |> Decode.andThen (fun _ ->
            customDecoder
            |> Decode.andThen (ToKeep >> Decode.succeed)
        )

    // PouchDB mixes a mango query object with other objects
    // so we ignore this object
    let ignoredMangoObject =
      Decode.field "language" Decode.string
      |> Decode.andThen (fun _ -> Decode.succeed ToRemove )

    // let's try to parse it assuming it's a list of documents {docs:[{"fieldName":value}]}
    let decoded =decodeDocs wantedObject ignoredMangoObject strDoc

    match decoded with
    | Ok o -> o
    | Error s ->
      // let's try to parse it assuming it's a single document {"fieldName":value}
      let decoded = Decode.decodeString customDecoder strDoc
      match decoded with
      | Ok o ->
        [o]
      | Error s ->
        printfn "WARNING: this database seems to have no documents."
        printfn "%s" s
        [] // no documents found we get a special object
        *)

let getResponse handler response =
  handler (Load response)

let getBasicResponse handler response =
  handler (Basic response)

let getSyncResponse handler response =
  handler (Sync response)

let createIndexResponse handler response =
  handler (IndexCreated response)

let getDocuments handler response =
  handler (GetDocument response)

let getSession handler response =
  let decoded = Decode.decodeString SessionDocument.Decoder (response.ToString())
  match decoded with
  | Ok data ->
      handler (SessionInformation data)
  | Error msg ->
      handler Unauthorized

let getUser handler response =
  response
  |> string
  |> Decode.decodeString CouchUserDocument.Decoder
  |> function
      | Ok data ->
          handler <| CouchUser data
      | Error msg ->
          handler <| ServerError msg

let getError handler response  =
  response
  |> string
  |> Decode.decodeString ErrorDocument.Decoder
  |> function
      | Ok data ->
          let rawError = sprintf "%A" data
          match data.Status with
          | 401 ->
            handler <| Unauthorized

          | 409 ->
            handler <| Conflict

          | 0 ->
            handler <| ConnectionFailed rawError

          | 400 ->
            handler <| MissingPassword

          | _ ->
            let status = Http.HttpCode.tryParse data.Status
            handler <| PouchError status
      | Error msg ->
          handler <| ServerError msg

let insert (db:Database) (o:Core.Document) handler=
    promise {
      let! r= db.put(o)
      return r
    }
    |> Promise.map(fun response -> handler (Load response))
    |> Promise.catch (getError handler )
    |> ignore

let get (db:Database) (id:Core.DocumentId) handler=
    promise {
      let! r= db.get(id)
      return r
    }
    |> Promise.map(fun doc -> handler (GetDocument doc))
    |> Promise.catch (getError handler)
    |> ignore

let createIndex (db:Database) (index:Find.CreateIndexOptions) handler=
    promise {
      let! r= db.createIndex(index)
      return r
    }
    |> Promise.map(fun response -> handler (IndexCreated response))
    |> Promise.catch (getError handler)
    |> ignore


let find (db:Database) (selector:Find.FindRequest) handler=
    promise {
      let! r= db.find(selector)
      return r
    }
    |> Promise.map(fun doc -> handler (GetDocument doc))
    |> Promise.catch (getError handler)
    |> ignore

let isIndexReady (createIndexMessage:Msg) =
    match createIndexMessage with
    | IndexCreated _ -> true
    | _ -> false

let prepareIndex fields =
  jsOptions<PouchDB.Find.CreateIndexOptions>( fun i ->
    let r = jsOptions<PouchDB.Find.CreateIndexRequest>( fun r ->
      r.fields <- Some !!fields
    )
    i.index <- r
  )



module Elmish =

(*
  let createIndex index (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.createIndex
      index
      (createIndexResponse msg)
      (getError msg)

  let find selector (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.find
      selector
      (getDocuments msg)
      (getError msg)
*)
  let allDocs options (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.allDocs
      options
      (getDocuments msg)
      (getError msg)

  let query (view:string,options:Query.Options option) (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.query
      (view,options)
      (getDocuments msg)
      (getError msg)

  let put data (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.put
      data
      (getResponse msg)
      (getError msg)

  let replicateTo remote (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.ReplicateTo
      remote
      (getSyncResponse msg)
      (getError msg)

  let replicateFrom remote (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.ReplicateFrom
      remote
      (getSyncResponse msg)
      (getError msg)

  let replicate remote options (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.Replicate
      (remote,options)
      (getSyncResponse msg)
      (getError msg)

  let destroy (msg:'Msg) (store:PouchDB.Database) =
    Elmish.Cmd.ofPromise
      store.destroy
      ()
      (getBasicResponse msg)
      (getError msg)

  module Authentication =

    let login (user:string,password:string) (msg:'Msg) (store:PouchDB.Database) =
      Elmish.Cmd.ofPromise
        store.login
        (user,password)
        (getBasicResponse msg)
        (getError msg)

    let logout (msg:'Msg) (store:PouchDB.Database) =
      Elmish.Cmd.ofPromise
        store.logout
        ()
        (getBasicResponse msg)
        (getError msg)

    let signUp (user:string,password:string) (msg:'Msg) (store:PouchDB.Database) =
      Elmish.Cmd.ofPromise
        store.signup
        (user,password)
        (getBasicResponse msg)
        (getError msg)

    let getSession (msg:'Msg) (store:PouchDB.Database) =
      Elmish.Cmd.ofPromise
        store.getSession
        ()
        (getSession msg)
        (getError msg)

    let getUser (username:string) (msg:'Msg) (store:PouchDB.Database) =
      Elmish.Cmd.ofPromise
        store.getUser
        username
        (getUser msg)
        (getError msg)

    let putUser name data (msg:'Msg) (store:PouchDB.Database) =
      Elmish.Cmd.ofPromise
        store.putUser
        (name,data)
        (getBasicResponse msg)
        (getError msg)
