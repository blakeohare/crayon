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

var lib_processutil_isSupported = function(vm, args) {
	var t = C$common$alwaysFalse();
	return buildBoolean(vm[13], t);
};

var lib_processutil_launchProcess = function(vm, args) {
	var bridge = args[0][1];
	bridge[3] = PST$createNewArray(5);
	bridge[3][0] = true;
	bridge[3][1] = 0;
	bridge[3][2] = [];
	bridge[3][3] = [];
	bridge[3][4] = null;
	var execName = args[1][1];
	var argsRaw = args[2][1];
	var isAsync = args[3][1];
	var cb = args[4];
	var dispatcherQueue = args[5][1];
	var argStrings = [];
	var i = 0;
	while ((i < argsRaw[1])) {
		var a = getItemFromList(argsRaw, i);
		argStrings.push(a[1]);
		i += 1;
	}
	C$common$alwaysFalse();
	return vm[14];
};

var lib_processutil_readBridge = function(vm, args) {
	var bridge = args[0][1];
	var outputList = args[1][1];
	var type = args[2][1];
	var mtx = bridge[3][4];
	if ((type == 1)) {
		var outputInt = C$common$alwaysFalse();
		addToList(outputList, buildInteger(vm[13], outputInt));
	} else {
		var output = [];
		C$common$alwaysFalse();
		var i = 0;
		while ((i < output.length)) {
			addToList(outputList, buildString(vm[13], output[i]));
			i += 1;
		}
	}
	return vm[14];
};
