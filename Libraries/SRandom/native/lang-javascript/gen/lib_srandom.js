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

var lib_srandom_getBoolean = function(vm, args) {
	var intPtr = args[0][1];
	var value = 0;
	value = (((intPtr[2][0][1] * 20077) + 12345) & 65535);
	intPtr[2][0] = buildInteger(vm[13], value);
	if (((value & 1) == 0)) {
		return vm[16];
	}
	return vm[15];
};

var lib_srandom_getFloat = function(vm, args) {
	var intPtr = args[0][1];
	var value1 = 0;
	value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535);
	var value2 = (((value1 * 20077) + 12345) & 65535);
	var value3 = (((value2 * 20077) + 12345) & 65535);
	intPtr[2][0] = buildInteger(vm[13], value3);
	value1 = ((value1 >> 8) & 255);
	value2 = ((value2 >> 8) & 255);
	value3 = ((value3 >> 8) & 255);
	return buildFloat(vm[13], (((value1 << 16) | (value2 << 8) | value3) / 16777216.0));
};

var lib_srandom_getInteger = function(vm, args) {
	var intPtr = args[0][1];
	var value1 = 0;
	value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535);
	var value2 = (((value1 * 20077) + 12345) & 65535);
	var value3 = (((value2 * 20077) + 12345) & 65535);
	var value4 = (((value3 * 20077) + 12345) & 65535);
	intPtr[2][0] = buildInteger(vm[13], value4);
	value1 = ((value1 >> 8) & 255);
	value2 = ((value2 >> 8) & 255);
	value3 = ((value3 >> 8) & 255);
	value4 = ((value4 >> 8) & 127);
	return buildInteger(vm[13], ((value4 << 24) | (value3 << 16) | (value2 << 8) | value1));
};
