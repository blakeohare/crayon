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
import time

sys.path.append(os.path.join('..', 'Scripts'))
import crypkgmake

def canonicalize_sep(path):
	return path.replace('/', os.sep).replace('\\', os.sep)
def canonicalize_newline(text, lineEnding):
	return text.replace("\r\n", "\n").replace("\r", "\n").replace("\n", lineEnding)

def copyDirectory(source, target, ext_filter = None, recursive = True):
	source = canonicalize_sep(source)
	target = canonicalize_sep(target)
	if not os.path.exists(target):
		os.makedirs(target)
	for file in os.listdir(source):
		fullpath = os.path.join(source, file)
		fulltargetpath = os.path.join(target, file)
		if os.path.isdir(fullpath):
			if recursive:
				copyDirectory(fullpath, fulltargetpath, ext_filter)
		elif file.lower() in ('.ds_store', 'thumbs.db'):
			pass
		elif ext_filter == None or file.endswith(ext_filter):
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

def ensure_directory_exists(path):
	path = canonicalize_sep(path)
	if os.path.exists(path): return
	os.makedirs(path)

def path_exists(path):
	path = canonicalize_sep(path)
	return os.path.exists(path)
	
def get_parent_path(path):
	path = canonicalize_sep(path)
	i = path.rfind(os.sep)
	return path[:i]

def list_directories(path):
	output = []
	for file in os.listdir(canonicalize_sep(path)):
		if os.path.isdir(path + os.sep + file):
			output.append(file)
	return output

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
		'CryptoMd5',
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
		'Nori',
		'NoriXml',
		'Random',
		'Resources',
		'SRandom',
		'TextEncoding',
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


	# Clean up pre-existing release and ensure output directory exists

	copyToDir = 'crayon-' + VERSION + '-' + platform
	if os.path.exists(copyToDir):
		shutil.rmtree(copyToDir)
		time.sleep(0.1)
	os.makedirs(copyToDir)


	# Compile the compiler bits in the source tree to their usual bin directory

	isMono = platform == 'mono'

	if isMono:
		BUILD_CMD = XBUILD
		SLN_PATH = '../Compiler/CrayonOSX.sln'
	else:
		BUILD_CMD = MSBUILD
		SLN_PATH = '..\\Compiler\\CrayonWindows.sln'
	cmd = ' '.join([BUILD_CMD, RELEASE_CONFIG, SLN_PATH])
	
	print("Running: " + cmd)
	print(runCommand(cmd))


	# Copy the compiler's release bits into the newly created release directory
	releaseDir = '../Compiler/Crayon/bin/Release'
	shutil.copyfile(canonicalize_sep(releaseDir + '/Crayon.exe'), canonicalize_sep(copyToDir + '/crayon.exe'))
	shutil.copyfile(canonicalize_sep(releaseDir + '/LICENSE.txt'), canonicalize_sep(copyToDir + '/LICENSE.txt'))
	shutil.copyfile(canonicalize_sep('../README.md'), canonicalize_sep(copyToDir + '/README.md'))


	# Go through the libraries (just the ones listed at the top of this file)
	# Copy the root directory and src directories to the output directory
	# Generate .crypkg files for the native files.

	# TODO: also crypkg the src files, possibly
	for lib in librariesForRelease:
		sourcePathRoot = '../Libraries/' + lib
		targetPathRoot = copyToDir + '/libs/' + lib
		copyDirectory(sourcePathRoot, targetPathRoot, recursive = False)
		copyDirectory(sourcePathRoot + '/src', targetPathRoot + '/src')
		sourceNativeDir = sourcePathRoot + '/native'
		if path_exists(sourceNativeDir):
			print("Generating crypkg'es for " + lib + "...")
			for platform in list_directories(sourceNativeDir):
				print('...' + platform)
				target_crypkg_path = targetPathRoot + '/native/' + platform + '.crypkg'
				ensure_directory_exists(get_parent_path(target_crypkg_path))
				crypkgmake.make_pkg(canonicalize_sep(sourceNativeDir + '/' + platform), canonicalize_sep(target_crypkg_path))


	# Go through the source's bin/Release directory and find all the library DLL's.
	# Copy those to the output release directory.

	for file in filter(lambda x:x.endswith('.dll'), os.listdir(releaseDir)):
		shutil.copyfile(releaseDir + '/' + file, copyToDir + '/' + file)


	# Artifically set the CRAYON_HOME environment variable to the target directory with all the libraries
	os.environ["CRAYON_HOME"] = os.path.abspath(canonicalize_sep(copyToDir))
	
	
	# Use the newly built compiler to generate the VM source code (in VmTemp)
	print("Generating VM code...")
	runtimeCompilationCommand = canonicalize_sep(copyToDir + '/crayon.exe') + ' -vm csharp-app -vmdir ' + canonicalize_sep(VM_TEMP_DIR_SOURCE)
	if isMono:
		runtimeCompilationCommand = 'mono ' + runtimeCompilationCommand
	print('running:')
	print('  ' + runtimeCompilationCommand)
	print(runCommand(runtimeCompilationCommand))


	# Now compile the generated VM source code	
	print("Compiling VM for distribution...")
	print(runCommand(' '.join([BUILD_CMD, RELEASE_CONFIG, canonicalize_sep(VM_TEMP_DIR_SOURCE + '/CrayonRuntime' + ('OSX' if isMono else '') + '.sln')])))


	# Copy the built bits from VmTemp to the vm/ directory
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/vm', '.dll')
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/vm', '.exe')


	# Throw in setup instructions according to the platform you're generating
	if platform == 'windows':
		setupFile = readFile("setup-windows.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\r\n')
	if platform == 'mono':
		setupFile = readFile("setup-mono.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\n')


	# Copy the Interpreter source to vmsrc
	# TODO: no longer needed! Need to copy the generated bits instead as a crypkg
	copyDirectory('../Interpreter/source', copyToDir + '/vmsrc', '.pst')


	# Hooray, you're done!
	print("Release directory created: " + copyToDir)

main(sys.argv[1:])
