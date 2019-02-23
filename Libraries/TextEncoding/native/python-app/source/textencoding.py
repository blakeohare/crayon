def lib_textencoding_bytesToText(byteValues, format, strOut):
	if format == 1 or format == 2:
		m = max(byteValues)
		if format == 1 and m > 127: return 4
		strOut[0] = ''.join(map(chr, byteValues))
		return 0
	
	if sys.version_info.major == 2:
		i = 0
		length = len(byteValues)
		cp = 0
		if format == 3:
			# this just verifies that the bytes are correct.
			# because Python strings in 2.x are inherently UTF-8, the original
			# value is just mapped to chr in the end.
			codePoints = lib_textencoding_utf8ToCodePoints(byteValues)
			if codePoints == None: return 4
			strOut[0] = ''.join(map(chr, byteValues))

		elif format == 4 or format == 5:
			isBigEndian = format == 5
			codePoints = []
			while i < length:
				if isBigEndian:
					a = byteValues[i]
					b = byteValues[i + 1]
				else:
					a = byteValues[i + 1]
					b = byteValues[i]
				w1 = (a << 8) | b
				i += 2
				
				if w1 < 0xD800 or w1 > 0xDFFF:
					cp = w1
				elif w1 < 0xD800 or w1 >= 0xDC00:
					return 4
				elif i + 1 >= length:
					return 4
				else:
					if isBigEndian:
						a = byteValues[i]
						b = byteValues[i + 1]
					else:
						a = byteValues[i + 1]
						b = byteValues[i]
					w2 = (a << 8) | b
					i += 2
					if w2 < 0xDC00 or w2 > 0xDFFF:
						return 4
					cp = (((w1 & 0x03FF) << 10) | (w2 & 0x03FF)) + 0x10000
				codePoints.append(cp)
			buffer = []
			lib_textencoding_py2x_codePointsToUtf8(codePoints, buffer)
			strOut[0] = ''.join(map(chr, buffer))
		
		elif format == 6 or format == 7:
			isBigEndian = format == 5
			codePoints = []
			while i < length:
				if isBigEndian:
					a = byteValues[i]
					b = byteValues[i + 1]
					c = byteValues[i + 2]
					d = byteValues[i + 3]
				else:
					a = byteValues[i + 3]
					b = byteValues[i + 2]
					c = byteValues[i + 1]
					d = byteValues[i]
				codePoints.append((a << 24) | (b << 16) | (c << 8) | d)
				i += 4
			buffer = []
			lib_textencoding_py2x_codePointsToUtf8(codePoints, buffer)
			strOut[0] = ''.join(map(chr, buffer))
			
	else:
		encoding = None
		if format == 3:
			encoding = 'utf-8'
		elif format == 4:
			encoding = 'utf-16le'
		elif format == 5:
			encoding = 'utf-16be'
		elif format == 6:
			encoding = 'utf-32le'
		elif format == 7:
			encoding = 'utf-32be'
		
		try:
			strOut[0] = bytes(byteValues).decode(encoding)
		except:
			return 4
	return 0

def lib_textencoding_stringToBytesDirect(value):
	bytes = []
	for c in value:
		bytes.append(ord(c))
	return bytes

def lib_textencoding_utf8ToCodePoints(bytes):
	codePoints = []
	cp = 0
	length = len(bytes)
	i = 0
	while i < length:
		c = bytes[i]
		if (c & 0x80) == 0:
			codePoints.append(c)
			i += 1
		elif (c & 0xE0) == 0xC0:
			if i + 1 >= length: return None
			cp = c & 0x1F
			c = bytes[i + 1]
			cp = (cp << 6) | (c & 0x3F)
			codePoints.append(cp)
			i += 2
		elif (c & 0xF0) == 0xE0:
			if i + 2 >= length: return None
			cp = c & 0x0F
			c = bytes[i + 1]
			if (c & 0xC0) != 0x80: return None
			cp = (cp << 6) | (c & 0x3F)
			c = bytes[i + 2]
			if (c & 0xC0) != 0x80: return None
			cp = (cp << 6) | (c & 0x3F)
			codePoints.append(cp)
			i += 3
		elif (c & 0xF8) == 0xF0:
			if i + 3 >= length: return None
			cp = c & 0x07
			c = bytes[i + 1]
			if (c & 0xC0) != 0x80: return None
			cp = (cp << 6) | (c & 0x3F)
			c = bytes[i + 2]
			if (c & 0xC0) != 0x80: return None
			cp = (cp << 6) | (c & 0x3F)
			c = bytes[i + 3]
			if (c & 0xC0) != 0x80: return None
			cp = (cp << 6) | (c & 0x3F)
			codePoints.append(cp)
			i += 4
	return codePoints
	
def lib_textencoding_py2x_stringToCodePoints(value):
	bytes = lib_textencoding_stringToBytesDirect(value)
	# Python 2.x strings in Crayon are inherently BOM-less UTF-8.
	return lib_textencoding_utf8ToCodePoints(bytes)

def lib_textencoding_py2x_codePointsToUtf8(codePoints, buffer):
	for cp in codePoints:
		if cp < 128:
			buffer.append(cp)
		elif cp < 0x0800:
			buffer.append(0xC0 | ((cp >> 6) & 0x1F))
			buffer.append(0x80 | (cp & 0x3F))
		elif cp < 0x10000:
			buffer.append(0xE0 | ((cp >> 12) & 0x0F))
			buffer.append(0x80 | ((cp >> 6) & 0x3F))
			buffer.append(0x80 | (cp & 0x3F))
		else:
			buffer.append(0xF0 | ((cp >> 18) & 3))
			buffer.append(0x80 | ((cp >> 12) & 0x3F))
			buffer.append(0x80 | ((cp >> 6) & 0x3F))
			buffer.append(0x80 | (cp & 0x3F))
		
def lib_textencoding_py2x_codePointsToUtf16(codePoints, buffer, isBigEndian):
	for cp in codePoints:
		if cp < 0x10000:
			if isBigEndian:
				buffer.append((cp >> 8) & 255)
				buffer.append(cp & 255)
			else:
				buffer.append(cp & 255)
				buffer.append((cp >> 8) & 255)
		else:
			cp -= 0x10000
			w1 = (cp >> 10) & 0x03FF
			w2 = cp & 0x03FF
			w1 = w1 | 0xD800
			w2 = w2 | 0xDC00
			a = (w1 >> 8) & 255
			b = w1 & 255
			c = (w2 >> 8) & 255
			d = w2 & 255
			if isBigEndian:
				buffer.append(a)
				buffer.append(b)
				buffer.append(c)
				buffer.append(d)
			else:
				buffer.append(b)
				buffer.append(a)
				buffer.append(d)
				buffer.append(c)

def lib_textencoding_py2x_codePointsToUtf32(codePoints, buffer, isBigEndian):
	for cp in codePoints:
		a = (cp >> 24) & 255
		b = (cp >> 16) & 255
		c = (cp >> 8) & 255
		d = cp & 255
		if isBigEndian:
			buffer.append(a)
			buffer.append(b)
			buffer.append(c)
			buffer.append(d)
		else:
			buffer.append(d)
			buffer.append(c)
			buffer.append(b)
			buffer.append(a)

def lib_textencoding_textToBytes(value, includeBom, format, byteList, positiveIntegers, intOut):
	output = []
	intOut[0] = 0
	if format == 1 or format == 2:
		for c in value:
			n = ord(c)
			if n < 0 or n > 255:
				return 1
			output.append(n)
	else:
		if includeBom:
			if format == 3:
				output = [239, 187, 191]
			elif format == 4:
				output = [255, 254]
			elif format == 5:
				output = [254, 255]
			elif format == 6:
				output = [255, 254, 0, 0]
			elif format == 7:
				output = [0, 0, 254, 255]
		
		if (sys.version_info.major == 2):
			codePoints = lib_textencoding_py2x_stringToCodePoints(value)
			if codePoints == None: return 1
			if format == 3:
				output += map(ord, value) # original is okay as-is. Previous conversion proved it's well-formed.
			elif format == 4 or format == 5:
				lib_textencoding_py2x_codePointsToUtf16(codePoints, output, format == 5)
			elif format == 6 or format == 7:
				lib_textencoding_py2x_codePointsToUtf32(codePoints, output, format == 7)
		else:
			blist = None
			if format == 3:
				blist = bytes(value, 'utf-8')
			elif format == 4:
				blist = bytes(value, 'utf-16le')
			elif format == 5:
				blist = bytes(value, 'utf-16be')
			elif format == 6:
				blist = bytes(value, 'utf-32le')
			elif format == 7:
				blist = bytes(value, 'utf-32be')
			
			for b in blist:
				output.append(b)
		
	for n in output:
		byteList.append(positiveIntegers[n])
	
	return 0