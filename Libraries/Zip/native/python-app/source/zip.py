
def lib_zip_createZipReaderImpl(byteArray):
	arr = array.array('B', byteArray)
	if sys.version_info[0] == 2:
		bArr = arr.tostring()
	else:
		bArr = arr.tobytes()
	try:
		archive = zipfile.ZipFile(io.BytesIO(bArr))
		return (archive, archive.namelist())
	except:
		return None
	
def lib_zip_readNextZipEntryImpl(nativeZipArchive, fileReadCount, boolsOut, nameOut, bytesOut):
	boolsOut[0] = True
	archive, entries = nativeZipArchive
	length = len(entries)
	isDone = fileReadCount >= length
	boolsOut[1] = not isDone
	if isDone: return
	
	name = entries[fileReadCount]
	entryByteArray = archive.read(name)
	boolsOut[2] = False # TODO: how to check this?
	nameOut[0] = name

	if sys.version_info[0] == 2:
		for b in entryByteArray:
			bytesOut.append(ord(b))
	else:
		for b in entryByteArray:
			bytesOut.append(b)
