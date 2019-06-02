PST$multiplyList = function(l, n) {
	var o = [];
	var s = l.length;
	var i;
	while (n-- > 0) {
		for (i = 0; i < s; ++i) {
			o.push(l[i]);
		}
	}
	return o;
};

PST$stringBuffer16 = PST$multiplyList([''], 16);

PST$intBuffer16 = PST$multiplyList([0], 16);

PST$floatBuffer16 = PST$multiplyList([0.0], 16);

PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_fileiocommon_directoryCreate = function(vm, args) {
	var bool1 = false;
	var i = 0;
	var int1 = 0;
	var stringList1 = null;
	var hostObject = args[0];
	var path = args[1][1];
	if (args[2][1]) {
		int1 = 0;
		if (!LIB$fileiocommon$fakedisk$dirExists(lib_fileiocommon_getDiskObject(hostObject), '/')) {
			int1 = 4;
		} else {
			stringList1 = [];
			bool1 = true;
			while ((bool1 && !LIB$fileiocommon$fakedisk$dirExists(lib_fileiocommon_getDiskObject(hostObject), path))) {
				stringList1.push(path);
				int1 = LIB$fileiocommon$fakedisk$getPathParent(path, PST$stringBuffer16);
				path = PST$stringBuffer16[0];
				if ((int1 != 0)) {
					bool1 = false;
				}
			}
			if (bool1) {
				i = (stringList1.length - 1);
				while ((i >= 0)) {
					path = stringList1[i];
					int1 = LIB$fileiocommon$fakedisk$mkdir(lib_fileiocommon_getDiskObject(hostObject), path);
					if ((int1 != 0)) {
						i = -1;
					}
					i -= 1;
				}
			}
		}
	} else {
		int1 = LIB$fileiocommon$fakedisk$mkdir(lib_fileiocommon_getDiskObject(hostObject), path);
	}
	return buildInteger(vm[13], int1);
};

var lib_fileiocommon_directoryDelete = function(vm, args) {
	var sc = LIB$fileiocommon$fakedisk$rmdir(lib_fileiocommon_getDiskObject(args[0]), args[1][1]);
	return buildInteger(vm[13], sc);
};

var lib_fileiocommon_directoryList = function(vm, args) {
	var diskhost = args[0];
	var path = args[1][1];
	var useFullPath = args[2][1];
	var outputList = args[3][1];
	var stringList1 = [];
	var sc = LIB$fileiocommon$fakedisk$listdir(lib_fileiocommon_getDiskObject(diskhost), path, useFullPath, stringList1);
	if ((sc == 0)) {
		var i = 0;
		while ((i < stringList1.length)) {
			addToList(outputList, buildString(vm[13], stringList1[i]));
			i += 1;
		}
	}
	return buildInteger(vm[13], sc);
};

var lib_fileiocommon_directoryMove = function(vm, args) {
	var statusCode = LIB$fileiocommon$fakedisk$movedir(lib_fileiocommon_getDiskObject(args[0]), args[1][1], args[2][1]);
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileDelete = function(vm, args) {
	var statusCode = LIB$fileiocommon$fakedisk$fileDelete(lib_fileiocommon_getDiskObject(args[0]), args[1][1]);
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileInfo = function(vm, args) {
	var mask = args[2][1];
	LIB$fileiocommon$fakedisk$getPathInfoExt(lib_fileiocommon_getDiskObject(args[0]), args[1][1], mask, PST$intBuffer16, PST$floatBuffer16);
	var outputList = args[3][1];
	clearList(outputList);
	var globals = vm[13];
	addToList(outputList, buildBoolean(globals, (PST$intBuffer16[0] > 0)));
	addToList(outputList, buildBoolean(globals, (PST$intBuffer16[1] > 0)));
	if (((mask & 1) != 0)) {
		addToList(outputList, buildInteger(globals, PST$intBuffer16[2]));
	} else {
		addToList(outputList, globals[0]);
	}
	if (((mask & 2) != 0)) {
		addToList(outputList, buildBoolean(globals, (PST$intBuffer16[3] > 0)));
	} else {
		addToList(outputList, globals[0]);
	}
	if (((mask & 4) != 0)) {
		addToList(outputList, buildFloat(globals, PST$floatBuffer16[0]));
	} else {
		addToList(outputList, globals[0]);
	}
	if (((mask & 8) != 0)) {
		addToList(outputList, buildFloat(globals, PST$floatBuffer16[1]));
	} else {
		addToList(outputList, globals[0]);
	}
	return args[3];
};

var lib_fileiocommon_fileMove = function(vm, args) {
	var statusCode = LIB$fileiocommon$fakedisk$fileMove(lib_fileiocommon_getDiskObject(args[0]), args[1][1], args[2][1], args[3][1], args[4][1]);
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileRead = function(vm, args) {
	var diskHostObject = args[0];
	var sandboxedPath = args[1][1];
	var readDataAsBytes = args[2][1];
	var outputList = args[3][1];
	var tList = [];
	var statusCode = LIB$fileiocommon$fakedisk$fileRead(lib_fileiocommon_getDiskObject(diskHostObject), sandboxedPath, readDataAsBytes, PST$stringBuffer16, vm[13][9], tList);
	if (((statusCode == 0) && !readDataAsBytes)) {
		addToList(outputList, buildString(vm[13], PST$stringBuffer16[0]));
	} else {
		outputList[2] = tList;
		outputList[1] = tList.length;
	}
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileWrite = function(vm, args) {
	var ints = vm[13][9];
	if ((args[3][0] != 3)) {
		return ints[3];
	}
	var statusCode = 0;
	var contentString = null;
	var byteArrayRef = null;
	var format = args[3][1];
	if ((format == 0)) {
		byteArrayRef = lib_fileiocommon_listToBytes(args[2][1]);
		if ((byteArrayRef == null)) {
			return ints[6];
		}
	} else if ((args[2][0] != 5)) {
		return ints[6];
	} else {
		contentString = args[2][1];
	}
	if ((statusCode == 0)) {
		statusCode = LIB$fileiocommon$fakedisk$fileWrite(lib_fileiocommon_getDiskObject(args[0]), args[1][1], format, contentString, byteArrayRef);
	}
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_getCurrentDirectory = function(vm, args) {
	return buildString(vm[13], '/');
};

var lib_fileiocommon_getDiskObject = function(diskObjectArg) {
	var objInst = diskObjectArg[1];
	return objInst[3][0];
};

var lib_fileiocommon_getUserDirectory = function(vm, args) {
	return buildString(vm[13], '/');
};

var lib_fileiocommon_initializeDisk = function(vm, args) {
	var objInstance1 = args[0][1];
	var objArray1 = PST$createNewArray(1);
	objInstance1[3] = objArray1;
	var object1 = LIB$fileiocommon$fakedisk$create(args[1][1]);
	objArray1[0] = object1;
	return vm[13][0];
};

var lib_fileiocommon_isWindows = function(vm, args) {
	if (C$common$alwaysFalse()) {
		return vm[13][1];
	}
	return vm[13][2];
};

var lib_fileiocommon_listToBytes = function(listOfMaybeInts) {
	var bytes = PST$createNewArray(listOfMaybeInts[1]);
	var intValue = null;
	var byteValue = 0;
	var i = (listOfMaybeInts[1] - 1);
	while ((i >= 0)) {
		intValue = listOfMaybeInts[2][i];
		if ((intValue[0] != 3)) {
			return null;
		}
		byteValue = intValue[1];
		if ((byteValue >= 256)) {
			return null;
		}
		if ((byteValue < 0)) {
			if ((byteValue < -128)) {
				return null;
			}
			byteValue += 256;
		}
		bytes[i] = byteValue;
		i -= 1;
	}
	return bytes;
};

var lib_fileiocommon_textToLines = function(vm, args) {
	lib_fileiocommon_textToLinesImpl(vm[13], args[0][1], args[1][1]);
	return args[1];
};

var lib_fileiocommon_textToLinesImpl = function(globals, text, output) {
	var stringList = [];
	LIB$fileiocommon$fakedisk$textToLines(text, stringList);
	var _len = stringList.length;
	var i = 0;
	while ((i < _len)) {
		addToList(output, buildString(globals, stringList[i]));
		i += 1;
	}
	return 0;
};
