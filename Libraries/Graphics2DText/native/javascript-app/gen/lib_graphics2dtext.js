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

var lib_graphics2dtext_createNativeFont = function(vm, args) {
	var ints = vm[13][9];
	var nf = args[0][1];
	var nfOut = nf[3];
	var fontType = args[1][1];
	var fontPath = "";
	if ((fontType == 0)) {
		fontType = args[2][1];
	} else {
		fontPath = args[2][1];
		if ((fontType == 1)) {
			var res = resource_manager_getResourceOfType(vm, fontPath, "TTF");
			if ((res[0] == 1)) {
				return ints[2];
			}
			var resList = res[1];
			if (!getItemFromList(resList, 0)[1]) {
				return ints[2];
			}
			fontPath = getItemFromList(resList, 1)[1];
		}
	}
	var fontClass = 0;
	var fontSize = args[3][1];
	var red = args[4][1];
	var green = args[5][1];
	var blue = args[6][1];
	var styleBitmask = args[7][1];
	var isBold = (styleBitmask & 1);
	var isItalic = (styleBitmask & 2);
	nfOut[0] = LIB$graphics2dtext$createNativeFont(fontType, fontClass, fontPath);
	if ((nfOut[0] == null)) {
		if ((fontType == 3)) {
			return ints[1];
		}
		return ints[2];
	}
	return ints[0];
};

var lib_graphics2dtext_getNativeFontUniqueKey = function(vm, args) {
	var list1 = args[7][1];
	var output = [];
	output.splice(0, 0, args[0], args[1]);
	var list2 = buildList(output)[1];
	list1[2] = list2[2];
	list1[1] = list2[1];
	return vm[14];
};

var lib_graphics2dtext_glGenerateAndLoadTexture = function(vm, args) {
	return vm[14];
};

var lib_graphics2dtext_glRenderCharTile = function(vm, args) {
	return vm[15];
};

var lib_graphics2dtext_glRenderTextSurface = function(vm, args) {
	return vm[14];
};

var lib_graphics2dtext_glSetNativeDataIntArray = function(vm, args) {
	return vm[14];
};

var lib_graphics2dtext_isDynamicFontLoaded = function(vm, args) {
	return buildBoolean(vm[13], LIB$graphics2dtext$isDynamicFontLoaded());
};

var lib_graphics2dtext_isGlRenderer = function(vm, args) {
	return vm[16];
};

var lib_graphics2dtext_isResourceAvailable = function(vm, args) {
	var path = args[0][1];
	var res = resource_manager_getResourceOfType(vm, path, "TTF");
	if ((res[0] == 1)) {
		return vm[16];
	}
	var resList = res[1];
	if (!getItemFromList(resList, 0)[1]) {
		return vm[16];
	}
	return vm[15];
};

var lib_graphics2dtext_isSystemFontPresent = function(vm, args) {
	return buildBoolean(vm[13], LIB$graphics2dtext$isSystemFontAvailable(args[0][1]));
};

var lib_graphics2dtext_renderText = function(vm, args) {
	var sizeOut = args[0][1];
	var textSurface = args[1][1];
	var imageOut = textSurface[3];
	var nativeFont = (args[2][1])[3][0];
	var sourceType = args[3][1];
	var fontClass = 0;
	var fontPath = "";
	if ((sourceType == 0)) {
		fontClass = args[4][1];
	} else {
		fontPath = args[4][1];
	}
	var fontSize = args[5][1];
	var fontStyle = args[6][1];
	var isBold = (fontStyle & 1);
	var isItalic = (fontStyle & 2);
	var red = args[7][1];
	var green = args[8][1];
	var blue = args[9][1];
	var text = args[10][1];
	var bmp = LIB$graphics2dtext$renderText(PST$intBuffer16, nativeFont, fontSize, (isBold > 0), (isItalic > 0), red, green, blue, text);
	var spoofedNativeData = PST$createNewArray(4);
	spoofedNativeData[3] = bmp;
	var spoofedNativeData2 = PST$createNewArray(1);
	spoofedNativeData2[0] = spoofedNativeData;
	imageOut[0] = spoofedNativeData2;
	setItemInList(sizeOut, 0, buildInteger(vm[13], PST$intBuffer16[0]));
	setItemInList(sizeOut, 1, buildInteger(vm[13], PST$intBuffer16[1]));
	return vm[14];
};

var lib_graphics2dtext_simpleBlit = function(vm, args) {
	var nativeBlittableBitmap = (args[0][1])[3][0];
	var drawQueueNativeData = (args[1][1])[3];
	var alpha = args[4][1];
	var eventQueue = drawQueueNativeData[0];
	var index = (drawQueueNativeData[1] - 16);
	var imageQueue = drawQueueNativeData[2];
	var imageQueueLength = drawQueueNativeData[3];
	eventQueue[index] = 6;
	eventQueue[(index | 1)] = 0;
	eventQueue[(index | 8)] = args[2][1];
	eventQueue[(index | 9)] = args[3][1];
	if ((imageQueue.length == imageQueueLength)) {
		var oldSize = imageQueue.length;
		var newSize = (oldSize * 2);
		var newImageQueue = PST$createNewArray(newSize);
		var i = 0;
		while ((i < oldSize)) {
			newImageQueue[i] = imageQueue[i];
			i += 1;
		}
		imageQueue = newImageQueue;
		drawQueueNativeData[2] = imageQueue;
	}
	imageQueue[imageQueueLength] = nativeBlittableBitmap;
	drawQueueNativeData[3] = (imageQueueLength + 1);
	return vm[14];
};
