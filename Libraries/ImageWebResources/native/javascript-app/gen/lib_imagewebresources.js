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

var lib_imagewebresources_bytesToImage = function(vm, args) {
	var objInstance1 = args[0][1];
	var object1 = objInstance1[3][0];
	var list1 = args[1][1];
	var value = getItemFromList(list1, 0);
	var objArray1 = PST$createNewArray(3);
	objInstance1 = value[1];
	objInstance1[3] = objArray1;
	if (C$common$alwaysTrue()) {
		var width = buildInteger(vm[13], objArray1[1]);
		var height = buildInteger(vm[13], objArray1[2]);
		list1[2][1] = width;
		list1[2][2] = height;
		return vm[15];
	}
	return vm[16];
};

var lib_imagewebresources_jsDownload = function(vm, args) {
	var url = args[0][1];
	var objInstance1 = args[1][1];
	var objArray1 = PST$createNewArray(5);
	objInstance1[3] = objArray1;
	objArray1[0] = false;
	LIB$imagewebresources$download(url, objArray1);
	return vm[14];
};

var lib_imagewebresources_jsGetImage = function(vm, args) {
	var objInstance1 = args[0][1];
	var objArray1 = objInstance1[3];
	var list1 = args[1][1];
	if (objArray1[1]) {
		var object1 = objArray1[2];
		var value = getItemFromList(list1, 0);
		objInstance1 = value[1];
		var objArray2 = PST$createNewArray(3);
		objInstance1[3] = objArray2;
		var width = objArray1[3];
		var height = objArray1[4];
		objArray2[0] = object1;
		objArray2[1] = width;
		objArray2[2] = height;
		clearList(list1);
		addToList(list1, value);
		addToList(list1, buildInteger(vm[13], width));
		addToList(list1, buildInteger(vm[13], height));
		return vm[15];
	}
	return vm[16];
};

var lib_imagewebresources_jsPoll = function(vm, args) {
	var objInstance1 = args[0][1];
	if (objInstance1[3][0]) {
		return vm[15];
	}
	return vm[16];
};
