import os
import platform
import sys

LIBRARIES_DIR = os.path.join('..', 'Libraries')
INTERPRE_DIR = os.path.join('..', 'Interpreter') # lol, "interpre-dir"

PLATFORMS = [
  'lang-csharp',
  'lang-javascript',
  'csharp-app',
  'javascript-app',
  'javascript-app-android',
  'javascript-app-ios',
]

PLATFORMS_LOOKUP = {}
for p in PLATFORMS:
  PLATFORMS_LOOKUP[p] = True

def run_command(cmd):
  c = os.popen(cmd)
  output = c.read()
  c.close()
  return output

def get_libraries():
  output = []
  for lib_dir in os.listdir(LIBRARIES_DIR):
    d = os.path.join(os.path.abspath(LIBRARIES_DIR), lib_dir)
    pastel_dir = os.path.join(d, 'pastel')
    if os.path.isdir(pastel_dir):
      manifests = []
      for txt in os.listdir(pastel_dir):
        if (txt.endswith('.txt') and PLATFORMS_LOOKUP.get(txt.split('.')[0], False)):
          manifests.append((txt, os.path.join(pastel_dir, txt)))

      output.append({
        'name': lib_dir,
        'manifests': manifests
      })
  return output

def get_interpreter():
  manifests = []
  d = os.path.abspath(INTERPRE_DIR)
  for file in os.listdir(d):
    if file.endswith('.txt') and PLATFORMS_LOOKUP.get(file.split('.')[0], False):
      manifests.append((file, os.path.join(d, file)))
  return {
    'name': "Interpreter",
    'manifests': manifests
  }

def main(args):

  p = platform.system()
  isWindows = p == 'Windows'
  isMac = p == 'Darwin'
  isLinux = p == 'Linux'
  if not isWindows and not isMac and not isLinux:
    print("Unknown platform: " + p)
    return
  if isLinux:
    print("Linux not currently supported")
    return

  pastelSource = os.environ['PASTEL_SOURCE']

  if pastelSource == None:
    print("PASTEL_SOURCE environment variable must be set. This should be the root of the https://github.com/blakeohare/pastel repository")
    return

  if len(args) > 1:
    print("Too many args!")
    return

  lib_filter = None
  if len(args) == 1:
    lib_filter = args[0]

  binary_name = 'Pastel.exe' if isWindows else 'Pastel'

  pastel_sln = os.path.join(pastelSource, 'Source', 'Pastel.sln')
  pastel_exe = [
    os.path.join(pastelSource, 'Source', 'bin', 'Release', 'netcoreapp3.1', binary_name),
    os.path.join(pastelSource, 'Source', 'bin', 'Release', 'netcoreapp3.1', 'osx-x64', 'publish', binary_name),
  ][isMac] # I'm not entirely sure why the pattern is different

  cmd = ' '.join([
    'dotnet build',
    pastel_sln,
    '-c Release',
  ])

  print(run_command(cmd))
  print(cmd)
  things = get_libraries()
  things.append(get_interpreter())
  for lib in things:
    if lib_filter == None or lib_filter.strip().lower() in lib['name'].lower():
      for manifest, path in lib['manifests']:
        print("Exporting: " + lib['name'] + ' --> ' + manifest)
        cmd = pastel_exe + ' ' + path
        print(run_command(cmd))

main(sys.argv[1:])
