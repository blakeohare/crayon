VERSION = '2.9.0'

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

def copy_directory(source, target, ext_filter = None, recursive = True):
	source = canonicalize_sep(source)
	target = canonicalize_sep(target)
	if not os.path.exists(target):
		os.makedirs(target)
	for file in os.listdir(source):
		full_path = os.path.join(source, file)
		full_target_path = os.path.join(target, file)
		if os.path.isdir(full_path):
			if recursive:
				copy_directory(full_path, full_target_path, ext_filter)
		elif file.lower() in ('.ds_store', 'thumbs.db'):
			pass
		elif ext_filter == None or file.endswith(ext_filter):
			shutil.copyfile(full_path, full_target_path)

def read_file(path):
	c = open(canonicalize_sep(path), 'rt')
	text = c.read()
	c.close()
	return text

def write_file(path, content, lineEnding):
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

def run_command(cmd):
	c = os.popen(cmd)
	output = c.read()
	c.close()
	return output

_logRef = [None]

def copy_extensions(crayon_exe, copy_to_dir):
	ext_src_dir = '../Extensions'
	for ext_name in list_directories(ext_src_dir):
		build_file = ext_src_dir + '/' + ext_name + '/' + ext_name + '.build'
		if path_exists(build_file):
			print("Exporting extension: " + ext_name)
			print("TODO: standalone cbx export support via command line args")
			#run_command(crayon_exe + ' ' + ext_src_dir)
	
def main(args):
	_logRef[0] = []
	try:
		build_release(args)
	except Exception as err:
		_logRef[0].append('EXCEPTION: ' + str(err))
		
	print("Finished performing the following:\n * " + "\n * ".join(_logRef[0]))

def log(value):
	_logRef[0].append(str(value))

def build_release(args):
	log('begin')
	
	platforms_for_interpreter_gen = [
		'csharp-app',
		'javascript-app',
	]

	if len(args) != 1:
		log("incorrect usage")
		print("usage: python release.py windows|mac|linux")
		return

	os_platform = args[0]

	is_windows = False
	is_mac = False
	is_linux = False

	if os_platform == 'windows':
		is_windows = True
	elif os_platform == 'mac':
		is_mac = True
	elif os_platform == 'linux':
		is_linux = True
		log("not supported yet")
		print("Linux support isn't here yet. But soon!")
		return
	else:
		log("incorrect platform")
		print ("Invalid platform: " + os_platform)
		return

	# Clean up pre-existing release and ensure output directory exists

	log("cleanup old directory")
	copy_to_dir = 'crayon-' + VERSION + '-' + os_platform
	if os.path.exists(copy_to_dir):
		shutil.rmtree(copy_to_dir)
		time.sleep(0.1)
	log("create new output directory")
	os.makedirs(copy_to_dir)

	if is_windows:
		u3_dir = os.path.join('..', 'U3', 'dist', 'win')
	elif is_mac:
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
		'-r', 'osx-x64' if is_mac else 'win-x64',
		'--self-contained true',
		'-p:PublishTrimmed=true',
		'-p:PublishSingleFile=true'
	])
	
	log("Compiling the .sln file with command: " + cmd)
	print("Running: " + cmd)
	print(run_command(cmd))


	# Copy the compiler's release bits into the newly created release directory
	release_dir = '../Compiler/Crayon/bin/Release/netcoreapp3.1/' + ('osx-x64' if is_mac else 'win-x64') + '/publish'
	
	log("Copying crayon.exe, readme, and license to output directory")
	if is_windows:
		crayon_exe_name = 'Crayon.exe'
	elif is_mac:
		crayon_exe_name = 'Crayon'
	else:
		raise Exception("Invalid platform")

	new_crayon_executable = canonicalize_sep(copy_to_dir + '/' + crayon_exe_name.lower())
	shutil.copyfile(canonicalize_sep(release_dir + '/' + crayon_exe_name), new_crayon_executable)
	
	shutil.copyfile(canonicalize_sep('../Compiler/Crayon/LICENSE.txt'), canonicalize_sep(copy_to_dir + '/LICENSE.txt'))
	shutil.copyfile(canonicalize_sep('../README.md'), canonicalize_sep(copy_to_dir + '/README.md'))


	# Go through the libraries (just the ones listed at the top of this file)
	# Copy the root directory and src directories to the output directory
	# Generate .crypkg files for the native files.

	# TODO: also crypkg the src files, possibly
	for lib in LIBRARIES:
		source_path_root = '../Libraries/' + lib
		target_path_root = copy_to_dir + '/libs/' + lib
		copy_directory(source_path_root, target_path_root, recursive = False)
		copy_directory(source_path_root + '/src', target_path_root + '/src')

	# Go through the source's bin/Release directory and find all the library DLL's.
	# Copy those to the output release directory.

	files = os.listdir(release_dir)
	#for file in filter(lambda x:x.endswith('.dll'), files):
	for file in filter(lambda f:not f.endswith('.pdb'), files):
		log("Copy " + file + " to the output directory")
		shutil.copyfile(release_dir + '/' + file, copy_to_dir + '/' + file)


	print("\nCopying Interpreter/gen to crypkg files in vmsrc...")
	for platform in platforms_for_interpreter_gen:
		log("Copying " + platform + "'s files from Interpreter/gen to vmsrc/" + platform + ".crypkg")
		print('  ' + platform + '...')
		target_pkg_path = copy_to_dir + '/vmsrc/' + platform + '.crypkg'
		interpreter_generated_code = '../Interpreter/gen/' + platform
		ensure_directory_exists(get_parent_path(target_pkg_path))
		crypkgmake.make_pkg(canonicalize_sep(interpreter_generated_code), canonicalize_sep(target_pkg_path))
		print("Done!\n")


	# Artifically set the CRAYON_HOME environment variable to the target directory with all the libraries
	os.environ["CRAYON_HOME"] = os.path.abspath(canonicalize_sep(copy_to_dir))
	log("Set the CRAYON_HOME to " + os.environ['CRAYON_HOME'])
	
	
	# Copy U3 window
	copy_directory(u3_dir, copy_to_dir + '/u3')

	if is_mac:
		u3win_app = copy_to_dir + '/u3/u3window.app'
		for file in list_all_files_recursive(u3win_app):
			run_command('chmod +x ' + u3win_app + '/' + file.replace(' ', '\\ ').replace('(', '\\(').replace(')', '\\)'))

	# Throw in setup instructions according to the platform you're generating
	log("Throwing in the setup-" + os_platform + ".txt file and syntax highlighter definition file.")
	if is_windows:
		setup_file = read_file("setup-windows.txt")
		write_file(copy_to_dir + '/Setup Instructions.txt', setup_file, '\r\n')
		syntax_highlighter = read_file("notepadplusplus_crayon_syntax.xml")
		write_file(copy_to_dir + '/notepadplusplus_crayon_syntax.xml', syntax_highlighter, '\n')
	elif is_mac:
		setup_file = read_file("setup-mono.txt")
		write_file(copy_to_dir + '/Setup Instructions.txt', setup_file, '\n')
	else:
		raise Exception("Invalid platform")


	# Create a dummy project with ALL libraries
	log("Ensuring libraries compile")
	print("Ensure libraries compile\n")
	old_cwd = os.getcwd()
	temp_dir = os.path.join('.', 'temp')
	ensure_directory_exists(temp_dir)
	os.chdir(temp_dir)
	
	run_command(os.path.join('..', new_crayon_executable) + ' -genDefaultProj LibTest')
	os.chdir(old_cwd)

	code = []
	for lib in LIBRARIES:
		if lib in ('FileIOCommon', ): continue
		code.append('import ' + lib + ';\n')
	code.append('\nfunction main() { }\n')
	write_file(temp_dir + '/LibTest/source/main.cry', ''.join(code), '\n')
	result = run_command(new_crayon_executable + ' ' + canonicalize_sep(temp_dir + '/LibTest/LibTest.build') + ' -target csharp')
	if result.strip() != '':
		err = "*** ERROR: Libraries did not compile cleanly!!! ***"
		print('*' * len(err))
		print(err)
		print('*' * len(err))
		print("The build output was this:")
		print(result)
		return

	copy_extensions(new_crayon_executable, copy_to_dir)

	# Hooray, you're done!
	log("Completed")
	print("Release directory created: " + copy_to_dir)

main(sys.argv[1:])
