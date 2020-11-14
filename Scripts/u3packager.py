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

  push_cd(os.path.join('.', 'src'))

  if os.path.exists('dist'):
    shutil.rmtree('dist')

  run_command('electron-builder -p never --win --x64')

  source = os.path.abspath(os.path.join('dist', 'win-unpacked', 'u3window.exe'))
  target = os.path.abspath(os.path.join('..', 'dist', 'win-u3window.exe'))
  os.rename(source, target)

  print("U3/dist/win-u3window.exe created")

  pop_cd()
  pop_cd()



main(sys.argv[1:])
