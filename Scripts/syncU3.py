import os
import sys
import shutil

def main():
  files = {}

  web_dest_dir = os.path.join('..', 'Compiler', 'Platforms', 'JavaScriptApp', 'ResourcesU3')

  from_dir = os.path.join('..', 'U3', 'src', 'render')

  for file in [
    'nori.js',
    'nori_canvas.js',
    'nori_context.js',
    'nori_events.js',
    'nori_layout.js',
    'nori_util.js'
  ]:
    from_file = os.path.join(from_dir, file)
    files[from_file] = [
      os.path.join(web_dest_dir, file.replace('.', '_') + '.txt')
    ]

  files[os.path.join('..', 'Libraries', 'MessageHub', 'client', 'js', 'messagehub.js')] = [
    os.path.join(web_dest_dir, 'messagehub_js.txt')
  ]

  for src_file in files.keys():
    for dst_file in files[src_file]:
      shutil.copy(src_file, dst_file)
      print(src_file + ' --> ' + dst_file)

  print("Done.")

main()
