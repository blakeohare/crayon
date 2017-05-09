import os

class FormatStyle:
	def __init__(self):
		self.newline_char = '\n'
		self.tab_char = '    '
		self.should_rtrim = True
		self.should_trim = True
		self.should_end_with_newline = True

	def tabs(self, tab_char):
		self.tab_char = tab_char
		return self

	def newline(self, newline_char):
		self.newline_char = newline_char
		return self

	def rtrim(self, should_rtrim):
		self.should_rtrim = should_rtrim
		return self

	def apply(self, text):
		if self.should_trim:
			text = text.strip()

		lines = text.replace('\r\n', '\n').split('\n')

		if self.should_rtrim:
			lines = map(lambda x:x.rstrip(), lines)

		new_lines = []
		for line in lines:
			tabs = 0
			trimming = True
			while trimming:
				if len(line) > 0:
					if line[0] == '\t':
						line = line[1:]
						tabs += 1
					elif line.startswith(self.tab_char):
						line = line[len(self.tab_char):]
						tabs += 1
					else:
						trimming = False
				else:
					trimming = False
			if tabs > 0:
				new_lines.append((self.tab_char * tabs) + line)
			else:
				new_lines.append(line)
		
		if self.should_end_with_newline:
			new_lines.append('')
		text = self.newline_char.join(new_lines)


MATCHERS = [
	('Compiler/*.cs', FormatStyle().tabs('    ').newline('\r\n'))
]

def get_all_files():
	output = []
	get_all_files_impl('.', output)
	return output

def get_all_files_impl(path, output):
	for file in os.listdir(path):
		full_path = path + os.sep + file
		if os.path.isdir(full_path):
			get_all_files_impl(full_path, output)
		else:
			output.append(full_path[2:])

all_files = get_all_files()
for pattern, matcher in MATCHERS:
    prefix, ext = pattern.split('*')
    for file in all_files:
        if file.startswith(prefix) and file.endswith(ext):
            print "This file matches: ", file


