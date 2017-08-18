# Http Library

Sends both text and binary HTTP requests either synchronously or asynchronously.

The default User-Agent is `Crayon/v0.2.0; Have a nice day.` unless the platform does not have a mandated User-Agent value (i.e. JavaScript running in browsers). 

# Class: HttpRequest

### constructor

`new HttpRequest(url)`

Creates a new HTTP request object that can be sent to the given URL. No request is sent until [send](#send) or [sendAsync](#sendasync) is invoked.

---

## Methods

### setMethod

`request.setMethod(method)`

Sets the method to use for the HTTP request. 

| Argument | Type | Description |
| --- | --- | --- |
| **method** | _[Method](#method) OR string_ | HTTP Method to use. |

**Return value**: returns itself (the HttpRequest) to allow for builder syntax.

---

### setHeader

`request.setHeader(name, value)`

Sets an HTTP header.

| Argument | Type | Description |
| --- | --- | --- |
| **name** | _string_ | Name of the header. e.g. `Content-Type` |
| **value** | _anything_ | Value of the header. Generally a string, but any value will be auto-converted. |

**Return value**: returns itself (the HttpRequest) to allow for builder syntax.

---

### setContentBytes

`request.setContentBytes(bytes)` or `request.setContentBytes(bytes, contentType)`

Sets the body of the request as binary data.

Optionally allows setting the **Content-Type** header at the same time.

| Argument | Type | Description |
| --- | --- | --- |
| **bytes** | _list of integers_ | A list of byte values (0-255) |
| **contentType** | _string_ | The Content-Type of the request. |

**Return value**: returns itself (the HttpRequest) to allow for builder syntax.

---

### setContent

`request.setContent(text)` or `request.setContent(text, contentType)`

Sets the body of the request as a text string.

Optionally allows setting the **Content-Type** header at the same time.

| Argument | Type | Description |
| --- | --- | --- |
| **text** | _string_ | Text to send in the HTTP body of the request. |
| **contentType** | _string_ | The Content-Type of the request. |

**Return value**: returns itself (the HttpRequest) to allow for builder syntax.

---

### send

`request.send()`

Sends the request and waits for a response, which is returned.

**Return value**: a [HttpResponse](#class-httpresponse) instance.

---

### sendAsync

`request.sendAsync()`

Sends the request but doesn't wait for a response. 

**Return value**: returns itself (the HttpRequest) as this is intended to be the end of the chain of a builder syntax expression.

---

### isDone

`request.isDone()`

Checks to see if the asynchronous request has received a response.

**Return value**: a boolean

---

### getResponse

`request.getResponse()`

Returns the [HttpResponse](#class-httpresponse) object after an asynchronous request has completed.

See also: [HttpRequest.isDone()](#isdone)

---

# Class: HttpResponse

Represents an HTTP response from a request.

## Methods

### getHeaderNames

`response.getHeaderNames()`

Returns a list of HTTP header names in the response.

**Return value**: list of strings.

---

### getHeaders

`response.getHeaders(name)`

Returns all the values for all headers that match this name as a list. 

| Argument | Type | Description |
| --- | --- | --- |
| **name** | _string_ | Name of the headers to match (case-insensitive). |

**Return type**: list of strings

---

### getHeader

`response.getHeader(name)`

Returns the value of the first header that has this name. Returns `null` if there are none.

| Argument | Type | Description |
| --- | --- | --- |
| **name** | _string_ | Name of the header to fetch (case-insensitive). |

---

### getContent

`response.getContent()`

Returns the content of the response as a string or a list of bytes if the content is binary content.

**Return type**: string, list of integers, or `null`

---

### getStatusCode

`response.getStatusCode()`

Returns the status code of the response.

**Return value**: integer corresponding to a [StatusCode](#enum-statuscode) value.

### getStatusMessage

`response.getStatusMessage()`

Returns the status message of the response. This is generall used in conjunction with the status code.

e.g. the "FORBIDDEN" part of "403 FORBIDDEN"

**Return type**: string

---

# Enum: Method

Enum value defining common 4 HTTP methods. However, all arguments that consume a method can either consume a Method enum OR a string, allowing more obscure methods such as `HEAD` or `PATCH` to be used as well.

| Value | Description |
| --- | --- |
| **GET** | Default method when not specified. |
| **POST** | |
| **PUT** | |
| **DELETE** | |

# Enum: StatusCode

An enum alias of standard HTTP status codes.

| Name | Integer Value | Description |
| --- | --- | --- |
| CONTINUE | 100 | |
| SWITCHING_PROTOCOLS | 101 | |
| PROCESSING | 102 | |
| CHECKPOINT | 103 | |
| OK | 200 | |
| CREATED | 201 | |
| ACCEPTED | 202 | |
| NON_AUTHORITATIVE_INFORMATION | 203 | |
| NO_CONTENT | 204 | |
| RESET_CONTENT | 205 | |
| PARTIAL_CONTENT | 206 | |
| MULTI_STATUS | 207 | |
| ALREADY_REPORTED | 208 | |
| IM_USED | 226 | |
| MULTIPLE_CHOICES | 300 | |
| MOVED_PERMANENTLY | 301 | |
| MOVED_TEMPORARILY | 302 | |
| SEE_OTHER | 303 | |
| NOT_MODIFIED | 304 | |
| USE_PROXY | 305 | |
| SWITCH_PROXY | 306 | |
| TEMPORARY_REDIRECT | 307 | |
| PERMANENT_REDIRECT | 308 | |
| BAD_REQUEST | 400 | |
| UNAUTHORIZED | 401 | |
| PAYMENT_REQUIRED | 402 | |
| FORBIDDEN | 403 | |
| NOT_FOUND | 404 | |
| METHOD_NOT_ALLOWED | 405 | |
| NOT_ACCEPTABLE | 406 | |
| PROXY_AUTHENTICATION_REQUIRED | 407 | |
| REQUEST_TIMEOUT | 408 | |
| CONFLICT | 409 | |
| GONE | 410 | |
| LENGTH_REQUIRED | 411 | |
| PRECONDITION_FAILED | 412 | |
| PAYLOAD_TOO_LARGE | 413 | |
| URI_TOO_LONG | 414 | |
| UNSUPPORTED_MEDIA_TYPE | 415 | |
| RANGE_NOT_SATISFIABLE | 416 | |
| EXPECTATION_FAILED | 417 | |
| IM_A_TEAPOT | 418 | |
| ENHANCE_YOUR_CALM | 420 | |
| MISDIRECTED_REQUEST | 421 | |
| UNPROCESSABLE_ENTITY | 422 | |
| LOCKED | 423 | |
| FAILED_DEPENDENCY | 424 | |
| UPGRADE_REQUIRED | 426 | |
| PRECONDITION_REQUIRED | 428 | |
| TOO_MANY_REQUESTS | 429 | |
| REQQUEST_HEADER_FIELDS_TOO_LARGE | 431 | |
| LOGIN_TIMEOUT | 440 | |
| RETRY_WITH | 449 | |
| BLOCKED_BY_PARENTAL_CONTROLS | 450 | |
| UNAVAILABLE_FOR_LEGAL_REASONS | 451 | |
| INTERNAL_SERVER_ERROR | 500 | |
| NOT_IMPLEMENTED | 501 | |
| BAD_GATEWAY | 502 | |
| SERVICE_UNAVAILABLE | 503 | |
| GATEWAY_TIMEOUT | 504 | |
| HTTP_VERSION_NOT_SUPPORTED | 505 | |
| VARIANT_ALSO_NEGOTIATES | 506 | |
| INSUFFICIENT_STORAGE | 507 | |
| LOOP_DETECTED | 508 | |
| BANDWIDTH_LIMIT_EXCEEDED | 509 | |
| NOT_EXTENDED | 510 | |
| NETWORK_AUTHENTICATION_REQIURED | 511 | |
