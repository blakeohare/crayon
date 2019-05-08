import os
import sys

MSBUILD = r'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
RELEASE_CONFIG = '/p:Configuration=Release'
PASTEL_SLN = r'..\Compiler\Pastel\Pastel.sln'
PASTEL_EXE = r'..\Compiler\Pastel\bin\Release\Pastel.exe'
LIBRARIES_DIR = r'..\Libraries'
INTERPRE_DIR = r'..\Interpreter' # lol, "interpre-dir"

PLATFORMS = [
  'lang-csharp',
  'lang-java',
  'lang-javascript',
  'lang-python',
  'csharp-app',
  'java-app',
  'javascript-app',
  'python-app',
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
  cmd = MSBUILD + ' ' + os.path.abspath(PASTEL_SLN) + ' ' + RELEASE_CONFIG
  pastel_exe = os.path.abspath(PASTEL_EXE)
  print(run_command(cmd))
  print(cmd)
  things = get_libraries()
  things.append(get_interpreter())
  for lib in things:
    for manifest, path in lib['manifests']:
      print("Exporting: " + lib['name'] + ' --> ' + manifest)
      cmd = pastel_exe + ' ' + path
      print(run_command(cmd))

main(sys.argv[1:])
