import shutil
import os


if not os.path.exists('Release'):
	os.makedirs('Release')
if not os.path.exists(os.path.join('Release', 'lib')):
	os.makedirs(os.path.join('Release', 'lib'))

shutil.copyfile(os.path.join('Compiler', 'bin', 'Release', 'Crayon.exe'), os.path.join('Release', 'crayon.exe'))
shutil.copyfile(os.path.join('Compiler', 'bin', 'Release', 'LibraryConfig.dll'), os.path.join('Release', 'LibraryConfig.dll'))
shutil.copyfile(os.path.join('Compiler', 'bin', 'Release', 'LICENSE.txt'), os.path.join('Release', 'LICENSE.txt'))
shutil.copyfile('README.md', os.path.join('Release', 'README.md'))

for lib in os.listdir('Libraries'):
	if os.path.isdir(os.path.join('Libraries', lib)):
		if os.path.exists(os.path.join('Libraries', lib, 'bin', 'Release')):
			shutil.copyfile(os.path.join('Libraries', lib, 'bin', 'Release', lib + '.dll'), os.path.join('Release', 'lib', lib + '.dll'))
