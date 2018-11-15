import os

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

def main():
	write_file('TestProj/TestProj.build', BUILD_FILE)
	write_file('TestProj/source/main.cry', '''function main() { test(); }''')
	passed = 0
	failed = 0
	for file in os.listdir('tests'):
		if file.endswith('.cry'):
			name = file[:-4]
			code = read_file('tests/' + name + '.cry')
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


main()
