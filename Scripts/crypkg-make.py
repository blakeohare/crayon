import os
import sys

def gather_files(path):
  output = []
  gather_files_impl(output, path, len(path) + 1)
  return output

BANNED_FILES = ['thumbs.db', '.ds_store']
def gather_files_impl(output, current_path, trim_length):
  for filename in os.listdir(current_path):
    if filename.lower() in BANNED_FILES: continue
    full_path = os.path.join(current_path, filename)
    if os.path.isdir(full_path):
      gather_files_impl(output, full_path, trim_length)
    else:
      relative_path = full_path[trim_length:]
      output.append((relative_path.replace('\\', '/'), full_path))

def main(args):
  if len(args) != 1:
    print("Usage: python crypkg-make.py directory")
    return

  path = args[0]
  if not os.path.isabs(path):
    path = os.path.join(os.getcwd(), path)

  path = os.path.abspath(path)

  if not os.path.isdir(path):
    print(path + " is not a directory")
    return

  parts = path.split(os.sep)
  last_dir = parts[-1]
  parent_dir = os.sep.join(parts[:-1])
  crypkg_path = path + '.crypkg'
  print("Packaging files into : " + crypkg_path)

  files = gather_files(path)
  files.sort(key = lambda x: x[0])

  header = []
  payload = []

  for file_path, disk_path in files:
    path_bytes = str_to_bytes(file_path)
    payload_offset = len(payload)

    c = open(disk_path, 'rb')
    for b in c.read():
      payload.append(b)
    c.close()
    file_length = len(payload) - payload_offset

    header.extend(int32_to_bytes(len(path_bytes)))
    header.extend(int32_to_bytes(file_length))
    header.extend(int32_to_bytes(payload_offset))
    header.extend(path_bytes)

  output = int32_to_bytes(len(files))
  output.extend(header)
  output.extend(payload)

  output_bytes = bytearray(output)

  c = open(crypkg_path, 'wb')
  c.write(output_bytes)
  c.close()
  print(str(len(output)) + ' bytes written')

def str_to_bytes(s):
  output = []
  for b in bytearray(s, 'utf8'):
    output.append(int(b))
  return output

def int32_to_bytes(n):
  return [
    (n >> 24) & 255,
    (n >> 16) & 255,
    (n >> 8) & 255,
    n & 255
  ]

main(sys.argv[1:])
