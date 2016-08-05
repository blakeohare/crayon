# A script that can be run before committing to normalize all whitespace
# Editors are all different. This will reduce a lot of diff noise or confusion,
# particularly because most editors render tabs as 4 spaces by default and github
# renders it as 8.

import os

def loadFile(path):
	c = open(path, 'rt')
	text = c.read()
	c.close()
	return text

def writeFile(path, content):
	c = open(path, 'wt')
	c.write(content)
	c.close()



def rtrim(string):
	while len(string) > 0 and string[-1] in ' \r\n\t':
		string = string[:-1]
	return string

def normalizeTab(string, tab):
	if len(string) == 0: return ''
	count = 0
	tablen = len(tab)
	isSpaces = tab[0] == ' '
	space4 = ' ' * 4
	prevlen = -1
	while len(string) > 0 and string[0] in ' \t':
		c = string[0]
		if c == '\t':
			string = string[1:]
			count += 1
		elif isSpaces:
			if string[:tablen] == tab:
				string = string[tablen:]
				count += 1
		else:
			if string[:4] == space4:
				string = string[4:]
				count += 1
		newlen = len(string)
		if newlen == prevlen:
			break
		prevlen = newlen
	return (tab * count) + string

def normalize(filepath, lineEnding, tab, includeNewlineAtEnd):
	originaltext = loadFile(filepath)
	text = originaltext.replace('\r\n', '\n').replace('\r', '\n')
	text = rtrim(text)
	
	output = []
	for line in text.split('\n'):
		
		line = rtrim(line)
		line = normalizeTab(line, tab)
		
		output.append(line)
	
	if includeNewlineAtEnd:
		output.append('')
	
	text = lineEnding.join(output)
	
	while '\n\n\n' in text:
		text = text.replace('\n\n\n', '\n\n')
		
	writeFile(filepath, text)
	return originaltext != text

def getFiles(dir, ending):
	output = []
	getFilesImpl(dir, ending, output)
	return output

def getFilesImpl(dir, ending, output):
	for file in os.listdir(dir):
		filepath = dir + os.sep + file
		if os.path.isdir(filepath):
			getFilesImpl(filepath, ending, output)
		else:
			if file.endswith(ending):
				output.append(filepath)

def normalizeBatch(dir, ending, lineEnding, tab, includeNewlineAtEnd):
	files = getFiles(dir, ending)
	for file in files:
		changes = normalize(file, lineEnding, tab, includeNewlineAtEnd)
		if changes:
			print('FIXED: ' + file)
		

def main():
	normalizeBatch('Compiler', '.cs', '\n', ' ' * 4, True)
	normalizeBatch('Compiler', '.csproj', '\n', ' ' * 2, False)
	normalizeBatch('Interpreter', '.cry', '\n', '\t', True)
	normalizeBatch('Resources/game-python-pygame', '.py', '\n', ' ' * 2, True)
	normalizeBatch('Resources/game-javascript', '.js', '\n', ' ' * 2, True)
	normalizeBatch('Demos', '.cry', '\n', ' ' * 4, True)

main()
