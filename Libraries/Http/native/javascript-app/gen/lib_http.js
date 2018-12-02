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

var lib_http_fastEnsureAllBytes = function(vm, args) {
	if ((args[0][0] == 6)) {
		var list1 = args[0][1];
		var i = list1[1];
		var int1 = 0;
		var intArray1 = PST$createNewArray(i);
		var value = null;
		while ((i > 0)) {
			i -= 1;
			value = list1[2][i];
			if ((value[0] != 3)) {
				return vm[16];
			}
			int1 = value[1];
			if ((int1 < 0)) {
				if ((int1 < -128)) {
					return vm[16];
				}
				int1 += 256;
			} else {
				if ((int1 >= 256)) {
					return vm[16];
				}
			}
			intArray1[i] = int1;
		}
		var objArray1 = PST$createNewArray(1);
		objArray1[0] = intArray1;
		var objInstance1 = args[1][1];
		objInstance1[3] = objArray1;
		return vm[15];
	}
	return vm[16];
};

var lib_http_getResponseBytes = function(vm, args) {
	var outputListValue = args[1];
	var objInstance1 = args[0][1];
	var objArray1 = objInstance1[3];
	var tList = [];
	LIB$http$getResponseBytes(objArray1[0], vm[13][9], tList);
	var outputList = outputListValue[1];
	outputList[2] = tList;
	outputList[1] = tList.length;
	return outputListValue;
};

var lib_http_pollRequest = function(vm, args) {
	var objInstance1 = args[0][1];
	var objArray1 = objInstance1[3];
	if (LIB$http$pollRequest(objArray1)) {
		return vm[15];
	}
	return vm[16];
};

var lib_http_populateResponse = function(vm, args) {
	var arg2 = args[1];
	var arg3 = args[2];
	var arg4 = args[3];
	var objInstance1 = args[0][1];
	var object1 = objInstance1[3][0];
	var objArray1 = PST$createNewArray(1);
	var stringList1 = [];
	LIB$http$readResponseData(object1, PST$intBuffer16, PST$stringBuffer16, objArray1, stringList1);
	objInstance1 = arg2[1];
	objInstance1[3] = objArray1;
	var outputList = arg3[1];
	outputList.push(buildInteger(vm[13], PST$intBuffer16[0]));
	outputList.push(buildString(vm[13], PST$stringBuffer16[0]));
	var value = vm[14];
	var value2 = vm[15];
	if ((PST$intBuffer16[1] == 0)) {
		value = buildString(vm[13], PST$stringBuffer16[1]);
		value2 = vm[16];
	}
	outputList.push(value);
	outputList.push(value2);
	var list1 = arg4[1];
	var i = 0;
	while ((i < stringList1.length)) {
		list1.push(buildString(vm[13], stringList1[i]));
		i += 1;
	}
	return vm[14];
};

var lib_http_sendRequest = function(vm, args) {
	var body = args[5];
	var objInstance1 = args[0][1];
	var objArray1 = PST$createNewArray(3);
	objInstance1[3] = objArray1;
	objArray1[2] = false;
	var method = args[2][1];
	var url = args[3][1];
	var headers = [];
	var list1 = args[4][1];
	var i = 0;
	while ((i < list1[1])) {
		headers.push(list1[2][i][1]);
		i += 1;
	}
	var bodyRawObject = body[1];
	var bodyState = 0;
	if ((body[0] == 5)) {
		bodyState = 1;
	} else {
		if ((body[0] == 8)) {
			objInstance1 = bodyRawObject;
			bodyRawObject = objInstance1[3][0];
			bodyState = 2;
		} else {
			bodyRawObject = null;
		}
	}
	var getResponseAsText = (args[6][1] == 1);
	if (args[1][1]) {
		LIB$http$sendRequestAsync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText);
	} else {
		if (LIB$http$sendRequestSync(objArray1, method, url, headers, bodyState, bodyRawObject, getResponseAsText, vm_getCurrentExecutionContextId(vm))) {
			vm_suspend(vm, 1);
		}
	}
	return vm[14];
};
