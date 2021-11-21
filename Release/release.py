VERSION = '2.9.0'
VM_TEMP_DIR = 'temp'
VM_TEMP_DIR_SOURCE = VM_TEMP_DIR + '/Source'

LIBRARIES = [
	'Audio',
	'Base64',
	'BrowserInterop',
	'Cookies',
	'Core',
	'CrayonUnit',
	'CryptoCommon',
	'CryptoMd5',
	'CryptoSha1',
	'Csv',
	'DateTime',
	'Easing',
	'Environment',
	'FileIO',
	'FileIOCommon',
	'Game',
	'Gamepad',
	'Graphics2D',
	'Graphics2DText',
	'Http',
	'IconEncoder',
	'Images',
	'ImageWebResources',
	'Ipc',
	'Json',
	'Math',
	'Matrices',
	'MessageHub',
	'NativeTunnel',
	'Nori',
	'NoriRichText',
	'NoriXml',
	'OpenGL',
	'ProcessUtil',
	'Random',
	'Resources',
	'SRandom',
	'TextEncoding',
	'U3Direct',
	'UrlUtil',
	'UserData',
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

def copy_file(from_path, to_path):
	f = canonicalize_sep(from_path)
	t = canonicalize_sep(to_path)
	if os.path.exists(to_path):
		if os.path.isdir(to_path):
			shutil.rmtree(to_path)
		else:
			os.remove(to_path)
	shutil.copyfile(from_path, to_path)

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

def list_all_files_recursive(path):
	output = []
	list_all_files_recursive_impl(path, '', output)
	return output

def list_all_files_recursive_impl(abs_path, rel_path, output):
	for file in os.listdir(abs_path):
		if file.lower() == '.ds_store': continue

		abs_file = os.path.join(abs_path, file)
		rel_file = file if rel_path == '' else os.path.join(rel_path, file)
		if os.path.isdir(abs_file):
			list_all_files_recursive_impl(abs_file, rel_file, output)
		else:
			output.append(rel_file)

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

	isWindows = os_platform == 'windows'
	isMac = os_platform == 'mac'
	isLinux = os_platform == 'linux'

	if isLinux:
		log("not supported yet")
		print("Linux support isn't here yet. But soon!")
		return

	if not isWindows and not isMac:
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

	if isWindows:
		u3_dir = os.path.join('..', 'U3', 'dist', 'win')
	elif isMac:
		u3_dir = os.path.join('..', 'U3', 'dist', 'mac')
	else:
		raise Exception("Not implemented")
	
	if not os.path.exists(u3_dir):
		print("*** ERROR! ***")
		print("U3 executable is missing. Run Scripts/u3packager.py first.")
		return
	
	# Compile the compiler bits in the source tree to their usual bin directory

	cmd = ' '.join([
		'dotnet publish',
		os.path.join('..', 'Compiler', 'Crayon', 'Crayon.csproj'),
		'-c Release',
		'-r', 'osx-x64' if isMac else 'win-x64',
		'--self-contained true',
		'-p:PublishTrimmed=true',
		'-p:PublishSingleFile=true'
	])
	
	log("Compiling the .sln file with command: " + cmd)
	print("Running: " + cmd)
	print(runCommand(cmd))


	# Copy the compiler's release bits into the newly created release directory
	releaseDir = '../Compiler/Crayon/bin/Release/netcoreapp3.1/' + ('osx-x64' if isMac else 'win-x64') + '/publish'
	
	log("Copying crayon.exe, readme, and license to output directory")
	if isWindows:
		crayon_exe_name = 'Crayon.exe'
	elif isMac:
		crayon_exe_name = 'Crayon'
	else:
		raise Exception("Invalid platform")

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
	
	# Use the newly built compiler to generate the VM source code (in temp)
	print("Generating VM code...")
	new_crayon_executable = canonicalize_sep(copyToDir + '/crayon' + ('' if isMac else '.exe'))
	if isMac:
		runCommand('chmod +x ' + new_crayon_executable)
	runtimeCompilationCommand = canonicalize_sep(new_crayon_executable) + ' -vm csharp-app -vmdir ' + canonicalize_sep(VM_TEMP_DIR_SOURCE)

	log("Generating the VM C# project in temp/ with the command: " + runtimeCompilationCommand)
	print('running:')
	print('  ' + runtimeCompilationCommand)
	print(runCommand(runtimeCompilationCommand))

	# Now compile the generated VM source code	
	print("Compiling VM for distribution...")
	dot_net_platform_name = 'osx-x64' if isMac else 'win-x64'
	cmd = ' '.join([
		'dotnet publish',
		os.path.join(VM_TEMP_DIR_SOURCE, 'CrayonRuntime', 'Interpreter.csproj'),
		'-c Release',
		'-r', dot_net_platform_name,
		'--self-contained true',
		'-p:PublishTrimmed=true',
		'-p:PublishSingleFile=true'
	])
	log("Compiling the VM in temp using the command: " + cmd)
	print(runCommand(cmd))
	
	# Copy the Crayon runtime
	runtime_from_dir = VM_TEMP_DIR + '/Source/CrayonRuntime/bin/Release/netcoreapp3.1/' + dot_net_platform_name + '/publish'
	runtime_to_dir = copyToDir + '/vm'
	ensure_directory_exists(runtime_to_dir)
	if isWindows:
		copy_file(runtime_from_dir + '/Interpreter.exe', runtime_to_dir + '/CrayonRuntime.exe')
	elif isMac:
		to_path = runtime_to_dir + '/CrayonRuntime'
		copy_file(runtime_from_dir + '/Interpreter', to_path)
		runCommand('chmod +x ' + to_path)
	else:
		raise Exception()

	# Copy U3 window
	copyDirectory(u3_dir, copyToDir + '/u3')

	if isMac:
		u3win_app = copyToDir + '/u3/u3window.app'
		for file in list_all_files_recursive(u3win_app):
			runCommand('chmod +x ' + u3win_app + '/' + file.replace(' ', '\\ ').replace('(', '\\(').replace(')', '\\)'))

	# Throw in setup instructions according to the platform you're generating
	log("Throwing in the setup-" + os_platform + ".txt file and syntax highlighter definition file.")
	if isWindows:
		setupFile = readFile("setup-windows.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\r\n')
		syntaxHighlighter = readFile("notepadplusplus_crayon_syntax.xml")
		writeFile(copyToDir + '/notepadplusplus_crayon_syntax.xml', syntaxHighlighter, '\n')
	elif isMac:
		setupFile = readFile("setup-mono.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\n')
	else:
		raise Exception("Invalid platform")


	# Copy the Interpreter source to vmsrc
	# TODO: no longer needed! Need to copy the generated bits instead as a crypkg
	#copyDirectory('../Interpreter/source', copyToDir + '/vmsrc', '.pst')


	# Create a dummy project with ALL libraries
	log("Ensuring libraries compile")
	print("Ensure libraries compile\n")
	old_cwd = os.getcwd()
	os.chdir(os.path.join('.', VM_TEMP_DIR))
	runCommand(os.path.join('..', new_crayon_executable) + ' -genDefaultProj LibTest')
	os.chdir(old_cwd)

	code = []
	for lib in LIBRARIES:
		if lib in ('FileIOCommon', ): continue
		code.append('import ' + lib + ';\n')
	code.append('\nfunction main() { }\n')
	writeFile(VM_TEMP_DIR + '/LibTest/source/main.cry', ''.join(code), '\n')
	result = runCommand(new_crayon_executable + ' ' + canonicalize_sep(VM_TEMP_DIR + '/LibTest/LibTest.build') + ' -target csharp')
	if result.strip() != '':
		err = "*** ERROR: Libraries did not compile cleanly!!! ***"
		print('*' * len(err))
		print(err)
		print('*' * len(err))
		print("The build output was this:")
		print(result)
		return

	# Hooray, you're done!
	log("Completed")
	print("Release directory created: " + copyToDir)

main(sys.argv[1:])
