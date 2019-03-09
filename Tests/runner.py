import os
import sys

BUILD_FILE = '''
{
	"id": "TestProject",
	"title": "Test Project",
	"description": "Enter description of TestProject",
	"source": "source",
	"compiler-locale": "en",
	"output": "output/%TARGET_NAME%",
	"target": [
		{
			"name": "csharp",
			"platform": "csharp-app"
		}
	]
}
'''

def run_command(cmd):
	c = os.popen(cmd)
	output = c.read()
	c.close()
	output = output.strip()
	return output

def read_file(path):
	c = open(path.replace('/', os.sep), 'rt')
	output = c.read().strip()
	c.close()
	return output

def write_file(path, text):
	parts = path.replace('\\', '/').split('/')
	file = parts[-1]
	dir = os.sep.join(parts[:-1])
	if not os.path.exists(dir):
		os.makedirs(dir)
	
	c = open(path, 'wt')
	c.write(text)
	c.close()

def main(args):
	run_unit_tests()
	run_compiler_negative_tests(args)

def run_unit_tests():
	print("Running unit tests...")
	unit_test_output = run_command('crayon UnitTests.build')
	print(unit_test_output)

def run_compiler_negative_tests(args):
	print("Running compiler negative tests...")
	
	write_file('TestProj/TestProj.build', BUILD_FILE)
	write_file('TestProj/source/main.cry', '''function main() { test(); }''')
	passed = 0
	failed = 0
	files = filter(lambda f: f.endswith('cry'), os.listdir('tests'))
	if len(args) == 1:
		files = filter(lambda f: args[0] in f, files)
	
	helper_functions = read_file('helper_functions.cry')
	
	for file in files:
		name = file[:-4]
		code = read_file('tests/' + name + '.cry')
		code += "\n\n" + helper_functions
		expected = read_file('tests/' + name + '.txt')
		write_file('TestProj/source/test.cry', code)
		result = run_command('crayon TestProj/TestProj.build')
		print(name + '...')
		if result == expected:
			print("PASS!")
			passed += 1
		else:
			print("FAIL!")
			print("Expected:")
			print(expected)
			print("Actual:")
			print(result)
			failed += 1
	
	print("\nDone!")
	print("Passed: " + str(passed))
	print("Failed: " + str(failed))


main(sys.argv[1:])
