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

PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$intBuffer16 = PST$multiplyList([0], 16);

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

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
