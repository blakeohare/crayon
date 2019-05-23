PST$sortedCopyOfArray = function(n) {
	var a = n.concat([]);
	a.sort();
	return a;
};

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

PST$checksubstring = function(s, index, lookfor) { return s.substring(index, index + lookfor.length) === lookfor; };

PST$stringTrimOneSide = function(s, isLeft) {
	var i = isLeft ? 0 : s.length - 1;
	var end = isLeft ? s.length : -1;
	var step = isLeft ? 1 : -1;
	var c;
	var trimming = true;
	while (trimming && i != end) {
		c = s.charAt(i);
		switch (c) {
			case ' ':
			case '\n':
			case '\t':
			case '\r':
				i += step;
				break;
			default:
				trimming = false;
				break;
		}
	}

	return isLeft ? s.substring(i) : s.substring(0, i + 1);
};

PST$floatParseHelper = function(o, s) {
	var t = parseFloat(s);
	if (t + '' == 'NaN') {
		o[0] = -1;
	} else {
		o[0] = 1;
		o[1] = t;
	}
};

PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$dictionaryKeys = function(d) {
	var o = [];
	for (var k in d) {
		o.push(k);
	}
	return o;
};

PST$dictionaryValues = function(d) {
	var o = [];
	for (var k in d) {
		o.push(d[k]);
	}
	return o;
};

PST$is_valid_integer = function(n) {
	var t = parseInt(n);
	return t < 0 || t >= 0;
};

PST$clearList = function(v) {
	v.length = 0;
};

PST$shuffle = function(v) {
	var t;
	var len = v.length;
	var sw;
	for (i = len - 1; i >= 0; --i) {
		sw = Math.floor(Math.random() * len);
		t = v[sw];
		v[sw] = v[i];
		v[i] = t;
	}
};

PST$stringEndsWith = function(s, v) {
	return s.indexOf(v, s.length - v.length) !== -1;
};

PST$intBuffer16 = PST$multiplyList([0], 16);
PST$floatBuffer16 = PST$multiplyList([0.0], 16);
PST$stringBuffer16 = PST$multiplyList([''], 16);

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

var lib_zip_initializeZipReader = function(vm, args) {
	var sc = 0;
	var byteArray = lib_zip_validateByteList(args[1], true);
	if ((byteArray == null)) {
		sc = 1;
	} else {
		var obj = args[0][1];
		obj[3] = PST$createNewArray(1);
		obj[3][0] = lib_zip_createZipReaderImpl(byteArray);
		obj[3][1] = 0;
		if ((obj[3][0] == null)) {
			sc = 2;
		}
	}
	return buildInteger(vm[13], sc);
};

var lib_zip_readerPeekNextEntry = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	var output = args[1][1];
	var boolOut = PST$createNewArray(3);
	var nameOut = PST$createNewArray(1);
	var integers = [];
	lib_zip_readNextZipEntryImpl(nd[0], nd[1], boolOut, nameOut, integers);
	var problemsEncountered = !boolOut[0];
	var foundAnything = boolOut[1];
	var isDirectory = boolOut[2];
	if (problemsEncountered) {
		return vm[16];
	}
	nd[1] = (1 + nd[1]);
	setItemInList(output, 0, buildBoolean(vm[13], foundAnything));
	if (!foundAnything) {
		return vm[15];
	}
	setItemInList(output, 1, buildString(vm[13], nameOut[0]));
	if (isDirectory) {
		setItemInList(output, 2, buildBoolean(vm[13], isDirectory));
		return vm[15];
	}
	var byteValues = getItemFromList(output, 3)[1];
	var length = integers.length;
	var i = 0;
	var positiveNumbers = vm[13][9];
	var valuesOut = byteValues[2];
	i = 0;
	while ((i < length)) {
		valuesOut.push(positiveNumbers[integers[i]]);
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
