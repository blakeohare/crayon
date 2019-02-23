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

var lib_textencoding_convertBytesToText = function(vm, args) {
	if ((args[0][0] != 6)) {
		return buildInteger(vm[13], 2);
	}
	var byteList = args[0][1];
	var format = args[1][1];
	var output = args[2][1];
	var strOut = PST$stringBuffer16;
	var length = byteList[1];
	var unwrappedBytes = PST$createNewArray(length);
	var i = 0;
	var value = null;
	var c = 0;
	while ((i < length)) {
		value = byteList[2][i];
		if ((value[0] != 3)) {
			return buildInteger(vm[13], 3);
		}
		c = value[1];
		if (((c < 0) || (c > 255))) {
			return buildInteger(vm[13], 3);
		}
		unwrappedBytes[i] = c;
		i += 1;
	}
	var sc = LIB$textencoding$bytesToText(unwrappedBytes, format, strOut);
	if ((sc == 0)) {
		addToList(output, buildString(vm[13], strOut[0]));
	}
	return buildInteger(vm[13], sc);
};

var lib_textencoding_convertTextToBytes = function(vm, args) {
	var value = args[0][1];
	var format = args[1][1];
	var includeBom = args[2][1];
	var output = args[3][1];
	var byteList = [];
	var intOut = PST$intBuffer16;
	var sc = LIB$textencoding$textToBytes(value, includeBom, format, byteList, vm[13][9], intOut);
	var swapWordSize = intOut[0];
	if ((swapWordSize != 0)) {
		var i = 0;
		var j = 0;
		var length = byteList.length;
		var swap = null;
		var half = (swapWordSize >> 1);
		var k = 0;
		while ((i < length)) {
			k = (i + swapWordSize - 1);
			j = 0;
			while ((j < half)) {
				swap = byteList[(i + j)];
				byteList[(i + j)] = byteList[(k - j)];
				byteList[(k - j)] = swap;
				j += 1;
			}
			i += swapWordSize;
		}
	}
	if ((sc == 0)) {
		addToList(output, buildList(byteList));
	}
	return buildInteger(vm[13], sc);
};
