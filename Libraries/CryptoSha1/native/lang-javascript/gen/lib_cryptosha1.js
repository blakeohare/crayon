PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_cryptosha1_bitShiftRight = function(value, amount) {
	if ((amount == 0)) {
		return value;
	}
	var mask = 2147483647;
	value = (value & lib_cryptosha1_uint32Hack(65535, 65535));
	if ((value > 0)) {
		return (value >> amount);
	}
	return (((value >> amount)) & ((mask >> ((amount - 1)))));
};

var lib_cryptosha1_bitwiseNot = function(x) {
	return (-x - 1);
};

var lib_cryptosha1_createWordsForBlock = function(startIndex, byteList, mWords) {
	var i = 0;
	while ((i < 64)) {
		mWords[(i >> 2)] = (((byteList[(startIndex + i)] << 24)) | ((byteList[(startIndex + i + 1)] << 16)) | ((byteList[(startIndex + i + 2)] << 8)) | (byteList[(startIndex + i + 3)]));
		i += 4;
	}
	return 0;
};

var lib_cryptosha1_digest = function(vm, args) {
	var obj = args[0][1];
	var output = args[1][1];
	var byteList = obj[3][0];
	var resultBytes = lib_cryptosha1_digestImpl(byteList);
	var i = 0;
	while ((i < 20)) {
		var b = resultBytes[i];
		addToList(output, vm[13][9][b]);
		i += 1;
	}
	return args[1];
};

var lib_cryptosha1_digestImpl = function(inputBytes) {
	var originalLength = (inputBytes.length * 8);
	var h0 = lib_cryptosha1_uint32Hack(26437, 8961);
	var h1 = lib_cryptosha1_uint32Hack(61389, 43913);
	var h2 = lib_cryptosha1_uint32Hack(39098, 56574);
	var h3 = lib_cryptosha1_uint32Hack(4146, 21622);
	var h4 = lib_cryptosha1_uint32Hack(50130, 57840);
	inputBytes.push(128);
	while (((inputBytes.length % 64) != 56)) {
		inputBytes.push(0);
	}
	inputBytes.push(0);
	inputBytes.push(0);
	inputBytes.push(0);
	inputBytes.push(0);
	inputBytes.push(((originalLength >> 24) & 255));
	inputBytes.push(((originalLength >> 16) & 255));
	inputBytes.push(((originalLength >> 8) & 255));
	inputBytes.push(((originalLength >> 0) & 255));
	var mWords = PST$createNewArray(80);
	var mask32 = lib_cryptosha1_uint32Hack(65535, 65535);
	var f = 0;
	var temp = 0;
	var k = 0;
	var kValues = PST$createNewArray(4);
	kValues[0] = lib_cryptosha1_uint32Hack(23170, 31129);
	kValues[1] = lib_cryptosha1_uint32Hack(28377, 60321);
	kValues[2] = lib_cryptosha1_uint32Hack(36635, 48348);
	kValues[3] = lib_cryptosha1_uint32Hack(51810, 49622);
	var chunkIndex = 0;
	while ((chunkIndex < inputBytes.length)) {
		lib_cryptosha1_createWordsForBlock(chunkIndex, inputBytes, mWords);
		var i = 16;
		while ((i < 80)) {
			mWords[i] = lib_cryptosha1_leftRotate((mWords[(i - 3)] ^ mWords[(i - 8)] ^ mWords[(i - 14)] ^ mWords[(i - 16)]), 1);
			i += 1;
		}
		var a = h0;
		var b = h1;
		var c = h2;
		var d = h3;
		var e = h4;
		var j = 0;
		while ((j < 80)) {
			if ((j < 20)) {
				f = (((b & c)) | ((lib_cryptosha1_bitwiseNot(b) & d)));
				k = kValues[0];
			} else {
				if ((j < 40)) {
					f = (b ^ c ^ d);
					k = kValues[1];
				} else {
					if ((j < 60)) {
						f = (((b & c)) | ((b & d)) | ((c & d)));
						k = kValues[2];
					} else {
						f = (b ^ c ^ d);
						k = kValues[3];
					}
				}
			}
			temp = (lib_cryptosha1_leftRotate(a, 5) + f + e + k + mWords[j]);
			e = d;
			d = c;
			c = lib_cryptosha1_leftRotate(b, 30);
			b = a;
			a = (temp & mask32);
			j += 1;
		}
		h0 = (((h0 + a)) & mask32);
		h1 = (((h1 + b)) & mask32);
		h2 = (((h2 + c)) & mask32);
		h3 = (((h3 + d)) & mask32);
		h4 = (((h4 + e)) & mask32);
		chunkIndex += 64;
	}
	var output = PST$createNewArray(20);
	output[0] = ((h0 >> 24) & 255);
	output[1] = ((h0 >> 16) & 255);
	output[2] = ((h0 >> 8) & 255);
	output[3] = (h0 & 255);
	output[4] = ((h1 >> 24) & 255);
	output[5] = ((h1 >> 16) & 255);
	output[6] = ((h1 >> 8) & 255);
	output[7] = (h1 & 255);
	output[8] = ((h2 >> 24) & 255);
	output[9] = ((h2 >> 16) & 255);
	output[10] = ((h2 >> 8) & 255);
	output[11] = (h2 & 255);
	output[12] = ((h3 >> 24) & 255);
	output[13] = ((h3 >> 16) & 255);
	output[14] = ((h3 >> 8) & 255);
	output[15] = (h3 & 255);
	output[16] = ((h4 >> 24) & 255);
	output[17] = ((h4 >> 16) & 255);
	output[18] = ((h4 >> 8) & 255);
	output[19] = (h4 & 255);
	return output;
};

var lib_cryptosha1_leftRotate = function(value, amt) {
	if ((amt == 0)) {
		return value;
	}
	var a = (value << amt);
	var b = lib_cryptosha1_bitShiftRight(value, (32 - amt));
	var result = (a | b);
	return result;
};

var lib_cryptosha1_uint32Hack = function(left, right) {
	return (((left << 16)) | right);
};
