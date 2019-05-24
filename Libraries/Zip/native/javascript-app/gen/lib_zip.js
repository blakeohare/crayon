PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_zip_ensureValidArchiveInfo = function(vm, args) {
	var sc = 0;
	if ((args[0][0] != 5)) {
		sc = 1;
	}
	if (((sc == 0) && (lib_zip_validateByteList(args[1], false) != null))) {
		sc = 2;
	}
	return buildInteger(vm[13], sc);
};

var lib_zip_initAsyncCallback = function(scOut, nativeData, nativeZipArchive, vm, execContext) {
	var sc = 0;
	if ((nativeZipArchive == null)) {
		sc = 2;
	}
	setItemInList(scOut, 0, buildInteger(vm[13], sc));
	nativeData[0] = nativeZipArchive;
	runInterpreter(vm, execContext);
};

var lib_zip_initializeZipReader = function(vm, args) {
	var sc = 0;
	var scOut = args[2][1];
	var execId = args[3][1];
	var byteArray = lib_zip_validateByteList(args[1], true);
	if ((byteArray == null)) {
		sc = 1;
	} else {
		var obj = args[0][1];
		obj[3] = PST$createNewArray(2);
		obj[3][0] = lib_zip_createZipReaderImpl(byteArray, vm, execId, obj[3], scOut);
		obj[3][1] = 0;
		if ((obj[3][0] == null)) {
			sc = 2;
		} else {
			sc = 0;
		}
		if (C$common$alwaysTrue()) {
			sc = 3;
			vm_suspend_context_by_id(vm, execId, 1);
		}
	}
	setItemInList(scOut, 0, buildInteger(vm[13], sc));
	return vm[14];
};

var lib_zip_readerPeekNextEntry = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	var output = args[1][1];
	var execId = args[2][1];
	var boolOut = PST$createNewArray(3);
	var nameOut = PST$createNewArray(1);
	var integers = [];
	lib_zip_readNextZipEntryImpl(nd[0], nd[1], boolOut, nameOut, integers, vm, execId, nd, output);
	if (C$common$alwaysTrue()) {
		vm_suspend_context_by_id(vm, execId, 1);
		return vm[15];
	}
	return lib_zip_readerPeekNextEntryCallback(!boolOut[0], boolOut[1], boolOut[2], nameOut[0], integers, nd, output, vm);
};

var lib_zip_readerPeekNextEntryCallback = function(problemsEncountered, foundAnything, isDirectory, name, bytesAsIntList, nativeData, output, vm) {
	if (problemsEncountered) {
		return vm[16];
	}
	nativeData[1] = (1 + nativeData[1]);
	setItemInList(output, 0, buildBoolean(vm[13], foundAnything));
	if (!foundAnything) {
		return vm[15];
	}
	setItemInList(output, 1, buildString(vm[13], name));
	if (isDirectory) {
		setItemInList(output, 2, buildBoolean(vm[13], isDirectory));
		return vm[15];
	}
	var byteValues = getItemFromList(output, 3)[1];
	var length = bytesAsIntList.length;
	var i = 0;
	var positiveNumbers = vm[13][9];
	var valuesOut = byteValues[2];
	i = 0;
	while ((i < length)) {
		valuesOut.push(positiveNumbers[bytesAsIntList[i]]);
		i += 1;
	}
	byteValues[1] = length;
	return vm[15];
};

var lib_zip_validateByteList = function(byteListValue, convert) {
	if ((byteListValue[0] != 6)) {
		return null;
	}
	var output = null;
	var bytes = byteListValue[1];
	var length = bytes[1];
	if (convert) {
		output = PST$createNewArray(length);
	} else {
		output = PST$createNewArray(1);
		output[0] = 1;
	}
	var value = null;
	var b = 0;
	var i = 0;
	while ((i < length)) {
		value = bytes[2][i];
		if ((value[0] != 3)) {
			return null;
		}
		b = value[1];
		if ((b > 255)) {
			return null;
		}
		if ((b < 0)) {
			if ((b >= -128)) {
				b += 255;
			} else {
				return null;
			}
		}
		if (convert) {
			output[i] = b;
		}
		i += 1;
	}
	return output;
};
