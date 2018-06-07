VERSION = '0.2.1'
MSBUILD = r'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
XBUILD = 'xbuild'
RELEASE_CONFIG = '/p:Configuration=Release'
VM_TEMP_DIR = 'VmTemp'
VM_TEMP_DIR_SOURCE = VM_TEMP_DIR + '/Source'
import shutil
import os
import io
import sys

def canonicalize_sep(path):
	return path.replace('/', os.sep).replace('\\', os.sep)
def canonicalize_newline(text, lineEnding):
	return text.replace("\r\n", "\n").replace("\r", "\n").replace("\n", lineEnding)

def copyDirectory(source, target, ext_filter = None):
	source = canonicalize_sep(source)
	target = canonicalize_sep(target)
	if not os.path.exists(target):
		os.makedirs(target)
	for file in os.listdir(source):
		if ext_filter == None or file.endswith(ext_filter):
			fullpath = os.path.join(source, file)
			fulltargetpath = os.path.join(target, file)
			if os.path.isdir(fullpath):
				copyDirectory(fullpath, fulltargetpath)
			elif file.lower() in ('.ds_store', 'thumbs.db'):
				pass
			else:
				shutil.copyfile(fullpath, fulltargetpath)

def readFile(path):
	c = open(canonicalize_sep(path), 'rt')
	text = c.read()
	c.close()
	return text

def writeFile(path, content, lineEnding):
	content = canonicalize_newline(content, lineEnding)
	ucontent = unicode(content, 'utf-8')
	with io.open(canonicalize_sep(path), 'w', newline=lineEnding) as f:
		f.write(ucontent)

def runCommand(cmd):
	c = os.popen(cmd)
	output = c.read()
	c.close()
	return output

def main(args):
	librariesForRelease = [
		'Audio',
		'Core',
		'CrayonUnit',
		'Easing',
		'FileIO',
		'FileIOCommon',
		'Game',
		'GameGifCap',
		'Gamepad',
		'Graphics2D',
		'Graphics2DText',
		'Http',
		'ImageEncoder',
		'ImageResources',
		'ImageWebResources',
		'Json',
		'Math',
		'Matrices',
		'Random',
		'Resources',
		'SRandom',
		'UserData',
		'Web',
		'Xml',
	]

	if len(args) != 1:
		print("usage: python release.py windows|mono")
		return

	platform = args[0]

	if not platform in ('windows', 'mono'):
		print ("Invalid platform: " + platform)
		return

	copyToDir = 'crayon-' + VERSION + '-' + platform
	if os.path.exists(copyToDir):
		shutil.rmtree(copyToDir)
	os.makedirs(copyToDir)

	isMono = platform == 'mono'

	if isMono:
		BUILD_CMD = XBUILD
		SLN_PATH = '../Compiler/CrayonOSX.sln'
	else:
		BUILD_CMD = MSBUILD
		SLN_PATH = '..\\Compiler\\CrayonWindows.sln'
	print(runCommand(' '.join([BUILD_CMD, RELEASE_CONFIG, SLN_PATH])))

	releaseDir = '../Compiler/Crayon/bin/Release'
	shutil.copyfile(canonicalize_sep(releaseDir + '/Crayon.exe'), canonicalize_sep(copyToDir + '/crayon.exe'))
	shutil.copyfile(canonicalize_sep(releaseDir + '/LICENSE.txt'), canonicalize_sep(copyToDir + '/LICENSE.txt'))
	shutil.copyfile(canonicalize_sep('../README.md'), canonicalize_sep(copyToDir + '/README.md'))

	for lib in librariesForRelease:
		sourcePath = '../Libraries/' + lib
		targetPath = copyToDir + '/libs/' + lib
		copyDirectory(sourcePath, targetPath)

	for file in filter(lambda x:x.endswith('.dll'), os.listdir(releaseDir)):
		shutil.copyfile(releaseDir + '/' + file, copyToDir + '/' + file)
	
	os.environ["CRAYON_HOME"] = os.path.abspath(canonicalize_sep(copyToDir))
	
	print("Generating VM code...")
	runtimeCompilationCommand = canonicalize_sep(copyToDir + '/crayon.exe') + ' -vm csharp-app -vmdir ' + canonicalize_sep(VM_TEMP_DIR_SOURCE)
	if isMono:
		runtimeCompilationCommand = 'mono ' + runtimeCompilationCommand
	print('running:')
	print('  ' + runtimeCompilationCommand)
	print(runCommand(runtimeCompilationCommand))
	
	print("Compiling VM for distribution...")
	print(runCommand(' '.join([BUILD_CMD, RELEASE_CONFIG, canonicalize_sep(VM_TEMP_DIR_SOURCE + '/CrayonRuntime' + ('OSX' if isMono else '') + '.sln')])))
	
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/vm', '.dll')
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/vm', '.exe')
	
	if platform == 'windows':
		setupFile = readFile("setup-windows.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\r\n')
	if platform == 'mono':
		setupFile = readFile("setup-mono.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\n')

	print("Release directory created: " + copyToDir)

main(sys.argv[1:])
