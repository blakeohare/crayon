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
  push_cd(os.path.join('..', 'U3'))

  if os.path.exists('dist'):
    shutil.rmtree('dist')
  os.mkdir('dist')

  run_command('electron-packager src u3window --platform=win32 --arch=x64 --out=dist/temp --lang=en-US')

  source = os.path.join('dist', 'temp', 'u3window-win32-x64')
  target = os.path.join('dist', 'win')
  shutil.move(source, target)

  print("U3/dist/win/u3window.exe created")

  shutil.rmtree(os.path.join('dist', 'temp'))

  pop_cd()



main(sys.argv[1:])
