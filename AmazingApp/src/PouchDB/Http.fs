module PouchDB.Http

open Fable.Core

(*
  Source: https://github.com/SuaveIO/suave/blob/1ba57887806ba3682221afbcc65cae49f1cbad38/src/Suave/Http.fs
*)

type HttpStatus =
  { code   : int
    reason : string
  }
  static member code_ = (fun x -> x.code), fun v x -> { x with code = v }
  static member reason_ = (fun x -> x.reason), fun v x -> { x with reason = v }

type HttpCode =
  | HTTP_100 | HTTP_101
  | HTTP_200 | HTTP_201 | HTTP_202 | HTTP_203 | HTTP_204 | HTTP_205 | HTTP_206
  | HTTP_300 | HTTP_301 | HTTP_302 | HTTP_303 | HTTP_304 | HTTP_305 | HTTP_306
  | HTTP_307 | HTTP_400 | HTTP_401 | HTTP_402 | HTTP_403 | HTTP_404 | HTTP_405
  | HTTP_406 | HTTP_407 | HTTP_408 | HTTP_409 | HTTP_410 | HTTP_411 | HTTP_412
  | HTTP_413 | HTTP_422 | HTTP_426 | HTTP_428 | HTTP_429 | HTTP_414 | HTTP_415
  | HTTP_416 | HTTP_417 | HTTP_451 | HTTP_500 | HTTP_501 | HTTP_502 | HTTP_503
  | HTTP_504 | HTTP_505

  member x.code =
    match x with
    | HTTP_100 -> 100 | HTTP_101 -> 101 | HTTP_200 -> 200 | HTTP_201 -> 201
    | HTTP_202 -> 202 | HTTP_203 -> 203 | HTTP_204 -> 204 | HTTP_205 -> 205
    | HTTP_206 -> 206 | HTTP_300 -> 300 | HTTP_301 -> 301 | HTTP_302 -> 302
    | HTTP_303 -> 303 | HTTP_304 -> 304 | HTTP_305 -> 305 | HTTP_306 -> 306
    | HTTP_307 -> 307 | HTTP_400 -> 400 | HTTP_401 -> 401 | HTTP_402 -> 402
    | HTTP_403 -> 403 | HTTP_404 -> 404 | HTTP_405 -> 405 | HTTP_406 -> 406
    | HTTP_407 -> 407 | HTTP_408 -> 408 | HTTP_409 -> 409 | HTTP_410 -> 410
    | HTTP_411 -> 411 | HTTP_412 -> 412 | HTTP_413 -> 413 | HTTP_414 -> 414
    | HTTP_415 -> 415 | HTTP_416 -> 416 | HTTP_417 -> 417 | HTTP_422 -> 422
    | HTTP_426 -> 426 | HTTP_428 -> 428 | HTTP_429 -> 429 | HTTP_451 -> 451
    | HTTP_500 -> 500 | HTTP_501 -> 501 | HTTP_502 -> 502 | HTTP_503 -> 503
    | HTTP_504 -> 504 | HTTP_505 -> 505

  member x.reason =
    match x with
    | HTTP_100 -> "Continue"
    | HTTP_101 -> "Switching Protocols"
    | HTTP_200 -> "OK"
    | HTTP_201 -> "Created"
    | HTTP_202 -> "Accepted"
    | HTTP_203 -> "Non-Authoritative Information"
    | HTTP_204 -> "No Content"
    | HTTP_205 -> "Reset Content"
    | HTTP_206 -> "Partial Content"
    | HTTP_300 -> "Multiple Choices"
    | HTTP_301 -> "Moved Permanently"
    | HTTP_302 -> "Found"
    | HTTP_303 -> "See Other"
    | HTTP_304 -> "Not Modified"
    | HTTP_305 -> "Use Proxy"
    | HTTP_306 -> "Unused"
    | HTTP_307 -> "Temporary Redirect"
    | HTTP_400 -> "Bad Request"
    | HTTP_401 -> "Unauthorized"
    | HTTP_402 -> "Payment Required"
    | HTTP_403 -> "Forbidden"
    | HTTP_404 -> "Not Found"
    | HTTP_405 -> "Method Not Allowed"
    | HTTP_406 -> "Not Acceptable"
    | HTTP_407 -> "Proxy Authentication Required"
    | HTTP_408 -> "Request Timeout"
    | HTTP_409 -> "Conflict"
    | HTTP_410 -> "Gone"
    | HTTP_411 -> "Length Required"
    | HTTP_412 -> "Precondition Failed"
    | HTTP_413 -> "Request Entity Too Large"
    | HTTP_414 -> "Request-URI Too Long"
    | HTTP_415 -> "Unsupported Media Type"
    | HTTP_416 -> "Requested Range Not Satisfiable"
    | HTTP_417 -> "Expectation Failed"
    | HTTP_422 -> "Unprocessable Entity"
    | HTTP_426 -> "Upgrade Required"
    | HTTP_428 -> "Precondition Required"
    | HTTP_429 -> "Too Many Requests"
    | HTTP_451 -> "Unavailable For Legal Reasons"
    | HTTP_500 -> "Internal Server Error"
    | HTTP_501 -> "Not Implemented"
    | HTTP_502 -> "Bad Gateway"
    | HTTP_503 -> "Service Unavailable"
    | HTTP_504 -> "Gateway Timeout"
    | HTTP_505 -> "HTTP Version Not Supported"

  member x.message =
    match x with
    | HTTP_100 -> "Request received, please continue"
    | HTTP_101 -> "Switching to new protocol; obey Upgrade header"
    | HTTP_200 -> "Request fulfilled, document follows"
    | HTTP_201 -> "Document created, URL follows"
    | HTTP_202 -> "Request accepted, processing continues off-line"
    | HTTP_203 -> "Request fulfilled from cache"
    | HTTP_204 -> "Request fulfilled, nothing follows"
    | HTTP_205 -> "Clear input form for further input."
    | HTTP_206 -> "Partial content follows."
    | HTTP_300 -> "Object has several resources -- see URI list"
    | HTTP_301 -> "Object moved permanently -- see URI list"
    | HTTP_302 -> "Object moved temporarily -- see URI list"
    | HTTP_303 -> "Object moved -- see Method and URL list"
    | HTTP_304 -> "Document has not changed since given time"
    | HTTP_305 -> "You must use proxy specified in Location to access this resource."
    | HTTP_306 -> "Unused is a proposed extension to the HTTP/1.1 specification that is not fully specified."
    | HTTP_307 -> "Object moved temporarily -- see URI list"
    | HTTP_400 -> "Bad request syntax or unsupported method"
    | HTTP_401 -> "No permission -- see authorization schemes"
    | HTTP_402 -> "No payment -- see charging schemes"
    | HTTP_403 -> "Request forbidden -- authorization will not help"
    | HTTP_404 -> "Nothing matches the given URI"
    | HTTP_405 -> "Specified method is invalid for this resource."
    | HTTP_406 -> "URI not available in preferred format."
    | HTTP_407 -> "You must authenticate with this proxy before proceeding."
    | HTTP_408 -> "Request timed out; try again later."
    | HTTP_409 -> "Request conflict."
    | HTTP_410 -> "URI no longer exists and has been permanently removed."
    | HTTP_411 -> "Client must specify Content-Length."
    | HTTP_412 -> "Precondition in headers is false."
    | HTTP_413 -> "Entity is too large."
    | HTTP_414 -> "URI is too long."
    | HTTP_415 -> "Entity body in unsupported format."
    | HTTP_416 -> "Cannot satisfy request range."
    | HTTP_417 -> "Expect condition could not be satisfied."
    | HTTP_422 -> "The entity sent to the server was invalid."
    | HTTP_426 -> "Upgrade Required indicates that the client should switch to a different protocol such as TLS/1.0."
    | HTTP_428 -> "You should verify the server accepts the request before sending it."
    | HTTP_429 -> "Request rate too high, chill out please."
    | HTTP_451 -> "The server is subject to legal restrictions which prevent it servicing the request"
    | HTTP_500 -> "Server got itself in trouble"
    | HTTP_501 -> "Server does not support this operation"
    | HTTP_502 -> "Invalid responses from another server/proxy."
    | HTTP_503 -> "The server cannot process the request due to a high load"
    | HTTP_504 -> "The gateway server did not receive a timely response"
    | HTTP_505 -> "Cannot fulfill request."

  member x.describe () =
    sprintf "%d %s: %s" x.code x.reason x.message

  member x.status = { code = x.code; reason = x.reason }
  static member tryParse (code : int) =
    let found =
      HttpCodeStatics.mapCases
      |> Map.tryFind ("HTTP_" + string code)

    match found with
    | Some x -> x
    | None ->
      failwith (sprintf "Couldn't convert %i to HttpCode." code)

and private HttpCodeStatics() =
  static member val mapCases : Map<string,HttpCode> =
    Reflection.FSharpType.GetUnionCases(typeof<HttpCode>)
    |> Array.map (fun case -> case.Name, Reflection.FSharpValue.MakeUnion(case, [||]) :?> HttpCode)
    |> Map.ofArray
