import os
import sys
import shutil

def main():
  files = {}

  web_dest_dir = os.path.join('..', 'Compiler', 'Platforms', 'JavaScriptApp', 'ResourcesU3')

  from_dir = os.path.join('..', 'U3', 'src', 'render')

  for file in [
    'nori.js',
    'nori_audio.js',
    'nori_canvas.js',
    'nori_context.js',
    'nori_events.js',
    'nori_gamepad.js',
    'nori_gl.js',
    'nori_layout.js',
    'nori_util.js'
  ]:
    from_file = os.path.join(from_dir, file)
    files[from_file] = [
      os.path.join(web_dest_dir, file.replace('.', '_') + '.txt')
    ]

  msghub_client = os.path.join('..', 'Libraries', 'MessageHub', 'client')
  files[os.path.join(msghub_client, 'js', 'messagehub.js')] = [
    os.path.join(web_dest_dir, 'messagehub_js.txt')
  ]

  msghub_nodejs_client = os.path.join(msghub_client, 'nodejs', 'messagehubclient')
  print(msghub_nodejs_client )
  for file in os.listdir(msghub_nodejs_client):
    files[os.path.join(msghub_nodejs_client, file)] = [os.path.join('..', 'U3', 'src', 'messagehubclient', file)]

  for src_file in files.keys():
    for dst_file in files[src_file]:
      shutil.copy(src_file, dst_file)
      print(src_file + ' --> ' + dst_file)

  print("Done.")

main()
