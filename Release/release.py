VERSION = '2.9.0'
MSBUILD = r'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
XBUILD = 'xbuild'
RELEASE_CONFIG = '/p:Configuration=Release'
VM_TEMP_DIR = 'VmTemp'
VM_TEMP_DIR_SOURCE = VM_TEMP_DIR + '/Source'

LIBRARIES = [
	'Audio',
	'Base64',
	'Core',
	'CrayonUnit',
	'CryptoCommon',
	'CryptoMd5',
	'CryptoSha1',
	'DateTime',
	'Dispatcher',
	'Easing',
	'Environment',
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
	'NativeTunnel',
	'Nori',
	'NoriXml',
	'ProcessUtil',
	'Random',
	'Resources',
	'SRandom',
	'TextEncoding',
	'UserData',
	'Web',
	'Xml',
]

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
	bytes = bytearray(content, 'utf-8')
	path = canonicalize_sep(path)
	c = open(path, 'wb')
	c.write(bytes)
	c.close()

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

_logRef = [None]
	
def main(args):
	_logRef[0] = []
	try:
		buildRelease(args)
	except Exception as err:
		_logRef[0].append('EXCEPTION: ' + str(err))
		
	print("Finished performing the following:\n * " + "\n * ".join(_logRef[0]))

def log(value):
	_logRef[0].append(str(value))

def buildRelease(args):
	log('begin')
	
	platformsForInterpreterGen = [
		'csharp-app',
		'javascript-app',
	]

	if len(args) != 1:
		log("incorrect usage")
		print("usage: python release.py windows|mono")
		return

	os_platform = args[0]

	isMono = os_platform == 'mono'
	isWindows = os_platform == 'windows'

	if not isWindows and not isMono:
		log("incorrect platform")
		print ("Invalid platform: " + os_platform)
		return

	# Clean up pre-existing release and ensure output directory exists

	log("cleanup old directory")
	copyToDir = 'crayon-' + VERSION + '-' + os_platform
	if os.path.exists(copyToDir):
		shutil.rmtree(copyToDir)
		time.sleep(0.1)
	log("create new output directory")
	os.makedirs(copyToDir)

	# Compile the compiler bits in the source tree to their usual bin directory

	if isMono:
		BUILD_CMD = XBUILD
	#	SLN_PATH = '../Compiler/Crayon.sln'
	else:
		BUILD_CMD = MSBUILD
	#	SLN_PATH = '..\\Compiler\\Crayon.sln'
	#cmd = ' '.join([BUILD_CMD, RELEASE_CONFIG, SLN_PATH])
	
	cmd = ' '.join([
		'dotnet publish',
		os.path.join('..', 'Compiler', 'Crayon', 'Crayon.csproj'),
		'-c Release',
		'-r', 'osx-x64' if isMono else 'win-x64',
		'--self-contained true',
		'-p:PublishTrimmed=true',
		'-p:PublishSingleFile=true'
	])
	
	log("Compiling the .sln file with command: " + cmd)
	print("Running: " + cmd)
	print(runCommand(cmd))


	# Copy the compiler's release bits into the newly created release directory
	releaseDir = '../Compiler/Crayon/bin/Release/netcoreapp3.1/' + ('osx-x64' if isMono else 'win-x64') + '/publish'
	
	log("Copying crayon.exe, readme, and license to output directory")
	if isWindows:
		crayon_exe_name = 'Crayon.exe'
	else:
		crayon_exe_name = 'Crayon'
	shutil.copyfile(canonicalize_sep(releaseDir + '/' + crayon_exe_name), canonicalize_sep(copyToDir + '/' + crayon_exe_name.lower()))
	
	shutil.copyfile(canonicalize_sep('../Compiler/Crayon/LICENSE.txt'), canonicalize_sep(copyToDir + '/LICENSE.txt'))
	shutil.copyfile(canonicalize_sep('../README.md'), canonicalize_sep(copyToDir + '/README.md'))


	# Go through the libraries (just the ones listed at the top of this file)
	# Copy the root directory and src directories to the output directory
	# Generate .crypkg files for the native files.

	# TODO: also crypkg the src files, possibly
	for lib in LIBRARIES:
		sourcePathRoot = '../Libraries/' + lib
		targetPathRoot = copyToDir + '/libs/' + lib
		copyDirectory(sourcePathRoot, targetPathRoot, recursive = False)
		copyDirectory(sourcePathRoot + '/src', targetPathRoot + '/src')
		sourceNativeDir = sourcePathRoot + '/native'
		if path_exists(sourceNativeDir):
			print("Generating crypkg'es for " + lib + "...")
			log("Generate crypkg'es for " + lib)
			for platform in list_directories(sourceNativeDir):
				print('...' + platform)
				target_crypkg_path = targetPathRoot + '/native/' + platform + '.crypkg'
				ensure_directory_exists(get_parent_path(target_crypkg_path))
				crypkgmake.make_pkg(canonicalize_sep(sourceNativeDir + '/' + platform), canonicalize_sep(target_crypkg_path))


	# Go through the source's bin/Release directory and find all the library DLL's.
	# Copy those to the output release directory.

	files = os.listdir(releaseDir)
	#for file in filter(lambda x:x.endswith('.dll'), files):
	for file in filter(lambda f:not f.endswith('.pdb'), files):
		log("Copy " + file + " to the output directory")
		shutil.copyfile(releaseDir + '/' + file, copyToDir + '/' + file)


	print("\nCopying Interpreter/gen to crypkg files in vmsrc...")
	for platform in platformsForInterpreterGen:
		log("Copying " + platform + "'s files from Interpreter/gen to vmsrc/" + platform + ".crypkg")
		print('  ' + platform + '...')
		target_pkg_path = copyToDir + '/vmsrc/' + platform + '.crypkg'
		interpreter_generated_code = '../Interpreter/gen/' + platform
		ensure_directory_exists(get_parent_path(target_pkg_path))
		crypkgmake.make_pkg(canonicalize_sep(interpreter_generated_code), canonicalize_sep(target_pkg_path))
		print("Done!\n")
	

	# Artifically set the CRAYON_HOME environment variable to the target directory with all the libraries
	os.environ["CRAYON_HOME"] = os.path.abspath(canonicalize_sep(copyToDir))
	log("Set the CRAYON_HOME to " + os.environ['CRAYON_HOME'])
	
	# Use the newly built compiler to generate the VM source code (in VmTemp)
	print("Generating VM code...")
	runtimeCompilationCommand = canonicalize_sep(copyToDir + '/crayon.exe') + ' -vm csharp-app -vmdir ' + canonicalize_sep(VM_TEMP_DIR_SOURCE)
	if isMono:
		runtimeCompilationCommand = 'mono ' + runtimeCompilationCommand
	log("Generating the VM C# project in VmTemp/ with the command: " + runtimeCompilationCommand)
	print('running:')
	print('  ' + runtimeCompilationCommand)
	print(runCommand(runtimeCompilationCommand))


	# Now compile the generated VM source code	
	print("Compiling VM for distribution...")
	cmd = ' '.join([BUILD_CMD, RELEASE_CONFIG, canonicalize_sep(VM_TEMP_DIR_SOURCE + '/CrayonRuntime' + ('OSX' if isMono else '') + '.sln')])
	log("Compiling the VM in VmTemp using the command: " + cmd)
	print(runCommand(cmd))


	# Copy the built bits from VmTemp to the vm/ directory
	log("Copying all the VmTemp/Libs/Release dll's and exe's to the vm/ directory")
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/vm', '.dll')
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/vm', '.exe')


	# Throw in setup instructions according to the platform you're generating
	log("Throwing in the setup-" + os_platform + ".txt file and syntax highlighter definition file.")
	if isWindows:
		setupFile = readFile("setup-windows.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\r\n')
		syntaxHighlighter = readFile("notepadplusplus_crayon_syntax.xml")
		writeFile(copyToDir + '/notepadplusplus_crayon_syntax.xml', syntaxHighlighter, '\n')

	if isMono:
		setupFile = readFile("setup-mono.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\n')


	# Copy the Interpreter source to vmsrc
	# TODO: no longer needed! Need to copy the generated bits instead as a crypkg
	#copyDirectory('../Interpreter/source', copyToDir + '/vmsrc', '.pst')


	# Hooray, you're done!
	log("Completed")
	print("Release directory created: " + copyToDir)

main(sys.argv[1:])
