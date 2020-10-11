try:
  from BaseHTTPServer import BaseHTTPRequestHandler, HTTPServer
except:
  from http.server import BaseHTTPRequestHandler, HTTPServer

import os
import sys
import threading

CONTENT_TYPES = {
  '.htm': 'text/html',
  '.html': 'text/html',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.js': 'application/javascript',
  '.json': 'application/json',
  '.mp3': 'audio/mpeg',
  '.ogg': 'audio/ogg',
  '.png': 'image/png',
  '.txt': 'text/plain',
}

class SimpleCrayonFileServer(BaseHTTPRequestHandler):

  def _get_content_type(self, filename):
    ext = filename.lower().split('.')[-1]
    return CONTENT_TYPES.get('.' + ext, 'text/plain')

  def _set_headers(self, sc = 200, ct = None):
    self.send_response(sc)
    content_type = ct
    if ct == None: content_type = 'text/html'
    self.send_header('Content-type', content_type)
    self.end_headers()

  def _set_not_found(self, headers_only):
    self._set_headers(404)
    if not headers_only:
      self.wfile.write("Not found".encode('utf-8'))

  def do_GET(self):
    self._do_get(False)
  def do_HEAD(self):
    self._do_get(True)

  def _do_get(self, headers_only):

    path = self.path[1:].split('?')[0].split('#')[0]

    if '/resources/' in path:
      path = 'resources/' + path.split('/resources/')[-1]
    if path == '':
      file = 'index.html'
    else:
      path_parts = path.split('/')
      if '..' in path_parts or '.' in path_parts:
        self.set_not_found(headers_only)
        return
      filepath = os.sep.join(path_parts)
      if not os.path.exists(filepath) or os.path.isdir(filepath):
        self._set_not_found(headers_only)
        return
      file = filepath

    content_type = self._get_content_type(file)
    self._set_headers(200, content_type)

    if not headers_only:
      with open(file, 'rb') as c:
        self.wfile.write(c.read())

def run(port):

  server = HTTPServer(('', port), SimpleCrayonFileServer)

  # HTTPServer catches all exceptions and ignores them, including
  # the keyboard interrupt exception. By running the server in a daemon
  # thread, the main thread can still be killed from the command line.
  thread = threading.Thread(target = server.serve_forever)
  thread.daemon = True
  thread.start()

  print("Test server is now running.")
  print("Visit: http://localhost:" + str(port))
  print("Press Ctrl + C to stop.\n")

  try:
    while True:
      pass
  except KeyboardInterrupt:
    print("Shutting down.")

def main(args):
  port = 8080
  if len(args) == 1:
    port = int(args[0])
  run(port)

if __name__ == "__main__":
  main(sys.argv[1:])
