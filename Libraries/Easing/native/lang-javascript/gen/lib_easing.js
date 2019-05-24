PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

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
