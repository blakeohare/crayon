import os
import sys

def get_all_code_files():
  paths = []
  scripts = os.getcwd()
  if not os.path.exists(os.path.join(scripts, 'updateversion.py')):
    return None
  root = os.path.abspath(os.path.join(os.getcwd(), '..'))
  get_all_code_files_impl(root, '', paths)
  return paths

def get_all_code_files_impl(abs, rel, output):
  for file in os.listdir(abs):
    abs_file = os.path.join(abs, file)
    rel_file = file if len(rel) == 0 else (rel + '/' + file)
    if os.path.isdir(abs_file):
      get_all_code_files_impl(abs_file, rel_file, output)
    else:
      output.append((abs_file, rel_file))

# These two functions won't give the correct line-ending/encoding format
# but are simple. Just run the format-fixer.py script again before committing.
def read_file(path):
  c = open(path, 'rt')
  t = c.read()
  c.close()
  return t
def write_file(path, content):
  c = open(path, 'wt')
  c.write(content)
  c.close()

def parse_version(args):
  if len(args) != 3: return None
  a = int(args[0])
  b = int(args[1])
  c = int(args[2])
  ver = (a, b, c)
  if None in ver:
    return None
  return ver

def update_assemblyInfo_cs(version_string, path):
  lines = read_file(path).split('\n')
  for i in range(len(lines)):
    line = lines[i]
    if not line.startswith('//') and 'assembly: AssemblyVersion(' in line or 'assembly: AssemblyFileVersion(' in line:
      t = line.split('"')
      t[1] = version_string
      lines[i] = '"'.join(t)
  write_file(path, '\n'.join(lines))

def update_common_versionInfo_cs(version, path):
  lines = read_file(path).split('\n')
  for i in range(len(lines)):
    line = lines[i]
    if '_SCRIPT_UPDATE' in line:
      t1 = line.split(';')
      t2 = t1[0].split(' ')
      v = version[0]
      if 'VERSION_MINOR' in line:
        v = version[1]
      elif 'VERSION_BUILD' in line:
        v = version[2]
      t2[-1] = str(v)
      t1[0] = ' '.join(t2)
      line = ';'.join(t1)
      lines[i] = line
  write_file(path, '\n'.join(lines))

def update_vmProgramCs(version, path):
  lines = read_file(path).split('\n')
  ok = False
  for i in range(len(lines)):
    line = lines[i]
    if 'private static readonly HashSet<string> COMPATIBLE_VERSIONS = new HashSet<string>()' in line:
      ok = True
      if lines[i + 2].strip() != '};':
        ok = False
        break

      line = lines[i + 1]
      t = line.split('"')
      t[1] = version
      line = '"'.join(t)
      lines[i + 1] = line
  write_file(path, '\n'.join(lines))

def update_lib_manifest(version, path):
  lines = read_file(path).split('\n')
  for i in range(len(lines)):
    line = lines[i]
    tline = line.strip()
    if tline.startswith('"documentation":') and 'crayonlang.org/docs/' in tline:
      t = line.split('/')
      t[-2] = version
      line = '/'.join(t)
      lines[i] = line
      if not ('crayonlang.org/docs/' + version + '/') in line:
        raise Exception("Something weird happened in " + path)
      break
  write_file(path, '\n'.join(lines))

def update_demo_build_file(version, path):
  lines = read_file(path).split('\n')
  for i in range(len(lines)):
    line = lines[i]
    tline = line.strip()
    if tline.startswith('"version":'):
      t = line.split('"')
      t[-2] = version
      line = '"'.join(t)
      lines[i] = line
      break
  write_file(path, '\n'.join(lines))

def update_docsIndexMd(version, path):
  lines = read_file(path).split('\n')
  line = lines[0]
  t = line.split(' ')
  t[-2] = version
  line = ' '.join(t)
  lines[0] = line
  write_file(path, '\n'.join(lines))

def update_releasePy(version, path):
  lines = read_file(path).split('\n')
  for i in range(len(lines)):
    line = lines[i]
    if line.startswith('VERSION = '):
      lines[i] = "VERSION = '" + version + "'"
      break
  write_file(path, '\n'.join(lines))

def main(args):
  version = parse_version(args)
  if version == None:
    print("Usage: python updateversion.py major minor build")
    print("e.g.: for \"2.1.0\", you'd run: python updateversion.py 2 1 0")
    return
  version_string = '.'.join(map(str, version))

  print("Running...")
  print("")

  all_files = get_all_code_files()

  assemblyInfo = filter(lambda x:x[1].endswith('/Properties/AssemblyInfo.cs'), all_files)
  for path, rel in assemblyInfo:
    print("Updating: " + rel)
    update_assemblyInfo_cs(version_string, path)

  versionInfo = next(filter(lambda x:x[1].endswith('/Common/VersionInfo.cs'), all_files))
  print("Updating: " + versionInfo[1])
  update_common_versionInfo_cs(version, versionInfo[0])

  programCs = next(filter(lambda x:x[1].endswith('/ResourcesVm/Program_cs.txt'), all_files))
  print("Updating: " + programCs[1])
  update_vmProgramCs(version_string, programCs[0])

  libManifests = filter(lambda x:x[1].endswith('/manifest.json') and x[1].startswith('Libraries/'), all_files)
  for path, rel in libManifests:
    print("Updating: " + rel)
    update_lib_manifest(version_string, path)

  demoBuildFiles = filter(lambda x:x[1].endswith('.build') and x[1].startswith('Demos/'), all_files)
  for path, rel in demoBuildFiles:
    print("Updating: " + rel)
    update_demo_build_file(version_string, path)

  docsIndexMd = next(filter(lambda x:x[1] == 'Docs/index.md', all_files))
  print("Updating: " + docsIndexMd[1])
  update_docsIndexMd(version_string, docsIndexMd[0])

  releaseScript = next(filter(lambda x:x[1] == 'Release/release.py', all_files))
  print("Updating: " + releaseScript[1])
  update_releasePy(version_string, releaseScript[0])

  print("")
  print("*" * 40)
  print("Note: that this script messes up the formatting because I'm lazy with how I wrote this.")
  print("Please re-run format-fixer.py before committing")
  print("*" * 40)
  print("")

  print("")
  print("v" * 40)
  print("You must still update the README.md manually!!!!")
  print("^" * 40)
  print("")

  print("Done!")

main(sys.argv[1:])
