import os
import sys
import shutil

def normalize_path(dir):
  return os.sep.join(dir.split('/'))

def main():
  files = {}

  ops = [
    { "from_dir": '../U3/js',
      "to_dir": '../Compiler/U3/u3',
    },
  ]

  for op in ops:
    from_dir = op['from_dir']
    to_dir = op['to_dir']
    for file in os.listdir(from_dir):
      from_file = normalize_path(from_dir + '/' + file)
      to_file = normalize_path(to_dir + '/' + file)
      print(from_file + ' --> ' + to_file)
      shutil.copy(from_file, to_file)

  print("Done.")

main()
