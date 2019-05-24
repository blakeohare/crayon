lib_zip_createZipReaderImpl = function(intArray, vm, execId, nativeData, scOut) {
	var zip = new JSZip();
	var bytes = new Uint8Array(intArray).buffer;
	JSZip.loadAsync(bytes).then(function(archive) {
		setTimeout(function() {
			var names = [];
			archive.forEach(function(fileName, ignore) {
				names.push(fileName);
			});
			lib_zip_initAsyncCallback(scOut, nativeData, [archive, names], vm, execId);
		}, 0);
	});
	return null;
};

lib_zip_readNextZipEntryImpl = function(nativeZipArchive, fileReadCount, boolOut, stringOut, bytesOut, vm, execId, nativeData, output) {
	var files = nativeZipArchive[1];
	if (fileReadCount >= files.length) {
		boolOut[0] = false;
		lib_zip_readNextEntryRestartVm(vm, execId);
		return;
	}
	
	var filename = files[fileReadCount];
	var archive = nativeZipArchive[0];
	
	var onFinished = function(data) {
		var bytes = [];
		data.forEach((b) => { bytes.push(b); });
		lib_zip_readerPeekNextEntryCallback(false, true, false, filename, bytes, nativeData, output, vm);
		lib_zip_readNextEntryRestartVm(vm, execId);
	};
	
	var onError = function(e) {
		lib_zip_readerPeekNextEntryCallback(true, false, false, '', [], nativeData, output, vm);
		lib_zip_readNextEntryRestartVm(vm, execId);
	};
	
	var fileData = archive.file(filename);
	fileData.internalStream('uint8array').on('data', onFinished).on('error', onError).resume();
};

lib_zip_readNextEntryRestartVm = function(vm, execId) {
	setTimeout(function() { runInterpreter(vm, execId); }, 0);
};
