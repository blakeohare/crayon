def lib_fileiocommon_isWindows():
	return os.name == 'nt'

def lib_fileiocommon_getUserDirectory():
	if lib_fileiocommon_isWindows():
		return os.environ.get('USERPROFILE')
	return '~'

def lib_fileiocommon_getCurrentDirectory():
	return os.getcwd()

def lib_fileiocommon_getDirectoryList(path, includeFullPath, output):
	path = path.replace('/', os.sep)
	prefix = (path + os.sep) if includeFullPath else ''

	try:
		files = os.listdir(path)
	except:
		if os.path.exists(path):
			return 4
		return 1
	for file in files:
		output.append(prefix + file)
	
	return 0

def lib_fileiocommon_fileRead(path, isBytes, stringOut, integers, byteOutput):
	c = None
	try:
		
		if isBytes:
			c = open(path, 'rb')
			bytes = map(ord, list(c.read()))
			for byte in bytes:
				byteOutput.append(integers[byte])
		else:
			c = open(path, 'rt')
			text = c.read()
			if text[:3] == ''.join(chr(239) + chr(187) + chr(191)):
				text = text[3:]
			stringOut[0] = text
	except:
		if not os.path.exists(path):
			return 4
		return 1
	finally:
		if c != None:
			c.close()
	return 0

def lib_fileiocommon_fileWrite(path, format, content, byteIntList):

	c = None
	try:
		if format == 0:
			c = open(path, 'wb')
			c.write(bytearray(byteIntList))
		else:
			# There is too much wrong with how strings and encodings work in Python
			# that I need to address this later in one swoop.
			c = open(path, 'wt')
			c.write(content)
		return 0
	except:
		return 1
	finally:
		if c != None:
			c.close()
	return 0

def lib_fileiocommon_fileDelete(path):

	if not os.path.exists(path):
		return 4
	try:
		os.remove(path)
	except:
		return 1
	return 0

def lib_fileiocommon_getFileInfo(path, mask, intOut, floatOut):
	exists = os.path.exists(path)
	dirExists = os.path.isdir(path)
	fileExists = exists and not dirExists
	
	if exists and mask != 0:
		
		fileAttributes = os.stat(path)[0]
		
		fetchSize = (mask & 1) != 0
		fetchReadOnly = (mask & 2) != 0
		fetchCreateTime = (mask & 4) != 0
		fetchModifyTime = (mask & 8) != 0
		
		if fetchSize:
			if fileExists:
				intOut[2] = os.path.getsize(path)
			else:
				intOut[2] = 0
		
		if fetchReadOnly:
			if fileExists:
				import stat
				intOut[3] = 1 if (not fileAttributes & stat.S_IWRITE) else 0
		
		if fetchCreateTime:
			floatOut[0] = os.path.getctime(path)
		
		if fetchModifyTime:
			floatOut[1] = os.path.getmtime(path)
		
	intOut[0] = 1 if exists else 0
	intOut[1] = 1 if dirExists else 0

def lib_fileiocommon_createDirectory(path):
	out = [0]
	status = lib_fileiocommon_getDirParent(path, out)
	if status != 0: return status
	parent = out[0]
	parentExists = os.path.isdir(parent)
	
	if not parentExists: return 11
	try:
		os.mkdir(path)
		return 0
	except:
		return 1

def lib_fileiocommon_moveDirectory(pathFrom, pathTo):
	import shutil
	
	if not os.path.exists(pathFrom):
		return 4
	if not os.path.isdir(pathFrom):
		return 4
	if not os.path.isdir(pathTo + os.sep + '..'):
		return 11
	try:
		shutil.move(pathFrom, pathTo)
	except:
		return 1
	
	return 0

def lib_fileiocommon_deleteDirectory(path):
	
	if not os.path.isdir(path):
		return 4
	
	import shutil
	try:
		shutil.rmtree(path)
	except:
		return 1
	return 0

def lib_fileiocommon_getDirParent(path, stringOut):
	if lib_fileiocommon_isWindows() and len(path) > 259:
		return 5
	stringOut[0] = '/'.join(path.replace('\\', '/').split('/')[:-1]).replace('/', os.sep)
	return 0

def lib_fileiocommon_getDirRoot(path):
	if lib_fileiocommon_isWindows():
		return path.split(':')[0] + ':\\'
	return '/'

def lib_fileiocommon_directoryExists(path):
	return os.path.isdir(path)

def lib_fileiocommon_textToLines(text, output):
	startIndex = 0
	i = 0
	length = len(text)
	while i < length:
		c = text[i]
		if c == '\r' and text[i:i + 2] == '\r\n':
			output.append(text[startIndex:i + 2])
			startIndex = i + 2
			i += 1
		elif c == '\n' or c == '\r':
			output.append(text[startIndex:i + 1])
			startIndex = i + 1
		i += 1
	output.append(text[startIndex:])
