import shutil
import os

def copyDirectory(source, target):
	os.makedirs(target)
	for file in os.listdir(source):
		fullpath = os.path.join(source, file)
		fulltargetpath = os.path.join(target, file)
		if os.path.isdir(fullpath):
			copyDirectory(fullpath, fulltargetpath)
		elif file.endswith('.txt') or file.endswith('.cry'):
			# The intent of this is to avoid os generated files like thumbs.db
			# Tweak if new file types are added.
			shutil.copyfile(fullpath, fulltargetpath)

librariesForRelease = [
	'Audio',
	'Core',
	'Easing',
	'FileIO',
	'Game',
	'Gamepad',
	'Graphics',
	'GraphicsText',
	'Http',
	'ImageResources',
	'JSON',
	'Math',
	'Random',
	'Resources',
	'Web',
	'XML',
]

if not os.path.exists('Release'):
	os.makedirs('Release')

shutil.copyfile(os.path.join('Compiler', 'bin', 'Release', 'Crayon.exe'), os.path.join('Release', 'crayon.exe'))
shutil.copyfile(os.path.join('Compiler', 'bin', 'Release', 'LICENSE.txt'), os.path.join('Release', 'LICENSE.txt'))
shutil.copyfile('README.md', os.path.join('Release', 'README.md'))

for lib in librariesForRelease:
	sourcePath = os.path.join('Libraries', lib)
	targetPath = os.path.join('Release', 'libs', lib)
	copyDirectory(sourcePath, targetPath)
