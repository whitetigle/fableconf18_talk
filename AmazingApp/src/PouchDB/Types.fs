module PouchDB.Types

open PouchDB
open Thoth.Json
open PouchDB.PouchDB.Core
open Global
open Fable.Core.JsInterop

type OriginRequest = Find | AllDocs | SyncDoc

type UserData<'T> =
  {
    Metadata: 'T
  }
  static member Decoder customDecoder=
    Decode.decode
      (fun d ->
        {
          Metadata = d
        } : UserData<'T>)
      |> Decode.required "metadata" (customDecoder)

  static member Prepare (o:obj) =
    Encoder.empty
    |> Encoder.object Encoder.Required "metadata" (Some o)
    |> Encoder.toPlainJsObj

type InnerDocument<'T> =
  {
    Id:string
    Key:string option
//    Value:obj // TODO
    Doc:'T
  }
  static member Decoder customDecoder=
    Decode.decode
      (fun i k  d ->
        {
          Id = i
          Key = k
//            Value = obj
          Doc = d
        } : InnerDocument<'T>)
      |> Decode.required "id" Decode.string
      |> Decode.optional "key" (Decode.option Decode.string) None
      |> Decode.required "doc" (customDecoder)

type AllDocsDocument<'T> =
  {
    TotalRows:int
    Offset:int
    Rows:InnerDocument<'T> list
  }
  static member Decoder customDecoder=
    Decode.decode
      (fun tr o r ->
        {
          TotalRows = tr
          Offset = o
          Rows = r
        } : AllDocsDocument<'T>)
      |> Decode.required "total_rows" Decode.int
      |> Decode.required "offset" Decode.int
      |> Decode.required "rows" (Decode.list <| InnerDocument.Decoder customDecoder)

type ErrorDocument =
  {
    Status:int
    Name:string option
    Message:string option
    Error:bool option
    DocId:string option
    Id:string option
  }
  static member Decoder =
    try
      Decode.decode
          (fun status name message error docId id ->
              { Status = status
                Name = name
                Message = message
                Error = error
                DocId = docId
                Id=id
              } : ErrorDocument)
          |> Decode.required "status" Decode.int
          |> Decode.optional "name" (Decode.option Decode.string) None
          |> Decode.optional "message" (Decode.option Decode.string) None
          |> Decode.optional "error" (Decode.option Decode.bool) None
          |> Decode.optional "docId" (Decode.option Decode.string) None
          |> Decode.optional "id" (Decode.option Decode.string) None
     with err ->
      failwith err.Message

// see https://github.com/MangelMaxime/Thot/issues/11 for more information
// Intermediate type to modelized the list
type InterType<'T> =
    /// Case used to removed the non wanted lins
    | ToRemove
    /// Case used to keep the wanted lines
    | ToKeep of 'T

    static member Decoder wanted ignored=
      Decode.oneOf [ wanted; ignored ]

type SessionInformation =
  {
    Authenticated: string option
    AuthenticationDB:string option
    AuthenticationHandlers:string list option
  }
  static member Decoder =
    try
      Decode.decode
          (fun a adb ah ->
              { Authenticated = a
                AuthenticationDB = adb
                AuthenticationHandlers =ah
              } : SessionInformation)
          |> Decode.optional "authenticated" (Decode.option Decode.string) None
          |> Decode.optional "authentication_db" (Decode.option Decode.string) None
          |> Decode.optional "authentication_handlers" (Decode.option (Decode.list Decode.string)) None
     with err ->
      failwith err.Message

type UserContext =
  {
    Name:string option
    Roles:string list option
  }
  static member Decoder =
    try
      Decode.decode
          (fun n r ->
            { Name=n
              Roles=r
            } : UserContext
          )
            |> Decode.optional "name" (Decode.option Decode.string) None
            |> Decode.optional "roles" (Decode.option (Decode.list Decode.string)) None
     with err ->
      failwith err.Message

type SessionDocument =
  {
    Info:SessionInformation
    Ok:bool
    UserCTX:UserContext
  }
  static member Decoder =
    try
      Decode.decode
          (fun info ok user  ->
              { Info = info
                Ok = ok
                UserCTX = user
              } : SessionDocument)
          |> Decode.required "info" (SessionInformation.Decoder)
          |> Decode.required "ok" Decode.bool
          |> Decode.required "userCtx" (UserContext.Decoder)
     with err ->
      failwith err.Message

type CouchUserDocument =
  {
    Id:string
    Rev:string
    DerivedKey:string
    Iterations:int
    Name:string
    PasswordScheme:string
    Roles:string list
    Salt:string
    Type:string
    Counter:int option
    Sync:string list
  }
  static member Decoder =
    try
      Decode.decode
          (fun i r d it n p ro s t c sl->
              { Id=i
                Rev=r
                DerivedKey=d
                Iterations=it
                Name=n
                PasswordScheme=p
                Roles=ro
                Salt=s
                Type=t
                Counter=c
                Sync=sl
              } : CouchUserDocument)
          |> Decode.required "_id" Decode.string
          |> Decode.required "_rev" Decode.string
          |> Decode.required "derived_key" Decode.string
          |> Decode.required "iterations" Decode.int
          |> Decode.required "name" Decode.string
          |> Decode.required "password_scheme" Decode.string
          |> Decode.required "roles" (Decode.list Decode.string)
          |> Decode.required "salt" Decode.string
          |> Decode.required "type" Decode.string
          |> Decode.optional "counter" (Decode.option Decode.int) None
          |> Decode.required "sync" (Decode.list Decode.string)
     with err ->
      failwith err.Message

type ReplicationCompleteDocument =
  {
    DocWriteFailures:float
    DocsRead:float
    DocsWritten:float
//    LastSeq:string
    StartTime:System.DateTime
    Ok:bool
    Status:string
    EndTime:System.DateTime
  }
  static member Decoder =
    try
      Decode.decode
          (fun dwf dr dw st ok status et->
              {
                DocWriteFailures=dwf
                DocsRead=dr
                DocsWritten=dw
//                LastSeq = lq
                StartTime = System.DateTime.Parse st
                Ok = ok
                Status = status
                EndTime= System.DateTime.Parse et
              } : ReplicationCompleteDocument)
          |> Decode.required "doc_write_failures" Decode.float
          |> Decode.required "docs_read" Decode.float
          |> Decode.required "docs_written" Decode.float
//          |> Decode.required "last_seq" (Decode.oneOf [Decode.string;Decode.int])
          |> Decode.required "start_time" Decode.string
          |> Decode.required "ok" Decode.bool
          |> Decode.required "status" Decode.string
          |> Decode.required "end_time" Decode.string
     with err ->
      failwith err.Message

