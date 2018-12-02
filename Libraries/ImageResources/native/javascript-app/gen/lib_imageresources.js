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

var lib_imageresources_blit = function(vm, args) {
	var object1 = null;
	var objInstance1 = args[0][1];
	var objInstance2 = args[1][1];
	LIB$imageresources$imageResourceBlitImage(objInstance1[3][0], objInstance2[3][0], args[2][1], args[3][1], args[4][1], args[5][1], args[6][1], args[7][1]);
	return vm[14];
};

var lib_imageresources_checkLoaderIsDone = function(vm, args) {
	var objInstance1 = args[0][1];
	var objInstance2 = args[1][1];
	var status = LIB$imageresources$checkLoaderIsDone(objInstance1[3], objInstance2[3]);
	return buildInteger(vm[13], status);
};

var lib_imageresources_flushImageChanges = function(vm, args) {
	return vm[14];
};

var lib_imageresources_getManifestString = function(vm, args) {
	return buildString(vm[13], LIB$imageresources$getImageResourceManifest());
};

var lib_imageresources_loadAsynchronous = function(vm, args) {
	var objInstance1 = args[0][1];
	var filename = args[1][1];
	var objInstance2 = args[2][1];
	var objArray1 = PST$createNewArray(3);
	objInstance1[3] = objArray1;
	var objArray2 = PST$createNewArray(4);
	objArray2[2] = 0;
	objInstance2[3] = objArray2;
	LIB$imageresources$imageLoad(filename, objArray1, objArray2);
	return vm[14];
};

var lib_imageresources_nativeImageDataInit = function(vm, args) {
	var objInstance1 = args[0][1];
	var nd = PST$createNewArray(4);
	var width = args[1][1];
	var height = args[2][1];
	nd[0] = LIB$imageresources$generateNativeBitmapOfSize(width, height);
	nd[1] = width;
	nd[2] = height;
	nd[3] = null;
	objInstance1[3] = nd;
	return vm[14];
};
