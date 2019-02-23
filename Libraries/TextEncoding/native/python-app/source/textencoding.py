def lib_textencoding_bytesToText(byteValues, format, strOut):
	if format == 1 or format == 2:
		strOut[0] = ''.join(map(chr, byteValues))
		return 0
	
	if sys.version_info.major == 2:
		raise Exception("Not implemented")
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
			return 0
		except:
			return 1
		
	raise Exception("Not implemented")

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
		if (sys.version_info.major == 2):
			raise Exception("Not implemented")
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