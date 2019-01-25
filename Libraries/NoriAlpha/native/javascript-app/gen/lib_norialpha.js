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

var lib_norialpha_closeFrame = function(vm, args) {
	var frameObj = args[0][1];
	var nativeFrameHandle = frameObj[3][0];
	NoriHelper.CloseFrame(nativeFrameHandle);
	return vm[14];
};

var lib_norialpha_encodeListToWireFormat = function(v) {
	var args = v[1];
	var sb = [];
	var valueList = null;
	var valueArray = null;
	var i = 0;
	var blindCopy = 0;
	var intValue = 0;
	var length = args[1];
	valueList = args[2];
	blindCopy = (2 + valueList[1][1]);
	i = 0;
	while ((i < blindCopy)) {
		if ((i > 0)) {
			sb.push(",");
		}
		intValue = valueList[i][1];
		sb.push(('' + intValue));
		i += 1;
	}
	var childCount = 0;
	var propertyCount = 0;
	var j = 0;
	var key = "";
	var value = "";
	while ((i < length)) {
		sb.push(",");
		sb.push(valueList[i][1]);
		sb.push(",");
		sb.push(('' + valueList[(i + 1)][1]));
		childCount = valueList[(i + 2)][1];
		propertyCount = valueList[(i + 3)][1];
		sb.push(",");
		sb.push(('' + childCount));
		sb.push(",");
		sb.push(('' + propertyCount));
		i += 4;
		j = 0;
		while ((j < childCount)) {
			sb.push(",");
			intValue = valueList[(i + j)][1];
			sb.push(('' + intValue));
			j += 1;
		}
		i += childCount;
		j = 0;
		while ((j < propertyCount)) {
			key = valueList[i][1];
			value = valueList[(i + 1)][1];
			sb.push(",");
			sb.push(key);
			sb.push(",");
			sb.push(NoriHelper.EscapeStringHex(value));
			i += 2;
			j += 1;
		}
	}
	return sb.join("");
};

var lib_norialpha_flushUpdatesToFrame = function(vm, args) {
	var frameObj = args[0][1];
	var nativeFrameHandle = frameObj[3][0];
	var data = args[1][1];
	NoriHelper.FlushUpdatesToFrame(nativeFrameHandle, data);
	return vm[14];
};

var lib_norialpha_runEventWatcher = function(vm, args) {
	var frameObj = args[0][1];
	var execContextIdForResume = args[1][1];
	var eventCallback = args[2];
	var ec = getExecutionContext(vm, execContextIdForResume);
	vm_suspend_for_context(ec, 1);
	NoriHelper.EventWatcher(vm, execContextIdForResume, eventCallback);
	return vm[14];
};

var lib_norialpha_showFrame = function(vm, args) {
	var frameObj = args[0][1];
	var title = args[1][1];
	var width = args[2][1];
	var height = args[3][1];
	var data = args[4][1];
	var execId = args[5][1];
	frameObj[3] = PST$createNewArray(1);
	frameObj[3][0] = NoriHelper.ShowFrame(args[0], title, width, height, data, execId);
	return vm[14];
};
