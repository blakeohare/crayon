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

var lib_easing_apply_pts = function(vm, args) {
	var sampleValues = args[1][1];
	var _len = sampleValues[1];
	var samples = PST$createNewArray(_len);
	var i = 0;
	while ((i < _len)) {
		samples[i] = sampleValues[2][i][1];
		i += 1;
	}
	samples[0] = 0.0;
	samples[(_len - 1)] = 1.0;
	var o = args[0][1];
	o[4] = [_len, samples];
	return vm[13][0];
};

var lib_easing_interpolate = function(vm, args) {
	var arg2 = args[1];
	var arg3 = args[2];
	var arg4 = args[3];
	var arg5 = args[4];
	var arg6 = args[5];
	var o = args[0][1];
	var es = o[4];
	var samples = es[1];
	var _len = es[0];
	var int1 = args[6][1];
	var float1 = 0.0;
	var float2 = 0.0;
	var float3 = 0.0;
	if ((arg4[0] == 3)) {
		float1 = (0.0 + arg4[1]);
	} else {
		if ((arg4[0] == 4)) {
			float1 = arg4[1];
		} else {
			return vm[13][0];
		}
	}
	if ((arg5[0] == 3)) {
		float2 = (0.0 + arg5[1]);
	} else {
		if ((arg5[0] == 4)) {
			float2 = arg5[1];
		} else {
			return vm[13][0];
		}
	}
	var bool1 = false;
	var bool2 = false;
	var first = false;
	if ((int1 == 2)) {
		first = true;
		if (((float1 * 2.0) > float2)) {
			float1 = ((float2 - float1) * 2);
			bool1 = true;
			bool2 = true;
		} else {
			float1 *= 2.0;
		}
	} else {
		if ((int1 == 1)) {
			float1 = (float2 - float1);
			bool1 = true;
		}
	}
	if ((float2 == 0)) {
		float1 = samples[0];
	} else {
		if ((float2 < 0)) {
			float2 = -float2;
			float1 = -float1;
		}
		if ((float1 >= float2)) {
			float1 = samples[(_len - 1)];
		} else {
			if ((float1 < 0)) {
				float1 = samples[0];
			} else {
				float1 = (float1 / float2);
				if ((_len > 2)) {
					float2 = (float1 * _len);
					var index = Math.floor(float2);
					float2 -= index;
					float1 = samples[index];
					if (((index < (_len - 1)) && (float2 > 0))) {
						float3 = samples[(index + 1)];
						float1 = ((float1 * (1 - float2)) + (float3 * float2));
					}
				}
			}
		}
	}
	if ((arg2[0] == 3)) {
		float2 = (0.0 + arg2[1]);
	} else {
		if ((arg2[0] == 4)) {
			float2 = arg2[1];
		} else {
			return vm[13][0];
		}
	}
	if ((arg3[0] == 3)) {
		float3 = (0.0 + arg3[1]);
	} else {
		if ((arg3[0] == 4)) {
			float3 = arg3[1];
		} else {
			return vm[13][0];
		}
	}
	if (bool1) {
		float1 = (1.0 - float1);
	}
	if (first) {
		float1 *= 0.5;
	}
	if (bool2) {
		float1 += 0.5;
	}
	float1 = ((float1 * float3) + ((1 - float1) * float2));
	if (((arg6[0] == 2) && arg6[1])) {
		return buildInteger(vm[13], Math.floor((float1 + 0.5)));
	}
	return buildFloat(vm[13], float1);
};
