VERSION = '0.2.0'

import shutil
import os
import io

def copyDirectory(source, target):
	source = source.replace('/', os.sep)
	target = target.replace('/', os.sep)
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

def readFile(path):
	c = open(path.replace('/', os.sep), 'rt')
	text = c.read()
	c.close()
	return text

def writeFile(path, content, lineEnding):
	content = content.replace("\r\n", "\n").replace("\r", "\n").replace("\n", lineEnding)
	ucontent = unicode(content, 'utf-8')
	with io.open(path.replace('/', os.sep), 'w', newline=lineEnding) as f:
		f.write(ucontent)
	

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
	'ImageWebResources',
	'JSON',
	'Math',
	'Random',
	'Resources',
	'Web',
	'XML',
]

for platform in ('windows', 'mono'):
	copyToDir = 'crayon-' + VERSION + '-' + platform
	if os.path.exists(copyToDir):
		shutil.rmtree(copyToDir)
	os.makedirs(copyToDir)
	shutil.copyfile('../Compiler/bin/Release/Crayon.exe', copyToDir + '/crayon.exe')
	shutil.copyfile('../Compiler/bin/Release/LICENSE.txt', copyToDir + '/LICENSE.txt')
	shutil.copyfile('../README.md', copyToDir + '/README.md')
	if platform == 'windows':
		setupFile = readFile("setup-windows.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\r\n')
	if platform == 'mono':
		setupFile = readFile("setup-mono.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\n')

	for lib in librariesForRelease:
		sourcePath = '../Libraries/' + lib
		targetPath = copyToDir + '/libs/' + lib
		copyDirectory(sourcePath, targetPath)
