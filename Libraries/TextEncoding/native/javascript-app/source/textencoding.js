
LIB$textencoding$bytesToText = function(bytes, format, strOut) {
	var i;
	var length = bytes.length;
	var sb = [];
	var c;
	var cp;
	var t;
	switch (format) {
		case 1:
		case 2:
			for (i = 0; i < length; ++i) {
				String.fromCharCode(bytes[i]);
			}
			break;
		case 3:
			i = 0;
			while (i < length) {
				c = bytes[i];
				if ((c & 0x80) == 0) {
					cp = c;
					i++;
				} else if ((c & 0xE0) == 0xC0) {
					if (i + 1 >= length) return 1;
					cp = c & 0x1FF;
					c = bytes[i + 1];
					if ((c & 0xC0) != 0x80) return 1;
					cp = (cp << 6) | (c & 0x3F);
					i += 2;
				} else if ((c & 0xF0) == 0xE0) {
					if (i + 2 >= length) return 1;
					cp = c & 0x0F;
					c = bytes[i + 1];
					if ((c & 0xC0) != 0x80) return 1;
					cp = (cp << 6) | (c & 0x3F);
					c = bytes[i + 2];
					if ((c & 0xC0) != 0x80) return 1;
					cp = (cp << 6) | (c & 0x3F);
					i += 3;
				} else if ((c & 0xF8) == 0xF0) {
					if (i + 3 >= length) return 1;
					cp = c & 0x07;
					c = bytes[i + 1];
					if ((c & 0xC0) != 0x80) return 1;
					cp = (cp << 6) | (c & 0x3F);
					c = bytes[i + 2];
					if ((c & 0xC0) != 0x80) return 1;
					cp = (cp << 6) | (c & 0x3F);
					c = bytes[i + 3];
					if ((c & 0xC0) != 0x80) return 1;
					cp = (cp << 6) | (c & 0x3F);
					i += 4;
				}
				sb.push(String.fromCharCode(c));
			}
			break;
		
		case 4:
		case 5:
			var a;
			var b;
			var charPair = [0, 0];
			var be = format == 5;
			for (i = 0; i < length; i += 2) {
				if (be) {
					b = bytes[i];
					a = bytes[i + 1];
				} else {
					a = bytes[i];
					b = bytes[i + 1];
				}
				
			}
			
	}
	
	strOut = sb.join('');
	return 0;
};

LIB$textencoding$textToBytes = function(value, includeBom, format, byteList, positiveIntegers, intOut) {
	var codePoints = [];
	var length = value.length;
	var maxValue = 0;
	var n = 0;
	var i;
	for (i = 0; i < length; ++i) {
		n = value.charCodeAt(i);
		if (n > maxValue) maxValue = n;
		codePoints.push(n);
	}
	if (maxValue > 127 && format == 1) return 1;

	// TODO: this is slightly wrong. Unicode characters should be converted into extended ASCII chars, if possible.
	if (maxValue > 255 && format == 2) return 1;

	var bytes;
	if (format <= 2) {
		bytes = codePoints;
	} else {
		bytes = [];
		if (includeBom) {
			switch (format) {
				case 3: bytes = [239, 187, 191]; break;
				case 4: bytes = [255, 254]; break;
				case 5: bytes = [254, 255]; break;
				case 6: bytes = [255, 254, 0, 0]; break;
				case 7: bytes = [0, 0, 254, 255]; break;
			}
		}
		length = codePoints.length;
		for (i = 0; i < length; ++i) {
			
		}
	}
	
	for (i = 0; i < bytes.length; ++i) {
		byteList.push(positiveIntegers[bytes[i]]);
	}
	
	return 0;
};

LIB$textencoding$codePointsToUtf8 = function(points, buffer) {
	var length = points.length;
	var p;
	for (var pIndex = 0; pIndex < length; ++i) {
		p = points[pIndex];
		if (p < 0x80) buffer.push(p);
		else if (p < 0x0800) {
			buffer.push(0xC0 | ((p >> 6) & 0x1F));
			buffer.push(0x80 | (p & 0x3F));
		} else if (p < 0x10000) {
			buffer.push(0xE0 | ((p >> 12) & 0x0F));
			buffer.push(0x80 | ((p >> 6) & 0x3F));
			buffer.push(0x80 | (p & 0x3F));
		} else {
			buffer.push(0xF0 | ((p >> 18) & 3));
			buffer.push(0x80 | ((p >> 12) & 0x3F));
			buffer.push(0x80 | ((p >> 6) & 0x3F));
			buffer.push(0x80 | (p & 0x3F));
		}
	}
};


LIB$textencoding$codePointsToUtf16 = function(points, buffer, isBigEndian) {
	var length = points.length;
	var p;
	var a;
	var b;
	
	for (var pIndex = 0; pIndex < length; ++i) {
		p = points[pIndex];
		if (p < 0x10000) {
			if (isBigEndian) {
				a = (p >> 8) & 255;
				b = p & 255;
			} else {
				b = (p >> 8) & 255;
				a = p & 255;
			}
			buffer.push(a);
			buffer.push(b);
		} else {
			p -= 0x10000;
			a = 0xD800 | ((p >> 10) & 0x03FF);
			b = 0xDC00 | (p & 0x03FF);
			if (isBigEndian) {
				buffer.push((a >> 8) & 255);
				buffer.push(a & 255);
				buffer.push((b >> 8) & 255);
				buffer.push(b & 255);
			} else {
				buffer.push(a & 255);
				buffer.push((a >> 8) & 255);
				buffer.push(b & 255);
				buffer.push((b >> 8) & 255);
			}
		}
	}
};
