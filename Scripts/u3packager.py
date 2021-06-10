import os
import sys
import shutil

def run_command(cmd):
  c = os.popen(cmd)
  t = c.read()
  c.close()
  return t


_current_dir = [os.getcwd()]
def push_cd(dir):
  _current_dir.append(os.path.abspath(dir))
  os.chdir(dir)

def pop_cd():
  _current_dir.pop()
  os.chdir(_current_dir[-1])

def main(args):

  platform = 'windows'
  if len(args) == 1:
    platform = args[0]

  isWindows = platform == 'windows'
  isMac = platform == 'mac'
  isLinux = platform == 'linux'

  if isLinux:
    print("Linux not supported yet")
    return

  if not isWindows and not isMac:
    print("Unknown platform: '" + platform + "'")
    return

  if isWindows:
    props = {
      'arch': 'win32',
      'electron-out': 'u3window-win32-x64',
      'u3-out': 'win',
      'bin-name': 'u3window.exe',
    }
  elif isMac:
    props = {
      'arch': 'darwin',
      'electron-out': 'u3window-darwin-x64',
      'u3-out': 'mac',
      'bin-name': 'u3window',
    }

  push_cd(os.path.join('..', 'U3'))

  if os.path.exists('dist'):
    shutil.rmtree('dist')
  os.mkdir('dist')

  run_command('electron-packager src u3window --platform=' + props['arch'] + ' --arch=x64 --out=dist/temp --lang=en-US')

  source = os.path.join('dist', 'temp', props['electron-out'])
  target = os.path.join('dist', props['u3-out'])
  shutil.move(source, target)

  print('U3/dist/' + props['u3-out'] + '/' + props['bin-name'] + ' created')

  shutil.rmtree(os.path.join('dist', 'temp'))

  pop_cd()



main(sys.argv[1:])
