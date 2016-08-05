
import threading as _threading
_is_old = 3 / 2 == 1
if _is_old:
  import urllib as _urllib
  import urllib2 as _urllib2
  import urlparse as _urlparse
else:
  import urllib as _urllib
  import urllib.parse as _urllib_parse
  import urllib.request as _urllib_request

def _parse_url(url):
  return _urlparse.urlparse(url)

def decode_url_value(value):
  if _is_old:
    return _urllib.unquote(value).decode('utf8')
  else:
    return _urllib_parse.unquote(value)

def encode_url_value(value):
  if _is_old:
    return _urllib2.quote(value.encode('utf8'))
  else:
    return _urllib_parse.quote(value)

def _send_impl(crayon_obj, req_obj, method, url, headers, content):
  if _is_old:
    opener = _urllib2.build_opener(_urllib2.HTTPHandler)
    if content == None:
      request = _urllib2.Request(url)
    else:
      request = _urllib2.Request(url, data=content)
  else:
    opener = _urllib_request.build_opener(_urllib_request.HTTPHandler)
    if content == None:
      request = _urllib_request.Request(url)
    else:
      request = _urllib_request.Request(url, data=content)
  for header in headers:
    request.add_header(header[0], header[1])
  request.get_method = lambda:method
  output = opener.open(request)
  content = output.read()
  headers = {}
  for header_key in output.headers.keys():
    headers[header_key] = output.headers[header_key]
  response_message = output.msg
  response_code = output.code
  req_obj._set_result(response_code, response_message, content, headers)
  _push_async_message_queue([%%%ASYNC_MESSAGE_TYPE_HTTP_RESPONSE%%%,
          crayon_obj,
          response_code,
          response_message,
          content,
          None]) # headers string-string dictionary

class HttpAsyncRequest:
  def __init__(self, crayon_obj, url):
    self.crayon_obj = crayon_obj
    bad_format = False
    try:
      if _is_old:
        url_parts = _parse_url(url)
      else:
        url_parts = _urllib_parse.urlparse(url)
      if url_parts.scheme == '' or url_parts.netloc == '':
        bad_format = True
    except:
      bad_format = True
    if bad_format:
      raise Exception("Bad URL! Bad!")

    self.mutex = _threading.Lock()
    self.method = 'GET'
    self.scheme = url_parts.scheme
    self.host = url_parts.hostname
    self.port = url_parts.port
    self.path = url_parts.path
    self.fragment = url_parts.fragment
    self.params = url_parts.params
    self.original_query = url_parts.query # use this if query params are not modified
    self.query = None # if modified, clear original_query and populate this with a dictionary lookup
    self.header_formatting = {} # preserves the formatting of the header key
    self.header_values = {} # canonical key of header with list of values of that header
    self.content = None
    self.done = False
    self.response_code = -1
    self.response_message = None
    self.response_content = None
    self.response_headers_values = None
    self.response_headers_formatting = None

  def send(self):
    url = self.scheme + '://' + self.host

    if self.port != None:
      url += ':' + str(self.port)

    if self.path != None and self.path != '':
      if self.path[0] != '/':
        self.path = '/' + self.path
      url += self.path

    if self.params != None and self.params != '':
      url += ';' + self.params

    if self.query == None:
      if self.original_query != '':
        url += '?' + self.original_query
    else:
      queries = []
      keys = self.query.keys()[:]
      keys.sort() # deterministic requests
      for key in keys:
        e_key = encode_url_value(key)
        for value in self.query[key]:
          e_value = encode_url_value(value)
          queries.append(e_key + '=' + e_value)
      url += '?' + '&'.join(queries)

    if self.fragment != '':
      url += '#' + self.fragment

    headers = []
    keys = list(self.header_formatting.keys())
    keys.sort()
    for key in keys:
      f_key = self.header_formatting[key]
      for value in self.header_values[key]:
        headers.append((f_key, value))


    thread = _threading.Thread(target = _send_impl, args = (self.crayon_obj, self, self.method, url, headers, self.content))
    thread.daemon = True
    thread.start()

  def _set_result(self, code, message, content, headers):
    self.mutex.acquire()
    try:
      self.response_code = code
      self.response_message = message
      self.response_content = content
      self.response_headers_values = {}
      self.response_headers_formatting = {}
      for key in headers.keys():
        ckey = key.lower()
        self.response_headers_values[ckey] = headers[key]
        self.response_headers_formatting[ckey] = key
    finally:
      self.mutex.release()

  def is_complete(self):
    self.mutex.acquire()
    try:
      return self.response_code != -1
    finally:
      self.mutex.release()

  def _ensure_request_complete(self):
    if not self.is_complete():
      raise Exception("Cannot access response until request is complete.")

  def get_response_code(self):
    self._ensure_request_complete()
    return self.response_code

  def get_response_message(self):
    self._ensure_request_complete()
    return self.response_message

  def get_response_header_names(self):
    self._ensure_request_complete()
    output = list(self.response_headers_formatting.values())
    output.sort()
    return output

  def get_response_header(self, name):
    self._ensure_request_complete()
    return self.response_headers_values.get(name.lower(), None)

  def get_response_content(self, mode='t'):
    self._ensure_request_complete()
    output = self.response_content
    if mode == 't':
      return output.decode('utf-8')
    else:
      return output

  def set_header(self, key, value):
    self.header_formatting[key.lower()] = key
    self.header_values[key.lower()] = [value]

  def add_header(self, key, value):
    canonical_key = key.lower()
    existing_headers = self.header_values.get(canonical_key, None)
    if existing_headers == None:
      self.set_header(key, value)
    else:
      self.header_values[key.lower()][0] += "\r\n" + key + ": " + value # I am a terrible person. So very terrible.
      #existing_headers.append(value)

  def clear_header(self, key):
    canonical_key = key.lower()
    if self.header_values.get(canonical_key, None) != None:
      self.header_values.pop(canonical_key)
      self.header_formatting.pop(canonical_key)

  def set_method(self, method):
    self.method = method

  def set_content(self, content):
    self.content = content

  def _init_query(self):
    if self.query == None:
      query = [] if self.original_query != '' else self.original_query.split('&')
      lookup_values = {}
      for item in query:
        parts = item.split('=')
        if len(parts) >= 2:
          item_key = decode_url_value(parts[0])
          item_value = decode_url_value('='.join(parts[1:]))
          existing_values = lookup_values.get(item_key, None)
          if existing_values == None:
            existing_values = []
            lookup_values[item_key] = existing_values
          existing_values.append(item_value)
      self.query = lookup_values

  def set_query(self, key, value):
    self._init_query()
    self.query[key] = [value]

  def add_query(self, key, value):
    self._init_query()
    values = self.query.get(key, None)
    if values != None:
      values.append(value)
    else:
      self.query[key] = [value]

  def clear_query(self, key):
    self._init_query()
    if self.query.get(key, None) != None:
      self.query.pop(key)

  def set_port(self, port):
    self.port = port

  def set_fragment(self, fragment):
    self.fragment = fragment

  def clear_fragment(self):
    self.fragment = None

  def set_scehem(self, scheme):
    self.scheme = scheme
