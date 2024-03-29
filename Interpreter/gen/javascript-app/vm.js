PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$clearList = function(v) {
	v.length = 0;
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

PST$stringBuffer16 = PST$multiplyList([''], 16);

PST$intBuffer16 = PST$multiplyList([0], 16);

PST$dictionaryKeys = function(d) {
	var o = [];
	for (var k in d) {
		o.push(k);
	}
	return o;
};

PST$stringEndsWith = function(s, v) {
	return s.indexOf(v, s.length - v.length) !== -1;
};

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

PST$is_valid_integer = function(n) {
	var t = parseInt(n);
	return t < 0 || t >= 0;
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

PST$floatBuffer16 = PST$multiplyList([0.0], 16);

PST$stdout = v => { console.log(v); };

PST$sortedCopyOfArray = function(n) {
	var a = n.concat([]);
	a.sort();
	return a;
};

PST$checksubstring = function(s, index, lookfor) { return s.substring(index, index + lookfor.length) === lookfor; };

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var addLiteralImpl = function(vm, row, stringArg) {
	var g = vm[13];
	var type = row[0];
	if ((type == 1)) {
		vm[4][4].push(g[0]);
	} else if ((type == 2)) {
		vm[4][4].push(buildBoolean(g, (row[1] == 1)));
	} else if ((type == 3)) {
		vm[4][4].push(buildInteger(g, row[1]));
	} else if ((type == 4)) {
		vm[4][4].push(buildFloat(g, parseFloat(stringArg)));
	} else if ((type == 5)) {
		vm[4][4].push(buildCommonString(g, stringArg));
	} else if ((type == 9)) {
		var index = vm[4][4].length;
		vm[4][4].push(buildCommonString(g, stringArg));
		vm[4][19][stringArg] = index;
	} else if ((type == 10)) {
		var cv = [false, row[1]];
		vm[4][4].push([10, cv]);
	}
	return 0;
};

var addNameImpl = function(vm, nameValue) {
	var index = vm[4][1].length;
	vm[4][2][nameValue] = index;
	vm[4][1].push(nameValue);
	if ("length" == nameValue) {
		vm[4][13] = index;
	}
	return 0;
};

var addToList = function(list, item) {
	list[2].push(item);
	list[1] += 1;
};

var applyDebugSymbolData = function(vm, opArgs, stringData, recentlyDefinedFunction) {
	return 0;
};

var Base64Helper_bytesToB64 = function(userByteList, output, stringsByInt) {
	if ((userByteList[0] != 6)) {
		return false;
	}
	var bytes = listImplToBytes(userByteList[1]);
	if ((bytes == null)) {
		return false;
	}
	var byteCount = bytes.length;
	var pairCount = (byteCount * 4);
	while (((pairCount % 3) != 0)) {
		pairCount += 1;
	}
	var pairs = PST$createNewArray(pairCount);
	var j = 0;
	var i = 0;
	i = 0;
	while ((i < byteCount)) {
		pairs[j] = (bytes[i] >> 6);
		pairs[(j + 1)] = ((bytes[i] >> 4) & 3);
		pairs[(j + 2)] = ((bytes[i] >> 2) & 3);
		pairs[(j + 3)] = (bytes[i] & 3);
		j += 4;
		i += 1;
	}
	while ((j < pairCount)) {
		pairs[j] = 0;
		j += 1;
	}
	var strLen = Math.floor(pairCount / 3);
	var lookupList = stringsByInt[2];
	var itemsList = output[2];
	i = 0;
	while ((i < pairCount)) {
		itemsList.push(lookupList[(((pairs[i] << 4)) + ((pairs[(i + 1)] << 2)) + pairs[(i + 2)])]);
		i += 3;
	}
	output[1] = strLen;
	return true;
};

var buildBoolean = function(g, value) {
	if (value) {
		return g[1];
	}
	return g[2];
};

var buildCommonString = function(g, s) {
	var value = null;
	value = g[11][s];
	if (value === undefined) value = null;
	if ((value == null)) {
		value = buildString(g, s);
		g[11][s] = value;
	}
	return value;
};

var buildFloat = function(g, value) {
	if ((value == 0.0)) {
		return g[6];
	}
	if ((value == 1.0)) {
		return g[7];
	}
	return [4, value];
};

var buildInteger = function(g, num) {
	if ((num < 0)) {
		if ((num > -257)) {
			return g[10][-num];
		}
	} else if ((num < 2049)) {
		return g[9][num];
	}
	return [3, num];
};

var buildList = function(valueList) {
	return buildListWithType(null, valueList);
};

var buildListWithType = function(type, valueList) {
	return [6, [type, valueList.length, valueList]];
};

var buildNull = function(globals) {
	return globals[0];
};

var buildRelayObj = function(type, iarg1, iarg2, iarg3, farg1, sarg1) {
	return [type, iarg1, iarg2, iarg3, farg1, sarg1];
};

var buildString = function(g, s) {
	if ((s.length == 0)) {
		return g[8];
	}
	return [5, s];
};

var buildStringDictionary = function(globals, stringKeys, values) {
	var size = stringKeys.length;
	var d = [size, 5, 0, null, {}, {}, [], []];
	var k = null;
	var i = 0;
	while ((i < size)) {
		k = stringKeys[i];
		if ((d[5][k] !== undefined)) {
			d[7][d[5][k]] = values[i];
		} else {
			d[5][k] = d[7].length;
			d[7].push(values[i]);
			d[6].push(buildString(globals, k));
		}
		i += 1;
	}
	d[0] = d[7].length;
	return [7, d];
};

var byteObjToList = function(nums, obj, emptyList) {
	var bytes = obj[3][0];
	var size = bytes.length;
	emptyList[1] = size;
	var list = emptyList[2];
	var i = 0;
	while ((i < size)) {
		list.push(nums[bytes[i]]);
		++i;
	}
	emptyList[2] = list;
	return 0;
};

var bytesToListValue = function(globals, bytes) {
	var b = [];
	var length = bytes.length;
	var nums = globals[9];
	var i = 0;
	while ((i < length)) {
		b.push(nums[bytes[i]]);
		i += 1;
	}
	return buildList(b);
};

var canAssignGenericToGeneric = function(vm, gen1, gen1Index, gen2, gen2Index, newIndexOut) {
	if ((gen2 == null)) {
		return true;
	}
	if ((gen1 == null)) {
		return false;
	}
	var t1 = gen1[gen1Index];
	var t2 = gen2[gen2Index];
	switch (t1) {
		case 0:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return (t2 == t1);
		case 1:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return (t2 == t1);
		case 2:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return (t2 == t1);
		case 4:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return (t2 == t1);
		case 5:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return (t2 == t1);
		case 10:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return (t2 == t1);
		case 3:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			return ((t2 == 3) || (t2 == 4));
		case 8:
			newIndexOut[0] = (gen1Index + 1);
			newIndexOut[1] = (gen2Index + 2);
			if ((t2 != 8)) {
				return false;
			}
			var c1 = gen1[(gen1Index + 1)];
			var c2 = gen2[(gen2Index + 1)];
			if ((c1 == c2)) {
				return true;
			}
			return isClassASubclassOf(vm, c1, c2);
		case 6:
			if ((t2 != 6)) {
				return false;
			}
			return canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut);
		case 7:
			if ((t2 != 7)) {
				return false;
			}
			if (!canAssignGenericToGeneric(vm, gen1, (gen1Index + 1), gen2, (gen2Index + 1), newIndexOut)) {
				return false;
			}
			return canAssignGenericToGeneric(vm, gen1, newIndexOut[0], gen2, newIndexOut[1], newIndexOut);
		case 9:
			if ((t2 != 9)) {
				return false;
			}
			return false;
		default:
			return false;
	}
};

var canAssignTypeToGeneric = function(vm, value, generics, genericIndex) {
	switch (value[0]) {
		case 1:
			switch (generics[genericIndex]) {
				case 5:
					return value;
				case 8:
					return value;
				case 10:
					return value;
				case 9:
					return value;
				case 6:
					return value;
				case 7:
					return value;
			}
			return null;
		case 2:
			if ((generics[genericIndex] == value[0])) {
				return value;
			}
			return null;
		case 5:
			if ((generics[genericIndex] == value[0])) {
				return value;
			}
			return null;
		case 10:
			if ((generics[genericIndex] == value[0])) {
				return value;
			}
			return null;
		case 3:
			if ((generics[genericIndex] == 3)) {
				return value;
			}
			if ((generics[genericIndex] == 4)) {
				return buildFloat(vm[13], (0.0 + value[1]));
			}
			return null;
		case 4:
			if ((generics[genericIndex] == 4)) {
				return value;
			}
			return null;
		case 6:
			var list = value[1];
			var listType = list[0];
			genericIndex += 1;
			if ((listType == null)) {
				if (((generics[genericIndex] == 1) || (generics[genericIndex] == 0))) {
					return value;
				}
				return null;
			}
			var i = 0;
			while ((i < listType.length)) {
				if ((listType[i] != generics[(genericIndex + i)])) {
					return null;
				}
				i += 1;
			}
			return value;
		case 7:
			var dict = value[1];
			var j = genericIndex;
			switch (dict[1]) {
				case 3:
					if ((generics[1] == dict[1])) {
						j += 2;
					} else {
						return null;
					}
					break;
				case 5:
					if ((generics[1] == dict[1])) {
						j += 2;
					} else {
						return null;
					}
					break;
				case 8:
					if ((generics[1] == 8)) {
						j += 3;
					} else {
						return null;
					}
					break;
			}
			var valueType = dict[3];
			if ((valueType == null)) {
				if (((generics[j] == 0) || (generics[j] == 1))) {
					return value;
				}
				return null;
			}
			var k = 0;
			while ((k < valueType.length)) {
				if ((valueType[k] != generics[(j + k)])) {
					return null;
				}
				k += 1;
			}
			return value;
		case 8:
			if ((generics[genericIndex] == 8)) {
				var targetClassId = generics[(genericIndex + 1)];
				var givenClassId = (value[1])[0];
				if ((targetClassId == givenClassId)) {
					return value;
				}
				if (isClassASubclassOf(vm, givenClassId, targetClassId)) {
					return value;
				}
			}
			return null;
	}
	return null;
};

var canonicalizeAngle = function(a) {
	var twopi = 6.28318530717958;
	a = (a % twopi);
	if ((a < 0)) {
		a += twopi;
	}
	return a;
};

var canonicalizeListSliceArgs = function(outParams, beginValue, endValue, beginIndex, endIndex, stepAmount, length, isForward) {
	if ((beginValue == null)) {
		if (isForward) {
			beginIndex = 0;
		} else {
			beginIndex = (length - 1);
		}
	}
	if ((endValue == null)) {
		if (isForward) {
			endIndex = length;
		} else {
			endIndex = (-1 - length);
		}
	}
	if ((beginIndex < 0)) {
		beginIndex += length;
	}
	if ((endIndex < 0)) {
		endIndex += length;
	}
	if (((beginIndex == 0) && (endIndex == length) && (stepAmount == 1))) {
		return 2;
	}
	if (isForward) {
		if ((beginIndex >= length)) {
			return 0;
		}
		if ((beginIndex < 0)) {
			return 3;
		}
		if ((endIndex < beginIndex)) {
			return 4;
		}
		if ((beginIndex == endIndex)) {
			return 0;
		}
		if ((endIndex > length)) {
			endIndex = length;
		}
	} else {
		if ((beginIndex < 0)) {
			return 0;
		}
		if ((beginIndex >= length)) {
			return 3;
		}
		if ((endIndex > beginIndex)) {
			return 4;
		}
		if ((beginIndex == endIndex)) {
			return 0;
		}
		if ((endIndex < -1)) {
			endIndex = -1;
		}
	}
	outParams[0] = beginIndex;
	outParams[1] = endIndex;
	return 1;
};

var classIdToString = function(vm, classId) {
	return vm[4][9][classId][16];
};

var clearList = function(a) {
	PST$clearList(a[2]);
	a[1] = 0;
	return 0;
};

var cloneDictionary = function(original, clone) {
	var type = original[1];
	var i = 0;
	var size = original[0];
	var kInt = 0;
	var kString = null;
	if ((clone == null)) {
		clone = [0, type, original[2], original[3], {}, {}, [], []];
		if ((type == 5)) {
			while ((i < size)) {
				clone[5][original[6][i][1]] = i;
				i += 1;
			}
		} else {
			while ((i < size)) {
				if ((type == 8)) {
					kInt = (original[6][i][1])[1];
				} else {
					kInt = original[6][i][1];
				}
				clone[4][kInt] = i;
				i += 1;
			}
		}
		i = 0;
		while ((i < size)) {
			clone[6].push(original[6][i]);
			clone[7].push(original[7][i]);
			i += 1;
		}
	} else {
		i = 0;
		while ((i < size)) {
			if ((type == 5)) {
				kString = original[6][i][1];
				if ((clone[5][kString] !== undefined)) {
					clone[7][clone[5][kString]] = original[7][i];
				} else {
					clone[5][kString] = clone[7].length;
					clone[7].push(original[7][i]);
					clone[6].push(original[6][i]);
				}
			} else {
				if ((type == 3)) {
					kInt = original[6][i][1];
				} else {
					kInt = (original[6][i][1])[1];
				}
				if ((clone[4][kInt] !== undefined)) {
					clone[7][clone[4][kInt]] = original[7][i];
				} else {
					clone[4][kInt] = clone[7].length;
					clone[7].push(original[7][i]);
					clone[6].push(original[6][i]);
				}
			}
			i += 1;
		}
	}
	clone[0] = (Object.keys(clone[4]).length + Object.keys(clone[5]).length);
	return clone;
};

var createInstanceType = function(classId) {
	var o = PST$createNewArray(2);
	o[0] = 8;
	o[1] = classId;
	return o;
};

var createVm = function(rawByteCode, resourceManifest, imageAtlasManifest) {
	var globals = initializeConstantValues();
	var resources = resourceManagerInitialize(globals, resourceManifest, imageAtlasManifest);
	var byteCode = initializeByteCode(rawByteCode);
	var localsStack = PST$createNewArray(10);
	var localsStackSet = PST$createNewArray(10);
	var i = 0;
	i = (localsStack.length - 1);
	while ((i >= 0)) {
		localsStack[i] = null;
		localsStackSet[i] = 0;
		i -= 1;
	}
	var stack = [0, 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null];
	var executionContext = [0, stack, 0, 100, PST$createNewArray(100), localsStack, localsStackSet, 1, 0, false, null, false, 0, null];
	var executionContexts = {};
	executionContexts[0] = executionContext;
	var vm = [executionContexts, executionContext[0], byteCode, [PST$createNewArray(byteCode[0].length), null, [], null, null, {}, {}], [null, [], {}, null, [], null, [], null, [], PST$createNewArray(100), PST$createNewArray(100), {}, null, -1, PST$createNewArray(10), 0, null, null, [0, 0, 0], {}, {}, null], 0, false, [], null, resources, [], [PST$createNewArray(0), false, null, null, null, null, null, null, null], [[], {}], globals, globals[0], globals[1], globals[2], 0];
	return vm;
};

var crypto_bitShiftRight = function(value, amount) {
	if ((amount == 0)) {
		return value;
	}
	value = (value & uint32Hack(65535, 65535));
	var mask = 2147483647;
	if ((value > 0)) {
		return (value >> amount);
	}
	return (((value >> amount)) & ((mask >> ((amount - 1)))));
};

var crypto_bitwiseNot = function(x) {
	return (-x - 1);
};

var crypto_digest = function(globals, bytes, algo) {
	var byteArray = listImplToBytes(bytes);
	var byteList = [];
	var i = 0;
	while ((i < byteArray.length)) {
		byteList.push(byteArray[i]);
		i += 1;
	}
	if ((algo == 1)) {
		return crypto_md5_digest(globals, byteList);
	}
	if ((algo == 2)) {
		return crypto_sha1_digest(globals, byteList);
	}
	return globals[0];
};

var crypto_leftRotate = function(value, amt) {
	if ((amt == 0)) {
		return value;
	}
	var a = (value << amt);
	var b = crypto_bitShiftRight(value, (32 - amt));
	var result = (a | b);
	return result;
};

var crypto_md5_createWordsForBlock = function(startIndex, byteList, mWords) {
	var i = 0;
	while ((i < 64)) {
		mWords[(i >> 2)] = ((byteList[(startIndex + i)]) | ((byteList[(startIndex + i + 1)] << 8)) | ((byteList[(startIndex + i + 2)] << 16)) | ((byteList[(startIndex + i + 3)] << 24)));
		i += 4;
	}
	return 0;
};

var crypto_md5_digest = function(globals, inputBytes) {
	var originalLength = (inputBytes.length * 8);
	var shiftTable = PST$createNewArray(64);
	var K = PST$createNewArray(64);
	var i = 0;
	while ((i < 16)) {
		shiftTable[i] = 7;
		shiftTable[(i + 1)] = 12;
		shiftTable[(i + 2)] = 17;
		shiftTable[(i + 3)] = 22;
		shiftTable[(i + 16)] = 5;
		shiftTable[(i + 17)] = 9;
		shiftTable[(i + 18)] = 14;
		shiftTable[(i + 19)] = 20;
		shiftTable[(i + 32)] = 4;
		shiftTable[(i + 33)] = 11;
		shiftTable[(i + 34)] = 16;
		shiftTable[(i + 35)] = 23;
		shiftTable[(i + 48)] = 6;
		shiftTable[(i + 49)] = 10;
		shiftTable[(i + 50)] = 15;
		shiftTable[(i + 51)] = 21;
		i += 4;
	}
	K[0] = uint32Hack(55146, 42104);
	K[1] = uint32Hack(59591, 46934);
	K[2] = uint32Hack(9248, 28891);
	K[3] = uint32Hack(49597, 52974);
	K[4] = uint32Hack(62844, 4015);
	K[5] = uint32Hack(18311, 50730);
	K[6] = uint32Hack(43056, 17939);
	K[7] = uint32Hack(64838, 38145);
	K[8] = uint32Hack(27008, 39128);
	K[9] = uint32Hack(35652, 63407);
	K[10] = uint32Hack(65535, 23473);
	K[11] = uint32Hack(35164, 55230);
	K[12] = uint32Hack(27536, 4386);
	K[13] = uint32Hack(64920, 29075);
	K[14] = uint32Hack(42617, 17294);
	K[15] = uint32Hack(18868, 2081);
	K[16] = uint32Hack(63006, 9570);
	K[17] = uint32Hack(49216, 45888);
	K[18] = uint32Hack(9822, 23121);
	K[19] = uint32Hack(59830, 51114);
	K[20] = uint32Hack(54831, 4189);
	K[21] = uint32Hack(580, 5203);
	K[22] = uint32Hack(55457, 59009);
	K[23] = uint32Hack(59347, 64456);
	K[24] = uint32Hack(8673, 52710);
	K[25] = uint32Hack(49975, 2006);
	K[26] = uint32Hack(62677, 3463);
	K[27] = uint32Hack(17754, 5357);
	K[28] = uint32Hack(43491, 59653);
	K[29] = uint32Hack(64751, 41976);
	K[30] = uint32Hack(26479, 729);
	K[31] = uint32Hack(36138, 19594);
	K[32] = uint32Hack(65530, 14658);
	K[33] = uint32Hack(34673, 63105);
	K[34] = uint32Hack(28061, 24866);
	K[35] = uint32Hack(64997, 14348);
	K[36] = uint32Hack(42174, 59972);
	K[37] = uint32Hack(19422, 53161);
	K[38] = uint32Hack(63163, 19296);
	K[39] = uint32Hack(48831, 48240);
	K[40] = uint32Hack(10395, 32454);
	K[41] = uint32Hack(60065, 10234);
	K[42] = uint32Hack(54511, 12421);
	K[43] = uint32Hack(1160, 7429);
	K[44] = uint32Hack(55764, 53305);
	K[45] = uint32Hack(59099, 39397);
	K[46] = uint32Hack(8098, 31992);
	K[47] = uint32Hack(50348, 22117);
	K[48] = uint32Hack(62505, 8772);
	K[49] = uint32Hack(17194, 65431);
	K[50] = uint32Hack(43924, 9127);
	K[51] = uint32Hack(64659, 41017);
	K[52] = uint32Hack(25947, 22979);
	K[53] = uint32Hack(36620, 52370);
	K[54] = uint32Hack(65519, 62589);
	K[55] = uint32Hack(34180, 24017);
	K[56] = uint32Hack(28584, 32335);
	K[57] = uint32Hack(65068, 59104);
	K[58] = uint32Hack(41729, 17172);
	K[59] = uint32Hack(19976, 4513);
	K[60] = uint32Hack(63315, 32386);
	K[61] = uint32Hack(48442, 62005);
	K[62] = uint32Hack(10967, 53947);
	K[63] = uint32Hack(60294, 54161);
	var A = uint32Hack(26437, 8961);
	var B = uint32Hack(61389, 43913);
	var C = uint32Hack(39098, 56574);
	var D = uint32Hack(4146, 21622);
	inputBytes.push(128);
	while (((inputBytes.length % 64) != 56)) {
		inputBytes.push(0);
	}
	inputBytes.push(((originalLength >> 0) & 255));
	inputBytes.push(((originalLength >> 8) & 255));
	inputBytes.push(((originalLength >> 16) & 255));
	inputBytes.push(((originalLength >> 24) & 255));
	inputBytes.push(0);
	inputBytes.push(0);
	inputBytes.push(0);
	inputBytes.push(0);
	var mWords = PST$createNewArray(16);
	var mask32 = uint32Hack(65535, 65535);
	var chunkIndex = 0;
	while ((chunkIndex < inputBytes.length)) {
		crypto_md5_createWordsForBlock(chunkIndex, inputBytes, mWords);
		var A_init = A;
		var B_init = B;
		var C_init = C;
		var D_init = D;
		var j = 0;
		while ((j < 64)) {
			A = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, A, B, C, D, j);
			D = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, D, A, B, C, (j | 1));
			C = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, C, D, A, B, (j | 2));
			B = crypto_md5_magicShuffle(mWords, K, shiftTable, mask32, B, C, D, A, (j | 3));
			j += 4;
		}
		A = (((A_init + A)) & mask32);
		B = (((B_init + B)) & mask32);
		C = (((C_init + C)) & mask32);
		D = (((D_init + D)) & mask32);
		chunkIndex += 64;
	}
	var output = [];
	output.push(buildInteger(globals, (A & 255)));
	output.push(buildInteger(globals, ((A >> 8) & 255)));
	output.push(buildInteger(globals, ((A >> 16) & 255)));
	output.push(buildInteger(globals, ((A >> 24) & 255)));
	output.push(buildInteger(globals, (B & 255)));
	output.push(buildInteger(globals, ((B >> 8) & 255)));
	output.push(buildInteger(globals, ((B >> 16) & 255)));
	output.push(buildInteger(globals, ((B >> 24) & 255)));
	output.push(buildInteger(globals, (C & 255)));
	output.push(buildInteger(globals, ((C >> 8) & 255)));
	output.push(buildInteger(globals, ((C >> 16) & 255)));
	output.push(buildInteger(globals, ((C >> 24) & 255)));
	output.push(buildInteger(globals, (D & 255)));
	output.push(buildInteger(globals, ((D >> 8) & 255)));
	output.push(buildInteger(globals, ((D >> 16) & 255)));
	output.push(buildInteger(globals, ((D >> 24) & 255)));
	return buildList(output);
};

var crypto_md5_magicShuffle = function(mWords, sineValues, shiftValues, mask32, a, b, c, d, counter) {
	var roundNumber = (counter >> 4);
	var t = 0;
	var shiftAmount = shiftValues[counter];
	var sineValue = sineValues[counter];
	var mWord = 0;
	if ((roundNumber == 0)) {
		t = (((b & c)) | ((crypto_bitwiseNot(b) & d)));
		mWord = mWords[counter];
	} else if ((roundNumber == 1)) {
		t = (((b & d)) | ((c & crypto_bitwiseNot(d))));
		mWord = mWords[((((5 * counter) + 1)) & 15)];
	} else if ((roundNumber == 2)) {
		t = (b ^ c ^ d);
		mWord = mWords[((((3 * counter) + 5)) & 15)];
	} else {
		t = (c ^ ((b | crypto_bitwiseNot(d))));
		mWord = mWords[(((7 * counter)) & 15)];
	}
	t = (((a + t + mWord + sineValue)) & mask32);
	t = (b + crypto_leftRotate(t, shiftAmount));
	return (t & mask32);
};

var crypto_sha1_createWordsForBlock = function(startIndex, byteList, mWords) {
	var i = 0;
	while ((i < 64)) {
		mWords[(i >> 2)] = (((byteList[(startIndex + i)] << 24)) | ((byteList[(startIndex + i + 1)] << 16)) | ((byteList[(startIndex + i + 2)] << 8)) | (byteList[(startIndex + i + 3)]));
		i += 4;
	}
	return 0;
};

var crypto_sha1_digest = function(globals, inputBytes) {
	var originalLength = (inputBytes.length * 8);
	var h0 = uint32Hack(26437, 8961);
	var h1 = uint32Hack(61389, 43913);
	var h2 = uint32Hack(39098, 56574);
	var h3 = uint32Hack(4146, 21622);
	var h4 = uint32Hack(50130, 57840);
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
	var mask32 = uint32Hack(65535, 65535);
	var f = 0;
	var temp = 0;
	var k = 0;
	var kValues = PST$createNewArray(4);
	kValues[0] = uint32Hack(23170, 31129);
	kValues[1] = uint32Hack(28377, 60321);
	kValues[2] = uint32Hack(36635, 48348);
	kValues[3] = uint32Hack(51810, 49622);
	var chunkIndex = 0;
	while ((chunkIndex < inputBytes.length)) {
		crypto_sha1_createWordsForBlock(chunkIndex, inputBytes, mWords);
		var i = 16;
		while ((i < 80)) {
			mWords[i] = crypto_leftRotate((mWords[(i - 3)] ^ mWords[(i - 8)] ^ mWords[(i - 14)] ^ mWords[(i - 16)]), 1);
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
				f = (((b & c)) | ((crypto_bitwiseNot(b) & d)));
				k = kValues[0];
			} else if ((j < 40)) {
				f = (b ^ c ^ d);
				k = kValues[1];
			} else if ((j < 60)) {
				f = (((b & c)) | ((b & d)) | ((c & d)));
				k = kValues[2];
			} else {
				f = (b ^ c ^ d);
				k = kValues[3];
			}
			temp = (crypto_leftRotate(a, 5) + f + e + k + mWords[j]);
			e = d;
			d = c;
			c = crypto_leftRotate(b, 30);
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
	var output = [];
	output.push(buildInteger(globals, ((h0 >> 24) & 255)));
	output.push(buildInteger(globals, ((h0 >> 16) & 255)));
	output.push(buildInteger(globals, ((h0 >> 8) & 255)));
	output.push(buildInteger(globals, (h0 & 255)));
	output.push(buildInteger(globals, ((h1 >> 24) & 255)));
	output.push(buildInteger(globals, ((h1 >> 16) & 255)));
	output.push(buildInteger(globals, ((h1 >> 8) & 255)));
	output.push(buildInteger(globals, (h1 & 255)));
	output.push(buildInteger(globals, ((h2 >> 24) & 255)));
	output.push(buildInteger(globals, ((h2 >> 16) & 255)));
	output.push(buildInteger(globals, ((h2 >> 8) & 255)));
	output.push(buildInteger(globals, (h2 & 255)));
	output.push(buildInteger(globals, ((h3 >> 24) & 255)));
	output.push(buildInteger(globals, ((h3 >> 16) & 255)));
	output.push(buildInteger(globals, ((h3 >> 8) & 255)));
	output.push(buildInteger(globals, (h3 & 255)));
	output.push(buildInteger(globals, ((h4 >> 24) & 255)));
	output.push(buildInteger(globals, ((h4 >> 16) & 255)));
	output.push(buildInteger(globals, ((h4 >> 8) & 255)));
	output.push(buildInteger(globals, (h4 & 255)));
	return buildList(output);
};

var DateTime_getNativeTimezone = function(value) {
	var tzObj = value[1];
	if ((tzObj[3] == null)) {
		return null;
	}
	return tzObj[3][0];
};

var DateTime_getUtcOffsetAt = function(vm, arg1, arg2) {
	var nativeTz = DateTime_getNativeTimezone(arg1);
	var unixTime = arg2[1];
	var offsetSeconds = COMMON.dateTime.getUtcOffsetAt(nativeTz, unixTime);
	return buildInteger(vm[13], offsetSeconds);
};

var DateTime_initTimeZone = function(vm, arg1, arg2, arg3) {
	var timezone = arg1[1];
	timezone[3] = PST$createNewArray(1);
	var nativeTzRef = null;
	var readableName = null;
	var offsetFromUtc = 0;
	var isDstObserved = 0;
	var fingerprint = null;
	if ((arg2[0] == 1)) {
		var strOut = PST$stringBuffer16;
		var intOut = PST$intBuffer16;
		nativeTzRef = COMMON.dateTime.getDataForLocalTimeZone(strOut, intOut);
		readableName = strOut[0];
		fingerprint = strOut[1];
		offsetFromUtc = intOut[0];
		isDstObserved = intOut[1];
	} else {
		return vm[14];
	}
	timezone[3] = PST$createNewArray(5);
	timezone[3][0] = nativeTzRef;
	timezone[3][1] = readableName;
	timezone[3][2] = offsetFromUtc;
	timezone[3][3] = (isDstObserved == 1);
	timezone[3][4] = fingerprint;
	var values = [];
	values.push(buildString(vm[13], readableName));
	values.push(buildInteger(vm[13], offsetFromUtc));
	values.push(buildBoolean(vm[13], (isDstObserved == 1)));
	values.push(buildString(vm[13], fingerprint));
	return buildList(values);
};

var DateTime_initTimeZoneList = function(vm, arg1) {
	var obj = arg1[1];
	obj[3] = PST$createNewArray(1);
	var timezones = COMMON.dateTime.initializeTimeZoneList();
	obj[3][0] = timezones;
	var length = timezones.length;
	return buildInteger(vm[13], length);
};

var DateTime_isDstOccurringAt = function(vm, arg1, arg2) {
	var nativeTz = DateTime_getNativeTimezone(arg1);
	var unixtime = arg2[1];
	return buildBoolean(vm[13], COMMON.dateTime.isDstOccurringAt(nativeTz, unixtime));
};

var DateTime_parseDate = function(vm, arg1, arg2, arg3, arg4, arg5, arg6, arg7) {
	var year = arg1[1];
	var month = arg2[1];
	var day = arg3[1];
	var hour = arg4[1];
	var minute = arg5[1];
	var microseconds = arg6[1];
	var nullableTimeZone = DateTime_getNativeTimezone(arg7);
	if (((year >= 1970) && (year < 2100) && (month >= 1) && (month <= 12) && (day >= 1) && (day <= 31) && (hour >= 0) && (hour < 24) && (minute >= 0) && (minute < 60) && (microseconds >= 0) && (microseconds < 60000000))) {
		var intOut = PST$intBuffer16;
		COMMON.dateTime.parseDate(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds);
		if ((intOut[0] == 1)) {
			var unixFloat = (intOut[1] + (intOut[2] / 1000000.0));
			return buildFloat(vm[13], unixFloat);
		}
	}
	return vm[14];
};

var DateTime_unixToStructured = function(vm, arg1, arg2) {
	var unixTime = arg1[1];
	var nullableTimeZone = DateTime_getNativeTimezone(arg2);
	var output = [];
	var intOut = PST$intBuffer16;
	var success = COMMON.dateTime.unixToStructured(intOut, nullableTimeZone, unixTime);
	if (!success) {
		return vm[14];
	}
	var i = 0;
	while ((i < 9)) {
		output.push(buildInteger(vm[13], intOut[i]));
		i += 1;
	}
	return buildList(output);
};

var debuggerClearBreakpoint = function(vm, id) {
	return 0;
};

var debuggerFindPcForLine = function(vm, path, line) {
	return -1;
};

var debuggerSetBreakpoint = function(vm, path, line) {
	return -1;
};

var debugSetStepOverBreakpoint = function(vm) {
	return false;
};

var defOriginalCodeImpl = function(vm, row, fileContents) {
	var fileId = row[0];
	var codeLookup = vm[3][2];
	while ((codeLookup.length <= fileId)) {
		codeLookup.push(null);
	}
	codeLookup[fileId] = fileContents;
	return 0;
};

var dictKeyInfoToString = function(vm, dict) {
	if ((dict[1] == 5)) {
		return "string";
	}
	if ((dict[1] == 3)) {
		return "int";
	}
	if ((dict[2] == 0)) {
		return "instance";
	}
	return classIdToString(vm, dict[2]);
};

var doEqualityComparisonAndReturnCode = function(a, b) {
	var leftType = a[0];
	var rightType = b[0];
	if ((leftType == rightType)) {
		var output = 0;
		switch (leftType) {
			case 1:
				output = 1;
				break;
			case 3:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 4:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 2:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 5:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 6:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 7:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 8:
				if ((a[1] == b[1])) {
					output = 1;
				}
				break;
			case 9:
				var f1 = a[1];
				var f2 = b[1];
				if ((f1[3] == f2[3])) {
					if (((f1[0] == 2) || (f1[0] == 4))) {
						if ((doEqualityComparisonAndReturnCode(f1[1], f2[1]) == 1)) {
							output = 1;
						}
					} else {
						output = 1;
					}
				}
				break;
			case 10:
				var c1 = a[1];
				var c2 = b[1];
				if ((c1[1] == c2[1])) {
					output = 1;
				}
				break;
			default:
				output = 2;
				break;
		}
		return output;
	}
	if ((rightType == 1)) {
		return 0;
	}
	if (((leftType == 3) && (rightType == 4))) {
		if ((a[1] == b[1])) {
			return 1;
		}
	} else if (((leftType == 4) && (rightType == 3))) {
		if ((a[1] == b[1])) {
			return 1;
		}
	}
	return 0;
};

var doExponentMath = function(globals, b, e, preferInt) {
	if ((e == 0.0)) {
		if (preferInt) {
			return globals[4];
		}
		return globals[7];
	}
	if ((b == 0.0)) {
		if (preferInt) {
			return globals[3];
		}
		return globals[6];
	}
	var r = 0.0;
	if ((b < 0)) {
		if (((e >= 0) && (e < 1))) {
			return null;
		}
		if (((e % 1.0) == 0.0)) {
			var eInt = Math.floor(e);
			r = (0.0 + Math.pow(b, eInt));
		} else {
			return null;
		}
	} else {
		r = Math.pow(b, e);
	}
	if (preferInt) {
		r = fixFuzzyFloatPrecision(r);
		if (((r % 1.0) == 0.0)) {
			return buildInteger(globals, Math.floor(r));
		}
	}
	return buildFloat(globals, r);
};

var encodeBreakpointData = function(vm, breakpoint, pc) {
	return null;
};

var errorResult = function(error) {
	return [3, error, 0.0, 0, false, ""];
};

var EX_AssertionFailed = function(ec, exMsg) {
	return generateException2(ec, 2, exMsg);
};

var EX_DivisionByZero = function(ec, exMsg) {
	return generateException2(ec, 3, exMsg);
};

var EX_Fatal = function(ec, exMsg) {
	return generateException2(ec, 0, exMsg);
};

var EX_IndexOutOfRange = function(ec, exMsg) {
	return generateException2(ec, 4, exMsg);
};

var EX_InvalidArgument = function(ec, exMsg) {
	return generateException2(ec, 5, exMsg);
};

var EX_InvalidAssignment = function(ec, exMsg) {
	return generateException2(ec, 6, exMsg);
};

var EX_InvalidInvocation = function(ec, exMsg) {
	return generateException2(ec, 7, exMsg);
};

var EX_InvalidKey = function(ec, exMsg) {
	return generateException2(ec, 8, exMsg);
};

var EX_KeyNotFound = function(ec, exMsg) {
	return generateException2(ec, 9, exMsg);
};

var EX_NullReference = function(ec, exMsg) {
	return generateException2(ec, 10, exMsg);
};

var EX_UnassignedVariable = function(ec, exMsg) {
	return generateException2(ec, 11, exMsg);
};

var EX_UnknownField = function(ec, exMsg) {
	return generateException2(ec, 12, exMsg);
};

var EX_UnsupportedOperation = function(ec, exMsg) {
	return generateException2(ec, 13, exMsg);
};

var finalizeInitializationImpl = function(vm, projectId, localeCount) {
	vm[3][1] = PST$multiplyList(vm[3][2], 1);
	vm[3][2] = null;
	vm[4][18][2] = localeCount;
	vm[4][0] = PST$multiplyList(vm[4][1], 1);
	vm[4][3] = PST$multiplyList(vm[4][4], 1);
	vm[4][12] = primitiveMethodsInitializeLookup(vm[4][2]);
	vm[8] = PST$createNewArray(vm[4][0].length);
	vm[4][16] = projectId;
	vm[4][1] = null;
	vm[4][4] = null;
	vm[6] = true;
	return 0;
};

var fixFuzzyFloatPrecision = function(x) {
	if (((x % 1) != 0)) {
		var u = (x % 1);
		if ((u < 0)) {
			u += 1.0;
		}
		var roundDown = false;
		if ((u > 0.9999999999)) {
			roundDown = true;
			x += 0.1;
		} else if ((u < 0.00000000002250000000)) {
			roundDown = true;
		}
		if (roundDown) {
			x = (Math.floor(x) + 0.0);
		}
	}
	return x;
};

var generateEsfData = function(byteCodeLength, esfArgs) {
	var output = PST$createNewArray(byteCodeLength);
	var esfTokenStack = [];
	var esfTokenStackTop = null;
	var esfArgIterator = 0;
	var esfArgLength = esfArgs.length;
	var j = 0;
	var pc = 0;
	while ((pc < byteCodeLength)) {
		if (((esfArgIterator < esfArgLength) && (pc == esfArgs[esfArgIterator]))) {
			esfTokenStackTop = PST$createNewArray(2);
			j = 1;
			while ((j < 3)) {
				esfTokenStackTop[(j - 1)] = esfArgs[(esfArgIterator + j)];
				j += 1;
			}
			esfTokenStack.push(esfTokenStackTop);
			esfArgIterator += 3;
		}
		while (((esfTokenStackTop != null) && (esfTokenStackTop[1] <= pc))) {
			esfTokenStack.pop();
			if ((esfTokenStack.length == 0)) {
				esfTokenStackTop = null;
			} else {
				esfTokenStackTop = esfTokenStack[(esfTokenStack.length - 1)];
			}
		}
		output[pc] = esfTokenStackTop;
		pc += 1;
	}
	return output;
};

var generateException = function(vm, stack, pc, valueStackSize, ec, type, message) {
	ec[2] = valueStackSize;
	stack[0] = pc;
	var mn = vm[4][18];
	var generateExceptionFunctionId = mn[1];
	var functionInfo = vm[4][10][generateExceptionFunctionId];
	pc = functionInfo[2];
	if ((ec[5].length <= (functionInfo[7] + stack[3]))) {
		increaseLocalsStackCapacity(ec, functionInfo[7]);
	}
	var localsIndex = stack[3];
	var localsStackSetToken = (ec[7] + 1);
	ec[7] = localsStackSetToken;
	ec[5][localsIndex] = buildInteger(vm[13], type);
	ec[5][(localsIndex + 1)] = buildString(vm[13], message);
	ec[6][localsIndex] = localsStackSetToken;
	ec[6][(localsIndex + 1)] = localsStackSetToken;
	ec[1] = [(pc + 1), localsStackSetToken, stack[3], (stack[3] + functionInfo[7]), stack, false, null, valueStackSize, 0, (stack[9] + 1), 0, null, null, null];
	return [5, null, 0.0, 0, false, ""];
};

var generateException2 = function(ec, exceptionType, exMsg) {
	ec[13] = [1, exceptionType, exMsg, 0.0, null];
	return true;
};

var generatePrimitiveMethodReference = function(lookup, globalNameId, context) {
	var functionId = resolvePrimitiveMethodName2(lookup, context[0], globalNameId);
	if ((functionId < 0)) {
		return null;
	}
	return [9, [4, context, 0, functionId, null]];
};

var generateTokenListFromPcs = function(vm, pcs) {
	var output = [];
	var tokensByPc = vm[3][0];
	var token = null;
	var i = 0;
	while ((i < pcs.length)) {
		var localTokens = tokensByPc[pcs[i]];
		if ((localTokens == null)) {
			if ((output.length > 0)) {
				output.push(null);
			}
		} else {
			token = localTokens[0];
			output.push(token);
		}
		i += 1;
	}
	return output;
};

var getBinaryOpFromId = function(id) {
	switch (id) {
		case 0:
			return "+";
		case 1:
			return "-";
		case 2:
			return "*";
		case 3:
			return "/";
		case 4:
			return "%";
		case 5:
			return "**";
		case 6:
			return "&";
		case 7:
			return "|";
		case 8:
			return "^";
		case 9:
			return "<<";
		case 10:
			return ">>";
		case 11:
			return "<";
		case 12:
			return "<=";
		case 13:
			return ">";
		case 14:
			return ">=";
		default:
			return "unknown";
	}
};

var getClassTable = function(vm, classId) {
	var oldTable = vm[4][9];
	var oldLength = oldTable.length;
	if ((classId < oldLength)) {
		return oldTable;
	}
	var newLength = (oldLength * 2);
	if ((classId >= newLength)) {
		newLength = (classId + 100);
	}
	var newTable = PST$createNewArray(newLength);
	var i = (oldLength - 1);
	while ((i >= 0)) {
		newTable[i] = oldTable[i];
		i -= 1;
	}
	vm[4][9] = newTable;
	return newTable;
};

var getExecutionContext = function(vm, id) {
	if ((vm[0][id] !== undefined)) {
		return vm[0][id];
	}
	return null;
};

var getExponentErrorMsg = function(vm, b, e) {
	return ["Invalid values for exponent computation. Base: ", valueToString(vm, b), ", Power: ", valueToString(vm, e)].join('');
};

var getFloat = function(num) {
	if ((num[0] == 4)) {
		return num[1];
	}
	return (num[1] + 0.0);
};

var getFunctionTable = function(vm, functionId) {
	var oldTable = vm[4][10];
	var oldLength = oldTable.length;
	if ((functionId < oldLength)) {
		return oldTable;
	}
	var newLength = (oldLength * 2);
	if ((functionId >= newLength)) {
		newLength = (functionId + 100);
	}
	var newTable = PST$createNewArray(newLength);
	var i = 0;
	while ((i < oldLength)) {
		newTable[i] = oldTable[i];
		i += 1;
	}
	vm[4][10] = newTable;
	return newTable;
};

var getItemFromList = function(list, i) {
	return list[2][i];
};

var getNamedCallbackId = function(vm, scope, functionName) {
	return getNamedCallbackIdImpl(vm, scope, functionName, false);
};

var getNamedCallbackIdImpl = function(vm, scope, functionName, allocIfMissing) {
	var lookup = vm[12][1];
	var idsForScope = null;
	idsForScope = lookup[scope];
	if (idsForScope === undefined) idsForScope = null;
	if ((idsForScope == null)) {
		idsForScope = {};
		lookup[scope] = idsForScope;
	}
	var id = -1;
	id = idsForScope[functionName];
	if (id === undefined) id = -1;
	if (((id == -1) && allocIfMissing)) {
		id = vm[12][0].length;
		vm[12][0].push(null);
		idsForScope[functionName] = id;
	}
	return id;
};

var getNativeDataItem = function(objValue, index) {
	var obj = objValue[1];
	return obj[3][index];
};

var getTypeFromId = function(id) {
	switch (id) {
		case 1:
			return "null";
		case 2:
			return "boolean";
		case 3:
			return "integer";
		case 4:
			return "float";
		case 5:
			return "string";
		case 6:
			return "list";
		case 7:
			return "dictionary";
		case 8:
			return "instance";
		case 9:
			return "function";
	}
	return null;
};

var getVmId = function(vm) {
	return vm[17];
};

var getVmReinvokeDelay = function(result) {
	return result[2];
};

var getVmResultAssemblyInfo = function(result) {
	return result[5];
};

var getVmResultExecId = function(result) {
	return result[3];
};

var getVmResultStatus = function(result) {
	return result[0];
};

var ImageHelper_fromBytes = function(globals, bmp, isB64, rawData, sizeOut, callback) {
	var data = null;
	if (isB64) {
		if ((rawData[0] != 5)) {
			return 1;
		}
		data = rawData[1];
	} else {
		if ((rawData[0] != 6)) {
			return 1;
		}
		data = listImplToBytes(rawData[1]);
		if ((data == null)) {
			return 1;
		}
	}
	var sizeOutInt = PST$createNewArray(2);
	bmp[3] = PST$createNewArray(1);
	var isSync = COMMON.imageUtil.fromBytes(data, sizeOutInt, bmp[3], callback);
	if (isSync) {
		if ((bmp[3][0] == null)) {
			return 2;
		}
		addToList(sizeOut, buildInteger(globals, sizeOutInt[0]));
		addToList(sizeOut, buildInteger(globals, sizeOutInt[1]));
		return 0;
	}
	return 3;
};

var ImageHelper_GetChunkCache = function(cacheWrapper) {
	if ((cacheWrapper[3] == null)) {
		cacheWrapper[3] = PST$createNewArray(1);
		cacheWrapper[3][0] = {};
	}
	return cacheWrapper[3][0];
};

var ImageHelper_GetChunkSync = function(cacheWrapper, o, cid) {
	o[3] = PST$createNewArray(1);
	o[3][0] = null;
	var lookup = ImageHelper_GetChunkCache(cacheWrapper);
	if ((lookup[cid] !== undefined)) {
		o[3][0] = lookup[cid];
	}
};

var ImageHelper_GetPixel = function(nums, bmp, edit, xv, yv, pOut, arr) {
	if (((xv[0] != 3) || (yv[0] != 3))) {
		return 1;
	}
	var e = null;
	if ((edit != null)) {
		e = edit[3][0];
	}
	COMMON.imageUtil.getPixel(bmp[3][0], e, xv[1], yv[1], arr);
	if ((arr[4] == 0)) {
		return 2;
	}
	var c = [];
	pOut[1] = 4;
	pOut[2] = c;
	c.push(nums[arr[0]]);
	c.push(nums[arr[1]]);
	c.push(nums[arr[2]]);
	c.push(nums[arr[3]]);
	return 0;
};

var ImageHelper_ImageBlit = function(target, src, sx, sy, sw, sh, tx, ty, tw, th) {
	COMMON.imageUtil.blit(target[3][0], src[3][0], sx, sy, sw, sh, tx, ty, tw, th);
};

var ImageHelper_ImageCreate = function(o, w, h) {
	o[3] = PST$createNewArray(1);
	o[3][0] = COMMON.imageUtil.newBitmap(w, h);
};

var ImageHelper_ImageEncode = function(globals, bmp, format) {
	var o = PST$createNewArray(1);
	var result = COMMON.imageUtil.encode(bmp, format, o);
	if (o[0]) {
		return buildString(globals, result);
	}
	return bytesToListValue(globals, result);
};

var ImageHelper_LoadChunk = function(vm, cacheWrapper, chunkId, allChunkIds, loadedCallback) {
	var size = allChunkIds[1];
	var chunkIds = PST$createNewArray(size);
	var i = 0;
	while ((i < size)) {
		chunkIds[i] = allChunkIds[2][i][1];
		++i;
	}
	COMMON.imageUtil.chunkLoadAsync(vm, ImageHelper_GetChunkCache(cacheWrapper), chunkId, chunkIds, loadedCallback);
};

var ImageHelper_Scale = function(src, dest, newWidth, newHeight, algo) {
	dest[3] = PST$createNewArray(1);
	dest[3][0] = COMMON.imageUtil.scale(src[3][0], newWidth, newHeight, algo);
};

var ImageHelper_SessionFinish = function(edit, bmp) {
	COMMON.imageUtil.endEditSession(edit[3][0], bmp[3][0]);
};

var ImageHelper_SessionStart = function(edit, bmp) {
	edit[3] = PST$createNewArray(1);
	edit[3][0] = COMMON.imageUtil.startEditSession(bmp[3][0]);
};

var ImageHelper_SetPixel = function(edit, xv1, yv1, xv2, yv2, rOrList, gv, bv, av) {
	if (((xv1[0] != 3) || (yv1[0] != 3))) {
		return 1;
	}
	if (((xv2[0] != 3) || (yv2[0] != 3))) {
		return 1;
	}
	var r = 0;
	var g = 0;
	var b = 0;
	var a = 255;
	if ((rOrList[0] == 6)) {
		var color = rOrList[1];
		r = color[1];
		if ((r == 4)) {
			av = color[2][3];
		} else if ((r != 3)) {
			return 5;
		}
		rOrList = color[2][0];
		gv = color[2][1];
		bv = color[2][2];
	} else if ((rOrList[0] != 3)) {
		return 3;
	}
	if (((rOrList[0] != 3) || (gv[0] != 3) || (bv[0] != 3) || (av[0] != 3))) {
		return 3;
	}
	r = rOrList[1];
	g = gv[1];
	b = bv[1];
	a = av[1];
	if (((r < 0) || (r > 255) || (g < 0) || (g > 255) || (b < 0) || (b > 255) || (a < 0) || (a > 255))) {
		return 4;
	}
	var outOfRange = COMMON.imageUtil.setPixel(edit[3][0], xv1[1], yv1[1], xv2[1], yv2[1], r, g, b, a);
	if (outOfRange) {
		return 2;
	}
	return 0;
};

var increaseListCapacity = function(list) {
};

var increaseLocalsStackCapacity = function(ec, newScopeSize) {
	var oldLocals = ec[5];
	var oldSetIndicator = ec[6];
	var oldCapacity = oldLocals.length;
	var newCapacity = ((oldCapacity * 2) + newScopeSize);
	var newLocals = PST$createNewArray(newCapacity);
	var newSetIndicator = PST$createNewArray(newCapacity);
	var i = 0;
	while ((i < oldCapacity)) {
		newLocals[i] = oldLocals[i];
		newSetIndicator[i] = oldSetIndicator[i];
		i += 1;
	}
	ec[5] = newLocals;
	ec[6] = newSetIndicator;
	return 0;
};

var initFileNameSymbolData = function(vm) {
	var symbolData = vm[3];
	if ((symbolData == null)) {
		return 0;
	}
	if ((symbolData[3] == null)) {
		var i = 0;
		var filenames = PST$createNewArray(symbolData[1].length);
		var fileIdByPath = {};
		i = 0;
		while ((i < filenames.length)) {
			var sourceCode = symbolData[1][i];
			if ((sourceCode != null)) {
				var colon = sourceCode.indexOf("\n");
				if ((colon != -1)) {
					var filename = sourceCode.substring(0, 0 + colon);
					filenames[i] = filename;
					fileIdByPath[filename] = i;
				}
			}
			i += 1;
		}
		symbolData[3] = filenames;
		symbolData[4] = fileIdByPath;
	}
	return 0;
};

var initializeByteCode = function(raw) {
	var index = PST$createNewArray(1);
	index[0] = 0;
	var length = raw.length;
	var header = read_till(index, raw, length, "@");
	if ((header != "CRAYON")) {
	}
	var alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	var opCount = read_integer(index, raw, length, alphaNums);
	var ops = PST$createNewArray(opCount);
	var iargs = PST$createNewArray(opCount);
	var sargs = PST$createNewArray(opCount);
	var c = " ";
	var argc = 0;
	var j = 0;
	var stringarg = null;
	var stringPresent = false;
	var iarg = 0;
	var iarglist = null;
	var i = 0;
	i = 0;
	while ((i < opCount)) {
		c = raw.charAt(index[0]);
		index[0] = (index[0] + 1);
		argc = 0;
		stringPresent = true;
		if ((c == "!")) {
			argc = 1;
		} else if ((c == "&")) {
			argc = 2;
		} else if ((c == "*")) {
			argc = 3;
		} else {
			if ((c != "~")) {
				stringPresent = false;
				index[0] = (index[0] - 1);
			}
			argc = read_integer(index, raw, length, alphaNums);
		}
		iarglist = PST$createNewArray((argc - 1));
		j = 0;
		while ((j < argc)) {
			iarg = read_integer(index, raw, length, alphaNums);
			if ((j == 0)) {
				ops[i] = iarg;
			} else {
				iarglist[(j - 1)] = iarg;
			}
			j += 1;
		}
		iargs[i] = iarglist;
		if (stringPresent) {
			stringarg = read_string(index, raw, length, alphaNums);
		} else {
			stringarg = null;
		}
		sargs[i] = stringarg;
		i += 1;
	}
	var hasBreakpoint = PST$createNewArray(opCount);
	var breakpointInfo = PST$createNewArray(opCount);
	i = 0;
	while ((i < opCount)) {
		hasBreakpoint[i] = false;
		breakpointInfo[i] = null;
		i += 1;
	}
	return [ops, iargs, sargs, PST$createNewArray(opCount), PST$createNewArray(opCount), [hasBreakpoint, breakpointInfo, {}, 1, 0]];
};

var initializeClass = function(pc, vm, args, className) {
	var i = 0;
	var memberId = 0;
	var globalId = 0;
	var functionId = 0;
	var t = 0;
	var classId = args[0];
	var baseClassId = args[1];
	var globalNameId = args[2];
	var constructorFunctionId = args[3];
	var staticConstructorFunctionId = args[4];
	var staticInitializationState = 0;
	if ((staticConstructorFunctionId == -1)) {
		staticInitializationState = 2;
	}
	var staticFieldCount = args[5];
	var assemblyId = args[6];
	var staticFields = PST$createNewArray(staticFieldCount);
	i = 0;
	while ((i < staticFieldCount)) {
		staticFields[i] = vm[13][0];
		i += 1;
	}
	var classInfo = [classId, globalNameId, baseClassId, assemblyId, staticInitializationState, staticFields, staticConstructorFunctionId, constructorFunctionId, 0, null, null, null, null, null, vm[4][20][classId], null, className];
	var classTable = getClassTable(vm, classId);
	classTable[classId] = classInfo;
	var classChain = [];
	classChain.push(classInfo);
	var classIdWalker = baseClassId;
	while ((classIdWalker != -1)) {
		var walkerClass = classTable[classIdWalker];
		classChain.push(walkerClass);
		classIdWalker = walkerClass[2];
	}
	var baseClass = null;
	if ((baseClassId != -1)) {
		baseClass = classChain[1];
	}
	var functionIds = [];
	var fieldInitializationCommand = [];
	var fieldInitializationLiteral = [];
	var fieldAccessModifier = [];
	var globalNameIdToMemberId = {};
	if ((baseClass != null)) {
		i = 0;
		while ((i < baseClass[8])) {
			functionIds.push(baseClass[9][i]);
			fieldInitializationCommand.push(baseClass[10][i]);
			fieldInitializationLiteral.push(baseClass[11][i]);
			fieldAccessModifier.push(baseClass[12][i]);
			i += 1;
		}
		var keys = PST$dictionaryKeys(baseClass[13]);
		i = 0;
		while ((i < keys.length)) {
			t = keys[i];
			globalNameIdToMemberId[t] = baseClass[13][t];
			i += 1;
		}
		keys = PST$dictionaryKeys(baseClass[14]);
		i = 0;
		while ((i < keys.length)) {
			t = keys[i];
			classInfo[14][t] = baseClass[14][t];
			i += 1;
		}
	}
	var accessModifier = 0;
	i = 7;
	while ((i < args.length)) {
		memberId = args[(i + 1)];
		globalId = args[(i + 2)];
		accessModifier = args[(i + 5)];
		while ((memberId >= functionIds.length)) {
			functionIds.push(-1);
			fieldInitializationCommand.push(-1);
			fieldInitializationLiteral.push(null);
			fieldAccessModifier.push(0);
		}
		globalNameIdToMemberId[globalId] = memberId;
		fieldAccessModifier[memberId] = accessModifier;
		if ((args[i] == 0)) {
			fieldInitializationCommand[memberId] = args[(i + 3)];
			t = args[(i + 4)];
			if ((t == -1)) {
				fieldInitializationLiteral[memberId] = vm[13][0];
			} else {
				fieldInitializationLiteral[memberId] = vm[4][3][t];
			}
		} else {
			functionId = args[(i + 3)];
			functionIds[memberId] = functionId;
		}
		i += 6;
	}
	classInfo[9] = PST$multiplyList(functionIds, 1);
	classInfo[10] = PST$multiplyList(fieldInitializationCommand, 1);
	classInfo[11] = PST$multiplyList(fieldInitializationLiteral, 1);
	classInfo[12] = PST$multiplyList(fieldAccessModifier, 1);
	classInfo[8] = functionIds.length;
	classInfo[13] = globalNameIdToMemberId;
	classInfo[15] = PST$createNewArray(classInfo[8]);
	if ((baseClass != null)) {
		i = 0;
		while ((i < baseClass[15].length)) {
			classInfo[15][i] = baseClass[15][i];
			i += 1;
		}
	}
	if ("Core.Exception" == className) {
		var mn = vm[4][18];
		mn[0] = classId;
	}
	return 0;
};

var initializeClassFieldTypeInfo = function(vm, opCodeRow) {
	var classInfo = vm[4][9][opCodeRow[0]];
	var memberId = opCodeRow[1];
	var _len = opCodeRow.length;
	var typeInfo = PST$createNewArray((_len - 2));
	var i = 2;
	while ((i < _len)) {
		typeInfo[(i - 2)] = opCodeRow[i];
		i += 1;
	}
	classInfo[15][memberId] = typeInfo;
	return 0;
};

var initializeConstantValues = function() {
	var pos = PST$createNewArray(2049);
	var neg = PST$createNewArray(257);
	var i = 0;
	while ((i < 2049)) {
		pos[i] = [3, i];
		i += 1;
	}
	i = 1;
	while ((i < 257)) {
		neg[i] = [3, -i];
		i += 1;
	}
	neg[0] = pos[0];
	var globals = [[1, null], [2, true], [2, false], pos[0], pos[1], neg[1], [4, 0.0], [4, 1.0], [5, ""], pos, neg, {}, PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(2)];
	globals[11][""] = globals[8];
	globals[12][0] = 2;
	globals[13][0] = 3;
	globals[15][0] = 4;
	globals[14][0] = 5;
	globals[16][0] = 10;
	globals[17][0] = 8;
	globals[17][1] = 0;
	return globals;
};

var initializeFunction = function(vm, args, currentPc, stringArg) {
	var functionId = args[0];
	var nameId = args[1];
	var minArgCount = args[2];
	var maxArgCount = args[3];
	var functionType = args[4];
	var classId = args[5];
	var localsCount = args[6];
	var numPcOffsetsForOptionalArgs = args[8];
	var pcOffsetsForOptionalArgs = PST$createNewArray((numPcOffsetsForOptionalArgs + 1));
	var i = 0;
	while ((i < numPcOffsetsForOptionalArgs)) {
		pcOffsetsForOptionalArgs[(i + 1)] = args[(9 + i)];
		i += 1;
	}
	var functionTable = getFunctionTable(vm, functionId);
	functionTable[functionId] = [functionId, nameId, currentPc, minArgCount, maxArgCount, functionType, classId, localsCount, pcOffsetsForOptionalArgs, stringArg, null];
	vm[4][21] = functionTable[functionId];
	if ((nameId >= 0)) {
		var name = vm[4][0][nameId];
		if ("_LIB_CORE_list_filter" == name) {
			vm[4][14][0] = functionId;
		} else if ("_LIB_CORE_list_map" == name) {
			vm[4][14][1] = functionId;
		} else if ("_LIB_CORE_list_sort_by_key" == name) {
			vm[4][14][2] = functionId;
		} else if ("_LIB_CORE_invoke" == name) {
			vm[4][14][3] = functionId;
		} else if ("_LIB_CORE_list_reduce" == name) {
			vm[4][14][4] = functionId;
		} else if ("_LIB_CORE_generateException" == name) {
			var mn = vm[4][18];
			mn[1] = functionId;
		}
	}
	return 0;
};

var initializeIntSwitchStatement = function(vm, pc, args) {
	var output = {};
	var i = 1;
	while ((i < args.length)) {
		output[args[i]] = args[(i + 1)];
		i += 2;
	}
	vm[2][3][pc] = output;
	return output;
};

var initializeStringSwitchStatement = function(vm, pc, args) {
	var output = {};
	var i = 1;
	while ((i < args.length)) {
		var s = vm[4][3][args[i]][1];
		output[s] = args[(i + 1)];
		i += 2;
	}
	vm[2][4][pc] = output;
	return output;
};

var initLocTable = function(vm, row) {
	var classId = row[0];
	var memberCount = row[1];
	var nameId = 0;
	var totalLocales = vm[4][18][2];
	var lookup = {};
	var i = 2;
	while ((i < row.length)) {
		var localeId = row[i];
		i += 1;
		var j = 0;
		while ((j < memberCount)) {
			nameId = row[(i + j)];
			if ((nameId != -1)) {
				lookup[((nameId * totalLocales) + localeId)] = j;
			}
			j += 1;
		}
		i += memberCount;
	}
	vm[4][20][classId] = lookup;
	return 0;
};

var interpret = function(vm, executionContextId) {
	var output = interpretImpl(vm, executionContextId);
	while (((output[0] == 5) && (output[2] == 0))) {
		output = interpretImpl(vm, executionContextId);
	}
	return output;
};

var interpreterFinished = function(vm, ec) {
	if ((ec != null)) {
		var id = ec[0];
		if ((vm[0][id] !== undefined)) {
			delete vm[0][id];
		}
	}
	return [1, null, 0.0, 0, false, ""];
};

var interpreterGetExecutionContext = function(vm, executionContextId) {
	var executionContexts = vm[0];
	if (!(executionContexts[executionContextId] !== undefined)) {
		return null;
	}
	return executionContexts[executionContextId];
};

var interpretImpl = function(vm, executionContextId) {
	var metadata = vm[4];
	var globals = vm[13];
	var VALUE_NULL = globals[0];
	var VALUE_TRUE = globals[1];
	var VALUE_FALSE = globals[2];
	var VALUE_INT_ONE = globals[4];
	var VALUE_INT_ZERO = globals[3];
	var VALUE_FLOAT_ZERO = globals[6];
	var VALUE_FLOAT_ONE = globals[7];
	var INTEGER_POSITIVE_CACHE = globals[9];
	var INTEGER_NEGATIVE_CACHE = globals[10];
	var executionContexts = vm[0];
	var ec = interpreterGetExecutionContext(vm, executionContextId);
	if ((ec == null)) {
		return interpreterFinished(vm, null);
	}
	ec[8] += 1;
	var stack = ec[1];
	var ops = vm[2][0];
	var args = vm[2][1];
	var stringArgs = vm[2][2];
	var classTable = vm[4][9];
	var functionTable = vm[4][10];
	var literalTable = vm[4][3];
	var identifiers = vm[4][0];
	var valueStack = ec[4];
	var valueStackSize = ec[2];
	var valueStackCapacity = valueStack.length;
	var hasInterrupt = false;
	var type = 0;
	var nameId = 0;
	var classId = 0;
	var functionId = 0;
	var localeId = 0;
	var classInfo = null;
	var _len = 0;
	var root = null;
	var row = null;
	var argCount = 0;
	var stringList = null;
	var returnValueUsed = false;
	var output = null;
	var functionInfo = null;
	var keyType = 0;
	var intKey = 0;
	var stringKey = null;
	var first = false;
	var primitiveMethodToCoreLibraryFallback = false;
	var bool1 = false;
	var bool2 = false;
	var staticConstructorNotInvoked = true;
	var int1 = 0;
	var int2 = 0;
	var int3 = 0;
	var i = 0;
	var j = 0;
	var float1 = 0.0;
	var float2 = 0.0;
	var float3 = 0.0;
	var floatList1 = PST$createNewArray(2);
	var value = null;
	var value2 = null;
	var value3 = null;
	var string1 = null;
	var string2 = null;
	var objInstance1 = null;
	var objInstance2 = null;
	var obj1 = null;
	var list1 = null;
	var list2 = null;
	var valueList1 = null;
	var valueList2 = null;
	var dictImpl = null;
	var dictImpl2 = null;
	var stringList1 = null;
	var intList1 = null;
	var valueArray1 = null;
	var intArray1 = null;
	var intArray2 = null;
	var objArray1 = null;
	var functionPointer1 = null;
	var intIntDict1 = null;
	var stringIntDict1 = null;
	var stackFrame2 = null;
	var leftValue = null;
	var rightValue = null;
	var classValue = null;
	var arg1 = null;
	var arg2 = null;
	var arg3 = null;
	var arg4 = null;
	var arg5 = null;
	var arg6 = null;
	var arg7 = null;
	var arg8 = null;
	var arg9 = null;
	var arg10 = null;
	var globalNameIdToPrimitiveMethodName = vm[4][12];
	var magicNumbers = vm[4][18];
	var integerSwitchesByPc = vm[2][3];
	var stringSwitchesByPc = vm[2][4];
	var integerSwitch = null;
	var stringSwitch = null;
	var esfData = vm[4][17];
	var closure = null;
	var parentClosure = null;
	var intBuffer = PST$createNewArray(16);
	var localsStack = ec[5];
	var localsStackSet = ec[6];
	var localsStackSetToken = stack[1];
	var localsStackCapacity = localsStack.length;
	var localsStackOffset = stack[2];
	var funcArgs = vm[8];
	var pc = stack[0];
	var debugData = vm[2][5];
	var isBreakPointPresent = debugData[0];
	while (true) {
		row = args[pc];
		switch (ops[pc]) {
			case 0:
				// ADD_LITERAL;
				addLiteralImpl(vm, row, stringArgs[pc]);
				break;
			case 1:
				// ADD_NAME;
				addNameImpl(vm, stringArgs[pc]);
				break;
			case 2:
				// ARG_TYPE_VERIFY;
				_len = row[0];
				i = 1;
				j = 0;
				while ((j < _len)) {
					j += 1;
				}
				break;
			case 3:
				// ASSIGN_CLOSURE;
				value = valueStack[--valueStackSize];
				i = row[0];
				if ((stack[12] == null)) {
					closure = {};
					closure[-1] = [stack[6]];
					stack[12] = closure;
					closure[i] = [value];
				} else {
					closure = stack[12];
					if ((closure[i] !== undefined)) {
						closure[i][0] = value;
					} else {
						closure[i] = [value];
					}
				}
				break;
			case 4:
				// ASSIGN_INDEX;
				valueStackSize -= 3;
				value = valueStack[(valueStackSize + 2)];
				value2 = valueStack[(valueStackSize + 1)];
				root = valueStack[valueStackSize];
				type = root[0];
				bool1 = (row[0] == 1);
				if ((type == 6)) {
					if ((value2[0] == 3)) {
						i = value2[1];
						list1 = root[1];
						if ((list1[0] != null)) {
							value3 = canAssignTypeToGeneric(vm, value, list1[0], 0);
							if ((value3 == null)) {
								hasInterrupt = EX_InvalidArgument(ec, ["Cannot convert a ", typeToStringFromValue(vm, value), " into a ", typeToString(vm, list1[0], 0)].join(''));
							}
							value = value3;
						}
						if (!hasInterrupt) {
							if ((i >= list1[1])) {
								hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.");
							} else if ((i < 0)) {
								i += list1[1];
								if ((i < 0)) {
									hasInterrupt = EX_IndexOutOfRange(ec, "Index is out of range.");
								}
							}
							if (!hasInterrupt) {
								list1[2][i] = value;
							}
						}
					} else {
						hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.");
					}
				} else if ((type == 7)) {
					dictImpl = root[1];
					if ((dictImpl[3] != null)) {
						value3 = canAssignTypeToGeneric(vm, value, dictImpl[3], 0);
						if ((value3 == null)) {
							hasInterrupt = EX_InvalidArgument(ec, "Cannot assign a value to this dictionary of this type.");
						} else {
							value = value3;
						}
					}
					keyType = value2[0];
					if ((keyType == 3)) {
						intKey = value2[1];
					} else if ((keyType == 5)) {
						stringKey = value2[1];
					} else if ((keyType == 8)) {
						objInstance1 = value2[1];
						intKey = objInstance1[1];
					} else {
						hasInterrupt = EX_InvalidArgument(ec, "Invalid key for a dictionary.");
					}
					if (!hasInterrupt) {
						bool2 = (dictImpl[0] == 0);
						if ((dictImpl[1] != keyType)) {
							if ((dictImpl[3] != null)) {
								string1 = ["Cannot assign a key of type ", typeToStringFromValue(vm, value2), " to a dictionary that requires key types of ", dictKeyInfoToString(vm, dictImpl), "."].join('');
								hasInterrupt = EX_InvalidKey(ec, string1);
							} else if (!bool2) {
								hasInterrupt = EX_InvalidKey(ec, "Cannot have multiple keys in one dictionary with different types.");
							}
						} else if (((keyType == 8) && (dictImpl[2] > 0) && (objInstance1[0] != dictImpl[2]))) {
							if (isClassASubclassOf(vm, objInstance1[0], dictImpl[2])) {
								hasInterrupt = EX_InvalidKey(ec, "Cannot use this type of object as a key for this dictionary.");
							}
						}
					}
					if (!hasInterrupt) {
						if ((keyType == 5)) {
							int1 = dictImpl[5][stringKey];
							if (int1 === undefined) int1 = -1;
							if ((int1 == -1)) {
								dictImpl[5][stringKey] = dictImpl[0];
								dictImpl[0] += 1;
								dictImpl[6].push(value2);
								dictImpl[7].push(value);
								if (bool2) {
									dictImpl[1] = keyType;
								}
							} else {
								dictImpl[7][int1] = value;
							}
						} else {
							int1 = dictImpl[4][intKey];
							if (int1 === undefined) int1 = -1;
							if ((int1 == -1)) {
								dictImpl[4][intKey] = dictImpl[0];
								dictImpl[0] += 1;
								dictImpl[6].push(value2);
								dictImpl[7].push(value);
								if (bool2) {
									dictImpl[1] = keyType;
								}
							} else {
								dictImpl[7][int1] = value;
							}
						}
					}
				} else if ((type == 1)) {
					hasInterrupt = EX_NullReference(ec, "Cannot assign to an index or key on null.");
				} else {
					hasInterrupt = EX_UnsupportedOperation(ec, getTypeFromId(type) + " type does not support assigning to an index.");
				}
				if (bool1) {
					valueStack[valueStackSize] = value;
					valueStackSize += 1;
				}
				break;
			case 6:
				// ASSIGN_STATIC_FIELD;
				classInfo = classTable[row[0]];
				staticConstructorNotInvoked = true;
				if ((classInfo[4] < 2)) {
					stack[0] = pc;
					stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST$intBuffer16);
					if ((PST$intBuffer16[0] == 1)) {
						return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
					}
					if ((stackFrame2 != null)) {
						staticConstructorNotInvoked = false;
						stack = stackFrame2;
						pc = stack[0];
						localsStackSetToken = stack[1];
						localsStackOffset = stack[2];
					}
				}
				if (staticConstructorNotInvoked) {
					valueStackSize -= 1;
					classInfo[5][row[1]] = valueStack[valueStackSize];
				}
				break;
			case 7:
				// ASSIGN_FIELD;
				valueStackSize -= 2;
				value = valueStack[(valueStackSize + 1)];
				value2 = valueStack[valueStackSize];
				nameId = row[2];
				if ((value2[0] == 8)) {
					objInstance1 = value2[1];
					classId = objInstance1[0];
					classInfo = classTable[classId];
					intIntDict1 = classInfo[14];
					if ((row[5] == classId)) {
						int1 = row[6];
					} else {
						int1 = intIntDict1[nameId];
						if (int1 === undefined) int1 = -1;
						if ((int1 != -1)) {
							int3 = classInfo[12][int1];
							if ((int3 > 1)) {
								if ((int3 == 2)) {
									if ((classId != row[3])) {
										int1 = -2;
									}
								} else {
									if (((int3 == 3) || (int3 == 5))) {
										if ((classInfo[3] != row[4])) {
											int1 = -3;
										}
									}
									if (((int3 == 4) || (int3 == 5))) {
										i = row[3];
										if ((classId == i)) {
										} else {
											classInfo = classTable[classInfo[0]];
											while (((classInfo[2] != -1) && (int1 < classTable[classInfo[2]][12].length))) {
												classInfo = classTable[classInfo[2]];
											}
											j = classInfo[0];
											if ((j != i)) {
												bool1 = false;
												while (((i != -1) && (classTable[i][2] != -1))) {
													i = classTable[i][2];
													if ((i == j)) {
														bool1 = true;
														i = -1;
													}
												}
												if (!bool1) {
													int1 = -4;
												}
											}
										}
										classInfo = classTable[classId];
									}
								}
							}
						}
						row[5] = classId;
						row[6] = int1;
					}
					if ((int1 > -1)) {
						int2 = classInfo[9][int1];
						if ((int2 == -1)) {
							intArray1 = classInfo[15][int1];
							if ((intArray1 == null)) {
								objInstance1[2][int1] = value;
							} else {
								value2 = canAssignTypeToGeneric(vm, value, intArray1, 0);
								if ((value2 != null)) {
									objInstance1[2][int1] = value2;
								} else {
									hasInterrupt = EX_InvalidArgument(ec, "Cannot assign this type to this field.");
								}
							}
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "Cannot override a method with assignment.");
						}
					} else if ((int1 < -1)) {
						string1 = identifiers[row[0]];
						if ((int1 == -2)) {
							string2 = "private";
						} else if ((int1 == -3)) {
							string2 = "internal";
						} else {
							string2 = "protected";
						}
						hasInterrupt = EX_UnknownField(ec, ["The field '", string1, "' is marked as ", string2, " and cannot be accessed from here."].join(''));
					} else {
						hasInterrupt = EX_InvalidAssignment(ec, ["'", classInfo[16], "' instances do not have a field called '", metadata[0][row[0]], "'"].join(''));
					}
				} else if ((value2[0] == 1)) {
					hasInterrupt = EX_NullReference(ec, "Cannot assign to a field on null.");
				} else {
					hasInterrupt = EX_InvalidAssignment(ec, "Cannot assign to a field on this type.");
				}
				if ((row[1] == 1)) {
					valueStack[valueStackSize++] = value;
				}
				break;
			case 8:
				// ASSIGN_THIS_FIELD;
				objInstance2 = stack[6][1];
				objInstance2[2][row[0]] = valueStack[--valueStackSize];
				break;
			case 5:
				// ASSIGN_LOCAL;
				i = (localsStackOffset + row[0]);
				localsStack[i] = valueStack[--valueStackSize];
				localsStackSet[i] = localsStackSetToken;
				break;
			case 9:
				// BINARY_OP;
				rightValue = valueStack[--valueStackSize];
				leftValue = valueStack[(valueStackSize - 1)];
				switch (((((leftValue[0] * 15) + row[0]) * 11) + rightValue[0])) {
					case 553:
						// int ** int;
						value = doExponentMath(globals, (0.0 + leftValue[1]), (0.0 + rightValue[1]), false);
						if ((value == null)) {
							hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
						}
						break;
					case 554:
						// int ** float;
						value = doExponentMath(globals, (0.0 + leftValue[1]), rightValue[1], false);
						if ((value == null)) {
							hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
						}
						break;
					case 718:
						// float ** int;
						value = doExponentMath(globals, leftValue[1], (0.0 + rightValue[1]), false);
						if ((value == null)) {
							hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
						}
						break;
					case 719:
						// float ** float;
						value = doExponentMath(globals, leftValue[1], rightValue[1], false);
						if ((value == null)) {
							hasInterrupt = EX_InvalidArgument(ec, getExponentErrorMsg(vm, leftValue, rightValue));
						}
						break;
					case 708:
						// float % float;
						float1 = rightValue[1];
						if ((float1 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
						} else {
							float3 = (leftValue[1] % float1);
							if ((float3 < 0)) {
								float3 += float1;
							}
							value = buildFloat(globals, float3);
						}
						break;
					case 707:
						// float % int;
						int1 = rightValue[1];
						if ((int1 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
						} else {
							float1 = (leftValue[1] % int1);
							if ((float1 < 0)) {
								float1 += int1;
							}
							value = buildFloat(globals, float1);
						}
						break;
					case 543:
						// int % float;
						float3 = rightValue[1];
						if ((float3 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
						} else {
							float1 = (leftValue[1] % float3);
							if ((float1 < 0)) {
								float1 += float3;
							}
							value = buildFloat(globals, float1);
						}
						break;
					case 542:
						// int % int;
						int2 = rightValue[1];
						if ((int2 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Modulo by 0.");
						} else {
							int1 = (leftValue[1] % int2);
							if ((int1 < 0)) {
								int1 += int2;
							}
							value = buildInteger(globals, int1);
						}
						break;
					case 996:
						// list + list;
						value = [6, valueConcatLists(leftValue[1], rightValue[1])];
						break;
					case 498:
						// int + int;
						int1 = (leftValue[1] + rightValue[1]);
						if ((int1 < 0)) {
							if ((int1 > -257)) {
								value = INTEGER_NEGATIVE_CACHE[-int1];
							} else {
								value = [3, int1];
							}
						} else if ((int1 < 2049)) {
							value = INTEGER_POSITIVE_CACHE[int1];
						} else {
							value = [3, int1];
						}
						break;
					case 509:
						// int - int;
						int1 = (leftValue[1] - rightValue[1]);
						if ((int1 < 0)) {
							if ((int1 > -257)) {
								value = INTEGER_NEGATIVE_CACHE[-int1];
							} else {
								value = [3, int1];
							}
						} else if ((int1 < 2049)) {
							value = INTEGER_POSITIVE_CACHE[int1];
						} else {
							value = [3, int1];
						}
						break;
					case 520:
						// int * int;
						int1 = (leftValue[1] * rightValue[1]);
						if ((int1 < 0)) {
							if ((int1 > -257)) {
								value = INTEGER_NEGATIVE_CACHE[-int1];
							} else {
								value = [3, int1];
							}
						} else if ((int1 < 2049)) {
							value = INTEGER_POSITIVE_CACHE[int1];
						} else {
							value = [3, int1];
						}
						break;
					case 531:
						// int / int;
						int1 = leftValue[1];
						int2 = rightValue[1];
						if ((int2 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
						} else if ((int1 == 0)) {
							value = VALUE_INT_ZERO;
						} else {
							if (((int1 % int2) == 0)) {
								int3 = Math.floor(int1 / int2);
							} else if ((((int1 < 0)) != ((int2 < 0)))) {
								float1 = (1 + ((-1.0 * int1) / int2));
								float1 -= (float1 % 1.0);
								int3 = Math.floor((-float1));
							} else {
								int3 = Math.floor(int1 / int2);
							}
							if ((int3 < 0)) {
								if ((int3 > -257)) {
									value = INTEGER_NEGATIVE_CACHE[-int3];
								} else {
									value = [3, int3];
								}
							} else if ((int3 < 2049)) {
								value = INTEGER_POSITIVE_CACHE[int3];
							} else {
								value = [3, int3];
							}
						}
						break;
					case 663:
						// float + int;
						value = buildFloat(globals, (leftValue[1] + rightValue[1]));
						break;
					case 499:
						// int + float;
						value = buildFloat(globals, (leftValue[1] + rightValue[1]));
						break;
					case 664:
						// float + float;
						float1 = (leftValue[1] + rightValue[1]);
						if ((float1 == 0)) {
							value = VALUE_FLOAT_ZERO;
						} else if ((float1 == 1)) {
							value = VALUE_FLOAT_ONE;
						} else {
							value = [4, float1];
						}
						break;
					case 510:
						// int - float;
						value = buildFloat(globals, (leftValue[1] - rightValue[1]));
						break;
					case 674:
						// float - int;
						value = buildFloat(globals, (leftValue[1] - rightValue[1]));
						break;
					case 675:
						// float - float;
						float1 = (leftValue[1] - rightValue[1]);
						if ((float1 == 0)) {
							value = VALUE_FLOAT_ZERO;
						} else if ((float1 == 1)) {
							value = VALUE_FLOAT_ONE;
						} else {
							value = [4, float1];
						}
						break;
					case 685:
						// float * int;
						value = buildFloat(globals, (leftValue[1] * rightValue[1]));
						break;
					case 521:
						// int * float;
						value = buildFloat(globals, (leftValue[1] * rightValue[1]));
						break;
					case 686:
						// float * float;
						value = buildFloat(globals, (leftValue[1] * rightValue[1]));
						break;
					case 532:
						// int / float;
						float1 = rightValue[1];
						if ((float1 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
						} else {
							value = buildFloat(globals, (leftValue[1] / float1));
						}
						break;
					case 696:
						// float / int;
						int1 = rightValue[1];
						if ((int1 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
						} else {
							value = buildFloat(globals, (leftValue[1] / int1));
						}
						break;
					case 697:
						// float / float;
						float1 = rightValue[1];
						if ((float1 == 0)) {
							hasInterrupt = EX_DivisionByZero(ec, "Division by 0.");
						} else {
							value = buildFloat(globals, (leftValue[1] / float1));
						}
						break;
					case 564:
						// int & int;
						value = buildInteger(globals, (leftValue[1] & rightValue[1]));
						break;
					case 575:
						// int | int;
						value = buildInteger(globals, (leftValue[1] | rightValue[1]));
						break;
					case 586:
						// int ^ int;
						value = buildInteger(globals, (leftValue[1] ^ rightValue[1]));
						break;
					case 597:
						// int << int;
						int1 = rightValue[1];
						if ((int1 < 0)) {
							hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.");
						} else {
							value = buildInteger(globals, (leftValue[1] << int1));
						}
						break;
					case 608:
						// int >> int;
						int1 = rightValue[1];
						if ((int1 < 0)) {
							hasInterrupt = EX_InvalidArgument(ec, "Cannot bit shift by a negative number.");
						} else {
							value = buildInteger(globals, (leftValue[1] >> int1));
						}
						break;
					case 619:
						// int < int;
						if ((leftValue[1] < rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 630:
						// int <= int;
						if ((leftValue[1] <= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 784:
						// float < int;
						if ((leftValue[1] < rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 795:
						// float <= int;
						if ((leftValue[1] <= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 620:
						// int < float;
						if ((leftValue[1] < rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 631:
						// int <= float;
						if ((leftValue[1] <= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 785:
						// float < float;
						if ((leftValue[1] < rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 796:
						// float <= float;
						if ((leftValue[1] <= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 652:
						// int >= int;
						if ((leftValue[1] >= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 641:
						// int > int;
						if ((leftValue[1] > rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 817:
						// float >= int;
						if ((leftValue[1] >= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 806:
						// float > int;
						if ((leftValue[1] > rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 653:
						// int >= float;
						if ((leftValue[1] >= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 642:
						// int > float;
						if ((leftValue[1] > rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 818:
						// float >= float;
						if ((leftValue[1] >= rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 807:
						// float > float;
						if ((leftValue[1] > rightValue[1])) {
							value = VALUE_TRUE;
						} else {
							value = VALUE_FALSE;
						}
						break;
					case 830:
						// string + string;
						value = [5, leftValue[1] + rightValue[1]];
						break;
					case 850:
						// string * int;
						value = multiplyString(globals, leftValue, leftValue[1], rightValue[1]);
						break;
					case 522:
						// int * string;
						value = multiplyString(globals, rightValue, rightValue[1], leftValue[1]);
						break;
					case 1015:
						// list * int;
						int1 = rightValue[1];
						if ((int1 < 0)) {
							hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.");
						} else {
							value = [6, valueMultiplyList(leftValue[1], int1)];
						}
						break;
					case 523:
						// int * list;
						int1 = leftValue[1];
						if ((int1 < 0)) {
							hasInterrupt = EX_UnsupportedOperation(ec, "Cannot multiply list by negative number.");
						} else {
							value = [6, valueMultiplyList(rightValue[1], int1)];
						}
						break;
					default:
						if (((row[0] == 0) && (((leftValue[0] == 5) || (rightValue[0] == 5))))) {
							value = [5, valueToString(vm, leftValue) + valueToString(vm, rightValue)];
						} else {
							// unrecognized op;
							hasInterrupt = EX_UnsupportedOperation(ec, ["The '", getBinaryOpFromId(row[0]), "' operator is not supported for these types: ", getTypeFromId(leftValue[0]), " and ", getTypeFromId(rightValue[0])].join(''));
						}
						break;
				}
				valueStack[(valueStackSize - 1)] = value;
				break;
			case 10:
				// BOOLEAN_NOT;
				value = valueStack[(valueStackSize - 1)];
				if ((value[0] != 2)) {
					hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
				} else if (value[1]) {
					valueStack[(valueStackSize - 1)] = VALUE_FALSE;
				} else {
					valueStack[(valueStackSize - 1)] = VALUE_TRUE;
				}
				break;
			case 11:
				// BREAK;
				if ((row[0] == 1)) {
					pc += row[1];
				} else {
					intArray1 = esfData[pc];
					pc = (intArray1[1] - 1);
					valueStackSize = stack[7];
					stack[10] = 1;
				}
				break;
			case 12:
				// CALL_FUNCTION;
				type = row[0];
				argCount = row[1];
				functionId = row[2];
				returnValueUsed = (row[3] == 1);
				classId = row[4];
				if (((type == 2) || (type == 6))) {
					// constructor or static method;
					classInfo = metadata[9][classId];
					staticConstructorNotInvoked = true;
					if ((classInfo[4] < 2)) {
						stack[0] = pc;
						stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST$intBuffer16);
						if ((PST$intBuffer16[0] == 1)) {
							return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
						}
						if ((stackFrame2 != null)) {
							staticConstructorNotInvoked = false;
							stack = stackFrame2;
							pc = stack[0];
							localsStackSetToken = stack[1];
							localsStackOffset = stack[2];
						}
					}
				} else {
					staticConstructorNotInvoked = true;
				}
				if (staticConstructorNotInvoked) {
					bool1 = true;
					// construct args array;
					if ((argCount == -1)) {
						valueStackSize -= 1;
						value = valueStack[valueStackSize];
						if ((value[0] == 1)) {
							argCount = 0;
						} else if ((value[0] == 6)) {
							list1 = value[1];
							argCount = list1[1];
							i = (argCount - 1);
							while ((i >= 0)) {
								funcArgs[i] = list1[2][i];
								i -= 1;
							}
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "Function pointers' .invoke method requires a list argument.");
						}
					} else {
						i = (argCount - 1);
						while ((i >= 0)) {
							valueStackSize -= 1;
							funcArgs[i] = valueStack[valueStackSize];
							i -= 1;
						}
					}
					if (!hasInterrupt) {
						if ((type == 3)) {
							value = stack[6];
							objInstance1 = value[1];
							if ((objInstance1[0] != classId)) {
								int2 = row[5];
								if ((int2 != -1)) {
									classInfo = classTable[objInstance1[0]];
									functionId = classInfo[9][int2];
								}
							}
						} else if ((type == 5)) {
							// field invocation;
							valueStackSize -= 1;
							value = valueStack[valueStackSize];
							localeId = row[5];
							switch (value[0]) {
								case 1:
									hasInterrupt = EX_NullReference(ec, "Invoked method on null.");
									break;
								case 8:
									// field invoked on an object instance.;
									objInstance1 = value[1];
									int1 = objInstance1[0];
									classInfo = classTable[int1];
									intIntDict1 = classInfo[14];
									int1 = ((row[4] * magicNumbers[2]) + row[5]);
									i = intIntDict1[int1];
									if (i === undefined) i = -1;
									if ((i != -1)) {
										int1 = intIntDict1[int1];
										functionId = classInfo[9][int1];
										if ((functionId > 0)) {
											type = 3;
										} else {
											value = objInstance1[2][int1];
											type = 4;
											valueStack[valueStackSize] = value;
											valueStackSize += 1;
										}
									} else {
										hasInterrupt = EX_UnknownField(ec, "Unknown field.");
									}
									break;
								case 10:
									// field invocation on a class object instance.;
									functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value[0], classId);
									if ((functionId < 0)) {
										hasInterrupt = EX_InvalidInvocation(ec, "Class definitions do not have that method.");
									} else {
										functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value[0], classId);
										if ((functionId < 0)) {
											hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value[0]) + " does not have that method.");
										} else if ((globalNameIdToPrimitiveMethodName[classId] == 8)) {
											type = 6;
											classValue = value[1];
											if (classValue[0]) {
												hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance of an interface.");
											} else {
												classId = classValue[1];
												if (!returnValueUsed) {
													hasInterrupt = EX_UnsupportedOperation(ec, "Cannot create an instance and not use the output.");
												} else {
													classInfo = metadata[9][classId];
													functionId = classInfo[7];
												}
											}
										} else {
											type = 9;
										}
									}
									break;
								default:
									// primitive method suspected.;
									functionId = resolvePrimitiveMethodName2(globalNameIdToPrimitiveMethodName, value[0], classId);
									if ((functionId < 0)) {
										hasInterrupt = EX_InvalidInvocation(ec, getTypeFromId(value[0]) + " does not have that method.");
									} else {
										type = 9;
									}
									break;
							}
						}
					}
					if (((type == 4) && !hasInterrupt)) {
						// pointer provided;
						valueStackSize -= 1;
						value = valueStack[valueStackSize];
						if ((value[0] == 9)) {
							functionPointer1 = value[1];
							switch (functionPointer1[0]) {
								case 1:
									// pointer to a function;
									functionId = functionPointer1[3];
									type = 1;
									break;
								case 2:
									// pointer to a method;
									functionId = functionPointer1[3];
									value = functionPointer1[1];
									type = 3;
									break;
								case 3:
									// pointer to a static method;
									functionId = functionPointer1[3];
									classId = functionPointer1[2];
									type = 2;
									break;
								case 4:
									// pointer to a primitive method;
									value = functionPointer1[1];
									functionId = functionPointer1[3];
									type = 9;
									break;
								case 5:
									// lambda instance;
									value = functionPointer1[1];
									functionId = functionPointer1[3];
									type = 10;
									closure = functionPointer1[4];
									break;
							}
						} else {
							hasInterrupt = EX_InvalidInvocation(ec, "This type cannot be invoked like a function.");
						}
					}
					if (((type == 9) && !hasInterrupt)) {
						// primitive method invocation;
						output = VALUE_NULL;
						primitiveMethodToCoreLibraryFallback = false;
						switch (value[0]) {
							case 5:
								// ...on a string;
								string1 = value[1];
								switch (functionId) {
									case 7:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string contains method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 5)) {
												hasInterrupt = EX_InvalidArgument(ec, "string contains method requires another string as input.");
											} else if ((string1.indexOf(value2[1]) != -1)) {
												output = VALUE_TRUE;
											} else {
												output = VALUE_FALSE;
											}
										}
										break;
									case 9:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string endsWith method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 5)) {
												hasInterrupt = EX_InvalidArgument(ec, "string endsWith method requires another string as input.");
											} else if (PST$stringEndsWith(string1, value2[1])) {
												output = VALUE_TRUE;
											} else {
												output = VALUE_FALSE;
											}
										}
										break;
									case 13:
										if (((argCount < 1) || (argCount > 2))) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string indexOf method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 5)) {
												hasInterrupt = EX_InvalidArgument(ec, "string indexOf method requires another string as input.");
											} else if ((argCount == 1)) {
												output = buildInteger(globals, string1.indexOf(value2[1]));
											} else if ((funcArgs[1][0] != 3)) {
												hasInterrupt = EX_InvalidArgument(ec, "string indexOf method requires an integer as its second argument.");
											} else {
												int1 = funcArgs[1][1];
												if (((int1 < 0) || (int1 >= string1.length))) {
													hasInterrupt = EX_IndexOutOfRange(ec, "String index is out of bounds.");
												} else {
													output = buildInteger(globals, string1.indexOf(value2[1], int1));
												}
											}
										}
										break;
									case 19:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string lower method", 0, argCount));
										} else {
											output = buildString(globals, string1.toLowerCase());
										}
										break;
									case 20:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string ltrim method", 0, argCount));
										} else {
											output = buildString(globals, PST$stringTrimOneSide(string1, true));
										}
										break;
									case 26:
										if ((argCount != 2)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string replace method", 2, argCount));
										} else {
											value2 = funcArgs[0];
											value3 = funcArgs[1];
											if (((value2[0] != 5) || (value3[0] != 5))) {
												hasInterrupt = EX_InvalidArgument(ec, "string replace method requires 2 strings as input.");
											} else {
												output = buildString(globals, string1.split(value2[1]).join(value3[1]));
											}
										}
										break;
									case 27:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string reverse method", 0, argCount));
										} else {
											output = buildString(globals, string1.split('').reverse().join(''));
										}
										break;
									case 28:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string rtrim method", 0, argCount));
										} else {
											output = buildString(globals, PST$stringTrimOneSide(string1, false));
										}
										break;
									case 31:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string split method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 5)) {
												hasInterrupt = EX_InvalidArgument(ec, "string split method requires another string as input.");
											} else {
												stringList = string1.split(value2[1]);
												_len = stringList.length;
												list1 = makeEmptyList(globals[14], _len);
												i = 0;
												while ((i < _len)) {
													list1[2].push(buildString(globals, stringList[i]));
													i += 1;
												}
												list1[1] = _len;
												output = [6, list1];
											}
										}
										break;
									case 32:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string startsWith method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 5)) {
												hasInterrupt = EX_InvalidArgument(ec, "string startsWith method requires another string as input.");
											} else if ((string1.indexOf(value2[1]) == 0)) {
												output = VALUE_TRUE;
											} else {
												output = VALUE_FALSE;
											}
										}
										break;
									case 33:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string trim method", 0, argCount));
										} else {
											output = buildString(globals, string1.trim());
										}
										break;
									case 34:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("string upper method", 0, argCount));
										} else {
											output = buildString(globals, string1.toUpperCase());
										}
										break;
									default:
										output = null;
										break;
								}
								break;
							case 6:
								// ...on a list;
								list1 = value[1];
								switch (functionId) {
									case 0:
										if ((argCount == 0)) {
											hasInterrupt = EX_InvalidArgument(ec, "List add method requires at least one argument.");
										} else {
											intArray1 = list1[0];
											i = 0;
											while ((i < argCount)) {
												value = funcArgs[i];
												if ((intArray1 != null)) {
													value2 = canAssignTypeToGeneric(vm, value, intArray1, 0);
													if ((value2 == null)) {
														hasInterrupt = EX_InvalidArgument(ec, ["Cannot convert a ", typeToStringFromValue(vm, value), " into a ", typeToString(vm, list1[0], 0)].join(''));
													}
													list1[2].push(value2);
												} else {
													list1[2].push(value);
												}
												i += 1;
											}
											list1[1] += argCount;
											output = VALUE_NULL;
										}
										break;
									case 3:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list choice method", 0, argCount));
										} else {
											_len = list1[1];
											if ((_len == 0)) {
												hasInterrupt = EX_UnsupportedOperation(ec, "Cannot use list.choice() method on an empty list.");
											} else {
												i = Math.floor(((Math.random() * _len)));
												output = list1[2][i];
											}
										}
										break;
									case 4:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clear method", 0, argCount));
										} else if ((list1[1] > 0)) {
											PST$clearList(list1[2]);
											list1[1] = 0;
										}
										break;
									case 5:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list clone method", 0, argCount));
										} else {
											_len = list1[1];
											list2 = makeEmptyList(list1[0], _len);
											i = 0;
											while ((i < _len)) {
												list2[2].push(list1[2][i]);
												i += 1;
											}
											list2[1] = _len;
											output = [6, list2];
										}
										break;
									case 6:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list concat method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 6)) {
												hasInterrupt = EX_InvalidArgument(ec, "list concat methods requires a list as an argument.");
											} else {
												list2 = value2[1];
												intArray1 = list1[0];
												if (((intArray1 != null) && !canAssignGenericToGeneric(vm, list2[0], 0, intArray1, 0, intBuffer))) {
													hasInterrupt = EX_InvalidArgument(ec, "Cannot concat a list: incompatible types.");
												} else {
													if (((intArray1 != null) && (intArray1[0] == 4) && (list2[0][0] == 3))) {
														bool1 = true;
													} else {
														bool1 = false;
													}
													_len = list2[1];
													i = 0;
													while ((i < _len)) {
														value = list2[2][i];
														if (bool1) {
															value = buildFloat(globals, (0.0 + value[1]));
														}
														list1[2].push(value);
														i += 1;
													}
													list1[1] += _len;
												}
											}
										}
										break;
									case 7:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list contains method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											_len = list1[1];
											output = VALUE_FALSE;
											i = 0;
											while ((i < _len)) {
												value = list1[2][i];
												if ((doEqualityComparisonAndReturnCode(value2, value) == 1)) {
													output = VALUE_TRUE;
													i = _len;
												}
												i += 1;
											}
										}
										break;
									case 10:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list filter method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 9)) {
												hasInterrupt = EX_InvalidArgument(ec, "list filter method requires a function pointer as its argument.");
											} else {
												primitiveMethodToCoreLibraryFallback = true;
												functionId = metadata[14][0];
												funcArgs[1] = value;
												argCount = 2;
												output = null;
											}
										}
										break;
									case 14:
										if ((argCount != 2)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list insert method", 1, argCount));
										} else {
											value = funcArgs[0];
											value2 = funcArgs[1];
											if ((value[0] != 3)) {
												hasInterrupt = EX_InvalidArgument(ec, "First argument of list.insert must be an integer index.");
											} else {
												intArray1 = list1[0];
												if ((intArray1 != null)) {
													value3 = canAssignTypeToGeneric(vm, value2, intArray1, 0);
													if ((value3 == null)) {
														hasInterrupt = EX_InvalidArgument(ec, "Cannot insert this type into this type of list.");
													}
													value2 = value3;
												}
												if (!hasInterrupt) {
													int1 = value[1];
													_len = list1[1];
													if ((int1 < 0)) {
														int1 += _len;
													}
													if ((int1 == _len)) {
														list1[2].push(value2);
														list1[1] += 1;
													} else if (((int1 < 0) || (int1 >= _len))) {
														hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.");
													} else {
														list1[2].splice(int1, 0, value2);
														list1[1] += 1;
													}
												}
											}
										}
										break;
									case 17:
										if ((argCount != 1)) {
											if ((argCount == 0)) {
												value2 = globals[8];
											} else {
												hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list join method", 1, argCount));
											}
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 5)) {
												hasInterrupt = EX_InvalidArgument(ec, "Argument of list.join needs to be a string.");
											}
										}
										if (!hasInterrupt) {
											stringList1 = [];
											string1 = value2[1];
											_len = list1[1];
											i = 0;
											while ((i < _len)) {
												value = list1[2][i];
												if ((value[0] != 5)) {
													string2 = valueToString(vm, value);
												} else {
													string2 = value[1];
												}
												stringList1.push(string2);
												i += 1;
											}
											output = buildString(globals, stringList1.join(string1));
										}
										break;
									case 21:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list map method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 9)) {
												hasInterrupt = EX_InvalidArgument(ec, "list map method requires a function pointer as its argument.");
											} else {
												primitiveMethodToCoreLibraryFallback = true;
												functionId = metadata[14][1];
												funcArgs[1] = value;
												argCount = 2;
												output = null;
											}
										}
										break;
									case 23:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list pop method", 0, argCount));
										} else {
											_len = list1[1];
											if ((_len < 1)) {
												hasInterrupt = EX_IndexOutOfRange(ec, "Cannot pop from an empty list.");
											} else {
												_len -= 1;
												value = list1[2].pop();
												if (returnValueUsed) {
													output = value;
												}
												list1[1] = _len;
											}
										}
										break;
									case 24:
										if (((argCount == 0) || (argCount > 2))) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list reduce method", 1, argCount));
										} else if ((funcArgs[0][0] != 9)) {
											hasInterrupt = EX_InvalidArgument(ec, "list reduce method requires a function pointer as its argument.");
										} else {
											funcArgs[2] = value;
											funcArgs[3] = VALUE_FALSE;
											if ((argCount == 1)) {
												funcArgs[3] = VALUE_TRUE;
												funcArgs[1] = VALUE_NULL;
											}
											argCount = 4;
											primitiveMethodToCoreLibraryFallback = true;
											functionId = metadata[14][4];
											output = null;
										}
										break;
									case 25:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list remove method", 1, argCount));
										} else {
											value = funcArgs[0];
											if ((value[0] != 3)) {
												hasInterrupt = EX_InvalidArgument(ec, "Argument of list.remove must be an integer index.");
											} else {
												int1 = value[1];
												_len = list1[1];
												if ((int1 < 0)) {
													int1 += _len;
												}
												if (((int1 < 0) || (int1 >= _len))) {
													hasInterrupt = EX_IndexOutOfRange(ec, "Index out of range.");
												} else {
													if (returnValueUsed) {
														output = list1[2][int1];
													}
													_len = (list1[1] - 1);
													list1[1] = _len;
													list1[2].splice(int1, 1);
												}
											}
										}
										break;
									case 27:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list reverse method", 0, argCount));
										} else {
											list1[2].reverse();
										}
										break;
									case 29:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("list shuffle method", 0, argCount));
										} else {
											PST$shuffle(list1[2]);
										}
										break;
									case 30:
										if ((argCount == 0)) {
											sortLists(list1, list1, PST$intBuffer16);
											if ((PST$intBuffer16[0] > 0)) {
												hasInterrupt = EX_InvalidArgument(ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.");
											}
										} else if ((argCount == 1)) {
											value2 = funcArgs[0];
											if ((value2[0] == 9)) {
												primitiveMethodToCoreLibraryFallback = true;
												functionId = metadata[14][2];
												funcArgs[1] = value;
												argCount = 2;
											} else {
												hasInterrupt = EX_InvalidArgument(ec, "list.sort(get_key_function) requires a function pointer as its argument.");
											}
											output = null;
										}
										break;
									default:
										output = null;
										break;
								}
								break;
							case 7:
								// ...on a dictionary;
								dictImpl = value[1];
								switch (functionId) {
									case 4:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clear method", 0, argCount));
										} else if ((dictImpl[0] > 0)) {
											dictImpl[4] = {};
											dictImpl[5] = {};
											PST$clearList(dictImpl[6]);
											PST$clearList(dictImpl[7]);
											dictImpl[0] = 0;
										}
										break;
									case 5:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary clone method", 0, argCount));
										} else {
											output = [7, cloneDictionary(dictImpl, null)];
										}
										break;
									case 7:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary contains method", 1, argCount));
										} else {
											value = funcArgs[0];
											output = VALUE_FALSE;
											if ((value[0] == 5)) {
												if ((dictImpl[5][value[1]] !== undefined)) {
													output = VALUE_TRUE;
												}
											} else {
												bool1 = true;
												if ((value[0] == 3)) {
													i = value[1];
												} else if ((value[0] == 8)) {
													i = (value[1])[1];
												} else {
													bool1 = false;
													output = VALUE_FALSE;
												}
												if ((bool1 && (dictImpl[4][i] !== undefined))) {
													output = VALUE_TRUE;
												}
											}
										}
										break;
									case 11:
										if (((argCount != 1) && (argCount != 2))) {
											hasInterrupt = EX_InvalidArgument(ec, "Dictionary get method requires 1 or 2 arguments.");
										} else {
											value = funcArgs[0];
											switch (value[0]) {
												case 3:
													int1 = value[1];
													i = dictImpl[4][int1];
													if (i === undefined) i = -1;
													break;
												case 8:
													int1 = (value[1])[1];
													i = dictImpl[4][int1];
													if (i === undefined) i = -1;
													break;
												case 5:
													string1 = value[1];
													i = dictImpl[5][string1];
													if (i === undefined) i = -1;
													break;
											}
											if ((i == -1)) {
												if ((argCount == 2)) {
													output = funcArgs[1];
												} else {
													output = VALUE_NULL;
												}
											} else {
												output = dictImpl[7][i];
											}
										}
										break;
									case 18:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary keys method", 0, argCount));
										} else {
											valueList1 = dictImpl[6];
											_len = valueList1.length;
											if ((dictImpl[1] == 8)) {
												intArray1 = PST$createNewArray(2);
												intArray1[0] = 8;
												intArray1[0] = dictImpl[2];
											} else {
												intArray1 = PST$createNewArray(1);
												intArray1[0] = dictImpl[1];
											}
											list1 = makeEmptyList(intArray1, _len);
											i = 0;
											while ((i < _len)) {
												list1[2].push(valueList1[i]);
												i += 1;
											}
											list1[1] = _len;
											output = [6, list1];
										}
										break;
									case 22:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary merge method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											if ((value2[0] != 7)) {
												hasInterrupt = EX_InvalidArgument(ec, "dictionary merge method requires another dictionary as a parameeter.");
											} else {
												dictImpl2 = value2[1];
												if ((dictImpl2[0] > 0)) {
													if ((dictImpl[0] == 0)) {
														value[1] = cloneDictionary(dictImpl2, null);
													} else if ((dictImpl2[1] != dictImpl[1])) {
														hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different key types cannot be merged.");
													} else if (((dictImpl2[1] == 8) && (dictImpl2[2] != dictImpl[2]) && (dictImpl[2] != 0) && !isClassASubclassOf(vm, dictImpl2[2], dictImpl[2]))) {
														hasInterrupt = EX_InvalidKey(ec, "Dictionary key types are incompatible.");
													} else {
														if ((dictImpl[3] == null)) {
														} else if ((dictImpl2[3] == null)) {
															hasInterrupt = EX_InvalidKey(ec, "Dictionaries with different value types cannot be merged.");
														} else if (!canAssignGenericToGeneric(vm, dictImpl2[3], 0, dictImpl[3], 0, intBuffer)) {
															hasInterrupt = EX_InvalidKey(ec, "The dictionary value types are incompatible.");
														}
														if (!hasInterrupt) {
															cloneDictionary(dictImpl2, dictImpl);
														}
													}
												}
												output = VALUE_NULL;
											}
										}
										break;
									case 25:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary remove method", 1, argCount));
										} else {
											value2 = funcArgs[0];
											bool2 = false;
											keyType = dictImpl[1];
											if (((dictImpl[0] > 0) && (keyType == value2[0]))) {
												if ((keyType == 5)) {
													stringKey = value2[1];
													if ((dictImpl[5][stringKey] !== undefined)) {
														i = dictImpl[5][stringKey];
														bool2 = true;
													}
												} else {
													if ((keyType == 3)) {
														intKey = value2[1];
													} else {
														intKey = (value2[1])[1];
													}
													if ((dictImpl[4][intKey] !== undefined)) {
														i = dictImpl[4][intKey];
														bool2 = true;
													}
												}
												if (bool2) {
													_len = (dictImpl[0] - 1);
													dictImpl[0] = _len;
													if ((i == _len)) {
														if ((keyType == 5)) {
															delete dictImpl[5][stringKey];
														} else {
															delete dictImpl[4][intKey];
														}
														dictImpl[6].splice(i, 1);
														dictImpl[7].splice(i, 1);
													} else {
														value = dictImpl[6][_len];
														dictImpl[6][i] = value;
														dictImpl[7][i] = dictImpl[7][_len];
														dictImpl[6].pop();
														dictImpl[7].pop();
														if ((keyType == 5)) {
															delete dictImpl[5][stringKey];
															stringKey = value[1];
															dictImpl[5][stringKey] = i;
														} else {
															delete dictImpl[4][intKey];
															if ((keyType == 3)) {
																intKey = value[1];
															} else {
																intKey = (value[1])[1];
															}
															dictImpl[4][intKey] = i;
														}
													}
												}
											}
											if (!bool2) {
												hasInterrupt = EX_KeyNotFound(ec, "dictionary does not contain the given key.");
											}
										}
										break;
									case 35:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("dictionary values method", 0, argCount));
										} else {
											valueList1 = dictImpl[7];
											_len = valueList1.length;
											list1 = makeEmptyList(dictImpl[3], _len);
											i = 0;
											while ((i < _len)) {
												addToList(list1, valueList1[i]);
												i += 1;
											}
											output = [6, list1];
										}
										break;
									default:
										output = null;
										break;
								}
								break;
							case 9:
								// ...on a function pointer;
								functionPointer1 = value[1];
								switch (functionId) {
									case 1:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMax method", 0, argCount));
										} else {
											functionId = functionPointer1[3];
											functionInfo = metadata[10][functionId];
											output = buildInteger(globals, functionInfo[4]);
										}
										break;
									case 2:
										if ((argCount > 0)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("argCountMin method", 0, argCount));
										} else {
											functionId = functionPointer1[3];
											functionInfo = metadata[10][functionId];
											output = buildInteger(globals, functionInfo[3]);
										}
										break;
									case 12:
										functionInfo = metadata[10][functionPointer1[3]];
										output = buildString(globals, functionInfo[9]);
										break;
									case 15:
										if ((argCount == 1)) {
											funcArgs[1] = funcArgs[0];
										} else if ((argCount == 0)) {
											funcArgs[1] = VALUE_NULL;
										} else {
											hasInterrupt = EX_InvalidArgument(ec, "invoke requires a list of arguments.");
										}
										funcArgs[0] = value;
										argCount = 2;
										primitiveMethodToCoreLibraryFallback = true;
										functionId = metadata[14][3];
										output = null;
										break;
									default:
										output = null;
										break;
								}
								break;
							case 10:
								// ...on a class definition;
								classValue = value[1];
								switch (functionId) {
									case 12:
										classInfo = metadata[9][classValue[1]];
										output = buildString(globals, classInfo[16]);
										break;
									case 16:
										if ((argCount != 1)) {
											hasInterrupt = EX_InvalidArgument(ec, primitiveMethodWrongArgCountError("class isA method", 1, argCount));
										} else {
											int1 = classValue[1];
											value = funcArgs[0];
											if ((value[0] != 10)) {
												hasInterrupt = EX_InvalidArgument(ec, "class isA method requires another class reference.");
											} else {
												classValue = value[1];
												int2 = classValue[1];
												output = VALUE_FALSE;
												if (isClassASubclassOf(vm, int1, int2)) {
													output = VALUE_TRUE;
												}
											}
										}
										break;
									default:
										output = null;
										break;
								}
								break;
						}
						if (!hasInterrupt) {
							if ((output == null)) {
								if (primitiveMethodToCoreLibraryFallback) {
									type = 1;
									bool1 = true;
								} else {
									hasInterrupt = EX_InvalidInvocation(ec, "primitive method not found.");
								}
							} else {
								if (returnValueUsed) {
									if ((valueStackSize == valueStackCapacity)) {
										valueStack = valueStackIncreaseCapacity(ec);
										valueStackCapacity = valueStack.length;
									}
									valueStack[valueStackSize] = output;
									valueStackSize += 1;
								}
								bool1 = false;
							}
						}
					}
					if ((bool1 && !hasInterrupt)) {
						// push a new frame to the stack;
						stack[0] = pc;
						bool1 = false;
						switch (type) {
							case 1:
								// function;
								functionInfo = functionTable[functionId];
								pc = functionInfo[2];
								value = null;
								classId = 0;
								break;
							case 10:
								// lambda;
								pc = functionId;
								functionInfo = metadata[11][functionId];
								value = null;
								classId = 0;
								break;
							case 2:
								// static method;
								functionInfo = functionTable[functionId];
								pc = functionInfo[2];
								value = null;
								classId = 0;
								break;
							case 3:
								// non-static method;
								functionInfo = functionTable[functionId];
								pc = functionInfo[2];
								classId = 0;
								break;
							case 6:
								// constructor;
								vm[5] += 1;
								classInfo = classTable[classId];
								valueArray1 = PST$createNewArray(classInfo[8]);
								i = (valueArray1.length - 1);
								while ((i >= 0)) {
									switch (classInfo[10][i]) {
										case 0:
											valueArray1[i] = classInfo[11][i];
											break;
										case 1:
											break;
										case 2:
											break;
									}
									i -= 1;
								}
								objInstance1 = [classId, vm[5], valueArray1, null, null];
								value = [8, objInstance1];
								functionId = classInfo[7];
								functionInfo = functionTable[functionId];
								pc = functionInfo[2];
								classId = 0;
								if (returnValueUsed) {
									returnValueUsed = false;
									if ((valueStackSize == valueStackCapacity)) {
										valueStack = valueStackIncreaseCapacity(ec);
										valueStackCapacity = valueStack.length;
									}
									valueStack[valueStackSize] = value;
									valueStackSize += 1;
								}
								break;
							case 7:
								// base constructor;
								value = stack[6];
								classInfo = classTable[classId];
								functionId = classInfo[7];
								functionInfo = functionTable[functionId];
								pc = functionInfo[2];
								classId = 0;
								break;
						}
						if (((argCount < functionInfo[3]) || (argCount > functionInfo[4]))) {
							pc = stack[0];
							hasInterrupt = EX_InvalidArgument(ec, "Incorrect number of args were passed to this function.");
						} else {
							int1 = functionInfo[7];
							int2 = stack[3];
							if ((localsStackCapacity <= (int2 + int1))) {
								increaseLocalsStackCapacity(ec, int1);
								localsStack = ec[5];
								localsStackSet = ec[6];
								localsStackCapacity = localsStack.length;
							}
							localsStackSetToken = (ec[7] + 1);
							ec[7] = localsStackSetToken;
							if ((localsStackSetToken > 2000000000)) {
								resetLocalsStackTokens(ec, stack);
								localsStackSetToken = 2;
							}
							localsStackOffset = int2;
							if ((type == 10)) {
								value = closure[-1][0];
							} else {
								closure = null;
							}
							// invoke the function;
							stack = [pc, localsStackSetToken, localsStackOffset, (localsStackOffset + int1), stack, returnValueUsed, value, valueStackSize, 0, (stack[9] + 1), 0, null, closure, null];
							i = 0;
							while ((i < argCount)) {
								int1 = (localsStackOffset + i);
								localsStack[int1] = funcArgs[i];
								localsStackSet[int1] = localsStackSetToken;
								i += 1;
							}
							if ((argCount != functionInfo[3])) {
								int1 = (argCount - functionInfo[3]);
								if ((int1 > 0)) {
									pc += functionInfo[8][int1];
									stack[0] = pc;
								}
							}
							if ((stack[9] > 1000)) {
								hasInterrupt = EX_Fatal(ec, "Stack overflow.");
							}
						}
					}
				}
				break;
			case 13:
				// CAST;
				value = valueStack[(valueStackSize - 1)];
				value2 = canAssignTypeToGeneric(vm, value, row, 0);
				if ((value2 == null)) {
					if (((value[0] == 4) && (row[0] == 3))) {
						if ((row[1] == 1)) {
							float1 = value[1];
							i = Math.floor(float1);
							if ((i < 0)) {
								if ((i > -257)) {
									value2 = globals[10][-i];
								} else {
									value2 = [3, i];
								}
							} else if ((i < 2049)) {
								value2 = globals[9][i];
							} else {
								value2 = [3, i];
							}
						}
					} else if (((value[0] == 3) && (row[0] == 4))) {
						int1 = value[1];
						if ((int1 == 0)) {
							value2 = VALUE_FLOAT_ZERO;
						} else {
							value2 = [4, (0.0 + int1)];
						}
					}
					if ((value2 != null)) {
						valueStack[(valueStackSize - 1)] = value2;
					}
				}
				if ((value2 == null)) {
					hasInterrupt = EX_InvalidArgument(ec, ["Cannot convert a ", typeToStringFromValue(vm, value), " to a ", typeToString(vm, row, 0)].join(''));
				} else {
					valueStack[(valueStackSize - 1)] = value2;
				}
				break;
			case 14:
				// CLASS_DEFINITION;
				initializeClass(pc, vm, row, stringArgs[pc]);
				classTable = metadata[9];
				break;
			case 15:
				// UNUSED_CNI_INVOKE;
				break;
			case 16:
				// UNUSED_CNI_REGISTER;
				break;
			case 17:
				// COMMAND_LINE_ARGS;
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				list1 = makeEmptyList(globals[14], 3);
				i = 0;
				while ((i < vm[11][0].length)) {
					addToList(list1, buildString(globals, vm[11][0][i]));
					i += 1;
				}
				valueStack[valueStackSize] = [6, list1];
				valueStackSize += 1;
				break;
			case 18:
				// CONTINUE;
				if ((row[0] == 1)) {
					pc += row[1];
				} else {
					intArray1 = esfData[pc];
					pc = (intArray1[1] - 1);
					valueStackSize = stack[7];
					stack[10] = 2;
				}
				break;
			case 19:
				// CORE_FUNCTION;
				switch (row[0]) {
					case 1:
						// parseInt;
						arg1 = valueStack[--valueStackSize];
						output = VALUE_NULL;
						if ((arg1[0] == 5)) {
							string1 = (arg1[1]).trim();
							if (PST$is_valid_integer(string1)) {
								output = buildInteger(globals, parseInt(string1));
							}
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "parseInt requires a string argument.");
						}
						break;
					case 2:
						// parseFloat;
						arg1 = valueStack[--valueStackSize];
						output = VALUE_NULL;
						if ((arg1[0] == 5)) {
							string1 = (arg1[1]).trim();
							PST$floatParseHelper(floatList1, string1);
							if ((floatList1[0] >= 0)) {
								output = buildFloat(globals, floatList1[1]);
							}
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "parseFloat requires a string argument.");
						}
						break;
					case 3:
						// print;
						arg1 = valueStack[--valueStackSize];
						output = VALUE_NULL;
						printToStdOut(vm[11][2], valueToString(vm, arg1));
						break;
					case 4:
						// typeof;
						arg1 = valueStack[--valueStackSize];
						output = buildInteger(globals, (arg1[0] - 1));
						break;
					case 5:
						// typeis;
						arg1 = valueStack[--valueStackSize];
						int1 = arg1[0];
						int2 = row[2];
						output = VALUE_FALSE;
						while ((int2 > 0)) {
							if ((row[(2 + int2)] == int1)) {
								output = VALUE_TRUE;
								int2 = 0;
							} else {
								int2 -= 1;
							}
						}
						break;
					case 6:
						// execId;
						output = buildInteger(globals, ec[0]);
						break;
					case 7:
						// assert;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						if ((arg1[0] != 2)) {
							hasInterrupt = EX_InvalidArgument(ec, "Assertion expression must be a boolean.");
						} else if (arg1[1]) {
							output = VALUE_NULL;
						} else {
							string1 = valueToString(vm, arg2);
							if (arg3[1]) {
								string1 = "Assertion failed: " + string1;
							}
							hasInterrupt = EX_AssertionFailed(ec, string1);
						}
						break;
					case 8:
						// chr;
						arg1 = valueStack[--valueStackSize];
						output = null;
						if ((arg1[0] == 3)) {
							int1 = arg1[1];
							if (((int1 >= 0) && (int1 < 256))) {
								output = buildCommonString(globals, String.fromCharCode(int1));
							}
						}
						if ((output == null)) {
							hasInterrupt = EX_InvalidArgument(ec, "chr requires an integer between 0 and 255.");
						}
						break;
					case 9:
						// ord;
						arg1 = valueStack[--valueStackSize];
						output = null;
						if ((arg1[0] == 5)) {
							string1 = arg1[1];
							if ((string1.length == 1)) {
								output = buildInteger(globals, string1.charCodeAt(0));
							}
						}
						if ((output == null)) {
							hasInterrupt = EX_InvalidArgument(ec, "ord requires a 1 character string.");
						}
						break;
					case 10:
						// currentTime;
						output = buildFloat(globals, ((Date.now ? Date.now() : new Date().getTime()) / 1000.0));
						break;
					case 11:
						// sortList;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = VALUE_NULL;
						list1 = arg1[1];
						list2 = arg2[1];
						sortLists(list2, list1, PST$intBuffer16);
						if ((PST$intBuffer16[0] > 0)) {
							hasInterrupt = EX_InvalidArgument(ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.");
						}
						break;
					case 12:
						// abs;
						arg1 = valueStack[--valueStackSize];
						output = arg1;
						if ((arg1[0] == 3)) {
							if ((arg1[1] < 0)) {
								output = buildInteger(globals, -arg1[1]);
							}
						} else if ((arg1[0] == 4)) {
							if ((arg1[1] < 0)) {
								output = buildFloat(globals, -arg1[1]);
							}
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "abs requires a number as input.");
						}
						break;
					case 13:
						// arcCos;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number as input.");
						}
						if (!hasInterrupt) {
							if (((float1 < -1) || (float1 > 1))) {
								hasInterrupt = EX_InvalidArgument(ec, "arccos requires a number in the range of -1 to 1.");
							} else {
								output = buildFloat(globals, Math.acos(float1));
							}
						}
						break;
					case 14:
						// arcSin;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number as input.");
						}
						if (!hasInterrupt) {
							if (((float1 < -1) || (float1 > 1))) {
								hasInterrupt = EX_InvalidArgument(ec, "arcsin requires a number in the range of -1 to 1.");
							} else {
								output = buildFloat(globals, Math.asin(float1));
							}
						}
						break;
					case 15:
						// arcTan;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						bool1 = false;
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							bool1 = true;
						}
						if ((arg2[0] == 4)) {
							float2 = arg2[1];
						} else if ((arg2[0] == 3)) {
							float2 = (0.0 + arg2[1]);
						} else {
							bool1 = true;
						}
						if (bool1) {
							hasInterrupt = EX_InvalidArgument(ec, "arctan requires numeric arguments.");
						} else {
							output = buildFloat(globals, Math.atan2(float1, float2));
						}
						break;
					case 16:
						// cos;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
							output = buildFloat(globals, Math.cos(float1));
						} else if ((arg1[0] == 3)) {
							int1 = arg1[1];
							output = buildFloat(globals, Math.cos(int1));
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "cos requires a number argument.");
						}
						break;
					case 17:
						// ensureRange;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						bool1 = false;
						if ((arg2[0] == 4)) {
							float2 = arg2[1];
						} else if ((arg2[0] == 3)) {
							float2 = (0.0 + arg2[1]);
						} else {
							bool1 = true;
						}
						if ((arg3[0] == 4)) {
							float3 = arg3[1];
						} else if ((arg3[0] == 3)) {
							float3 = (0.0 + arg3[1]);
						} else {
							bool1 = true;
						}
						if ((!bool1 && (float3 < float2))) {
							float1 = float3;
							float3 = float2;
							float2 = float1;
							value = arg2;
							arg2 = arg3;
							arg3 = value;
						}
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							bool1 = true;
						}
						if (bool1) {
							hasInterrupt = EX_InvalidArgument(ec, "ensureRange requires numeric arguments.");
						} else if ((float1 < float2)) {
							output = arg2;
						} else if ((float1 > float3)) {
							output = arg3;
						} else {
							output = arg1;
						}
						break;
					case 18:
						// floor;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
							int1 = Math.floor(float1);
							if ((int1 < 2049)) {
								if ((int1 >= 0)) {
									output = INTEGER_POSITIVE_CACHE[int1];
								} else if ((int1 > -257)) {
									output = INTEGER_NEGATIVE_CACHE[-int1];
								} else {
									output = [3, int1];
								}
							} else {
								output = [3, int1];
							}
						} else if ((arg1[0] == 3)) {
							output = arg1;
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "floor expects a numeric argument.");
						}
						break;
					case 19:
						// max;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						bool1 = false;
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							bool1 = true;
						}
						if ((arg2[0] == 4)) {
							float2 = arg2[1];
						} else if ((arg2[0] == 3)) {
							float2 = (0.0 + arg2[1]);
						} else {
							bool1 = true;
						}
						if (bool1) {
							hasInterrupt = EX_InvalidArgument(ec, "max requires numeric arguments.");
						} else if ((float1 >= float2)) {
							output = arg1;
						} else {
							output = arg2;
						}
						break;
					case 20:
						// min;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						bool1 = false;
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							bool1 = true;
						}
						if ((arg2[0] == 4)) {
							float2 = arg2[1];
						} else if ((arg2[0] == 3)) {
							float2 = (0.0 + arg2[1]);
						} else {
							bool1 = true;
						}
						if (bool1) {
							hasInterrupt = EX_InvalidArgument(ec, "min requires numeric arguments.");
						} else if ((float1 <= float2)) {
							output = arg1;
						} else {
							output = arg2;
						}
						break;
					case 23:
						// sign;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 3)) {
							float1 = (0.0 + (arg1[1]));
						} else if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "sign requires a number as input.");
						}
						if ((float1 == 0)) {
							output = VALUE_INT_ZERO;
						} else if ((float1 > 0)) {
							output = VALUE_INT_ONE;
						} else {
							output = INTEGER_NEGATIVE_CACHE[1];
						}
						break;
					case 24:
						// sin;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "sin requires a number argument.");
						}
						output = buildFloat(globals, Math.sin(float1));
						break;
					case 25:
						// tan;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "tan requires a number argument.");
						}
						if (!hasInterrupt) {
							float2 = Math.cos(float1);
							if ((float2 < 0)) {
								float2 = -float2;
							}
							if ((float2 < 0.00000000015)) {
								hasInterrupt = EX_DivisionByZero(ec, "Tangent is undefined.");
							} else {
								output = buildFloat(globals, Math.tan(float1));
							}
						}
						break;
					case 26:
						// log;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						if ((arg1[0] == 4)) {
							float1 = arg1[1];
						} else if ((arg1[0] == 3)) {
							float1 = (0.0 + arg1[1]);
						} else {
							hasInterrupt = EX_InvalidArgument(ec, "logarithms require a number argument.");
						}
						if (!hasInterrupt) {
							if ((float1 <= 0)) {
								hasInterrupt = EX_InvalidArgument(ec, "logarithms require positive inputs.");
							} else {
								output = buildFloat(globals, fixFuzzyFloatPrecision((Math.log(float1) * arg2[1])));
							}
						}
						break;
					case 30:
						// sleep;
						arg1 = valueStack[--valueStackSize];
						float1 = getFloat(arg1);
						if ((row[1] == 1)) {
							if ((valueStackSize == valueStackCapacity)) {
								valueStack = valueStackIncreaseCapacity(ec);
								valueStackCapacity = valueStack.length;
							}
							valueStack[valueStackSize] = VALUE_NULL;
							valueStackSize += 1;
						}
						prepareToSuspend(ec, stack, valueStackSize, pc);
						ec[13] = [3, 0, "", float1, null];
						hasInterrupt = true;
						break;
					case 31:
						// projectId;
						output = buildCommonString(globals, metadata[16]);
						break;
					case 34:
						// allocNativeData;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						objInstance1 = arg1[1];
						int1 = arg2[1];
						objArray1 = PST$createNewArray(int1);
						objInstance1[3] = objArray1;
						break;
					case 35:
						// setNativeData;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						(arg1[1])[3][arg2[1]] = arg3[1];
						break;
					case 36:
						// getExceptionTrace;
						arg1 = valueStack[--valueStackSize];
						intList1 = getNativeDataItem(arg1, 1);
						list1 = makeEmptyList(globals[14], 20);
						output = [6, list1];
						if ((intList1 != null)) {
							stringList1 = tokenHelperConvertPcsToStackTraceStrings(vm, intList1);
							i = 0;
							while ((i < stringList1.length)) {
								addToList(list1, buildString(globals, stringList1[i]));
								i += 1;
							}
							reverseList(list1);
						}
						break;
					case 37:
						// reflectAllClasses;
						output = Reflect_allClasses(vm);
						break;
					case 38:
						// reflectGetMethods;
						arg1 = valueStack[--valueStackSize];
						output = Reflect_getMethods(vm, ec, arg1);
						hasInterrupt = (ec[13] != null);
						break;
					case 39:
						// reflectGetClass;
						arg1 = valueStack[--valueStackSize];
						if ((arg1[0] != 8)) {
							hasInterrupt = EX_InvalidArgument(ec, "Cannot get class from non-instance types.");
						} else {
							objInstance1 = arg1[1];
							output = [10, [false, objInstance1[0]]];
						}
						break;
					case 40:
						// convertFloatArgsToInts;
						int1 = stack[3];
						i = localsStackOffset;
						while ((i < int1)) {
							value = localsStack[i];
							if ((localsStackSet[i] != localsStackSetToken)) {
								i += int1;
							} else if ((value[0] == 4)) {
								float1 = value[1];
								int2 = Math.floor(float1);
								if (((int2 >= 0) && (int2 < 2049))) {
									localsStack[i] = INTEGER_POSITIVE_CACHE[int2];
								} else {
									localsStack[i] = buildInteger(globals, int2);
								}
							}
							i += 1;
						}
						break;
					case 42:
						// nativeTunnelSend;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						objArray1 = PST$createNewArray(2);
						objArray1[0] = arg1[1];
						objArray1[1] = arg2[1];
						obj1 = (PST$extCallbacks["nativeTunnelSend"] || ((o) => null))(objArray1);
						int1 = 0;
						if ((obj1 != null)) {
							int1 = obj1;
						}
						output = buildInteger(globals, int1);
						break;
					case 43:
						// nativeTunnelRecv;
						arg1 = valueStack[--valueStackSize];
						list1 = arg1[1];
						objArray1 = PST$createNewArray(4);
						objArray1[3] = false;
						obj1 = (PST$extCallbacks["nativeTunnelRecv"] || ((o) => null))(objArray1);
						bool1 = false;
						if ((obj1 != null)) {
							bool1 = obj1;
						}
						output = buildBoolean(globals, bool1);
						if (bool1) {
							value = buildInteger(globals, objArray1[0]);
							value2 = buildInteger(globals, objArray1[1]);
							value3 = buildString(globals, objArray1[2]);
							rightValue = buildBoolean(globals, objArray1[3]);
							list1[2][0] = value;
							list1[2][1] = value2;
							list1[2][2] = value3;
							list1[2][3] = rightValue;
						}
						break;
					case 45:
						// ipcNamedPipeCreate;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						string1 = IpcNamedPipe_create(arg1, arg2);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 46:
						// ipcNamedPipeSend;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						string1 = IpcNamedPipe_send(arg1, arg2);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 47:
						// ipcNamedPipeFlush;
						arg1 = valueStack[--valueStackSize];
						string1 = IpcNamedPipe_flush(arg1);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 48:
						// randomFloat;
						output = [4, Math.random()];
						break;
					case 49:
						// randomInt;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						if (((arg1[0] != 3) || (arg2[0] != 3))) {
							output = vm[14];
						} else {
							int1 = arg1[1];
							int2 = arg2[1];
							if ((int1 >= int2)) {
								output = vm[14];
							} else {
								int3 = Math.floor(((Math.random() * (int2 - int1))));
								output = buildInteger(vm[13], (int1 + int3));
							}
						}
						break;
					case 50:
						// resourceGetManifest;
						output = buildList(vm[9][2]);
						break;
					case 51:
						// resourceGetText;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						COMMON.res.readText(vm[11][5], arg1[1], arg2[1], arg3);
						output = vm[14];
						break;
					case 52:
						// environmentGetVariable;
						arg1 = valueStack[--valueStackSize];
						string1 = null;
						if ((string1 != null)) {
							output = buildString(globals, string1);
						}
						break;
					case 53:
						// srandomPopulateQueue;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = buildInteger(globals, SRandomQueuePopulate(globals, arg1[1], arg2[1], arg3[1]));
						break;
					case 54:
						// dateTimeGetUtcOffsetAt;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = DateTime_getUtcOffsetAt(vm, arg1, arg2);
						break;
					case 55:
						// dateTimeInitTimeZone;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = DateTime_initTimeZone(vm, arg1, arg2, arg3);
						break;
					case 56:
						// dateTimeInitTimeZoneList;
						arg1 = valueStack[--valueStackSize];
						output = DateTime_initTimeZoneList(vm, arg1);
						break;
					case 57:
						// dateTimeIsDstOccurringAt;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = DateTime_isDstOccurringAt(vm, arg1, arg2);
						break;
					case 58:
						// dateTimeParseDate;
						valueStackSize -= 7;
						output = DateTime_parseDate(vm, valueStack[valueStackSize], valueStack[(valueStackSize + 1)], valueStack[(valueStackSize + 2)], valueStack[(valueStackSize + 3)], valueStack[(valueStackSize + 4)], valueStack[(valueStackSize + 5)], valueStack[(valueStackSize + 6)]);
						break;
					case 59:
						// dateTimeUnixToStructured;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = DateTime_unixToStructured(vm, arg1, arg2);
						break;
					case 60:
						// ipcNamedPipeServerCreate;
						valueStackSize -= 5;
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						string1 = IpcNamedPipeServer_create(vm, arg1, arg2, arg3, arg4, arg5);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 61:
						// ipcNamedPipeServerClose;
						arg1 = valueStack[--valueStackSize];
						string1 = IpcNamedPipeServer_close(arg1);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 63:
						// jsInteropInvoke;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						if ((arg3[0] == 1)) {
							arg3 = null;
						}
						output = buildBoolean(globals, COMMON.interop.invoke(arg1[1], arg2[1], arg3));
						break;
					case 64:
						// jsInteropRegisterCallback;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						COMMON.interop.registerCallback(arg1[1], arg2[1], arg3);
						break;
					case 65:
						// jsInteropCallbackReturn;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						COMMON.interop.callbackReturn(arg1[1], arg2[1]);
						break;
					case 66:
						// imageCreate;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_ImageCreate(arg1[1], arg2[1], arg3[1]);
						output = VALUE_NULL;
						break;
					case 67:
						// imageGetPixel;
						valueStackSize -= 5;
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						objInstance1 = null;
						if ((arg2[0] != 1)) {
							objInstance1 = arg2[1];
						}
						int1 = ImageHelper_GetPixel(globals[9], arg1[1], objInstance1, arg3, arg4, arg5[1], intBuffer);
						output = globals[9][int1];
						break;
					case 68:
						// imageSetPixel;
						valueStackSize -= 9;
						arg9 = valueStack[(valueStackSize + 8)];
						arg8 = valueStack[(valueStackSize + 7)];
						arg7 = valueStack[(valueStackSize + 6)];
						arg6 = valueStack[(valueStackSize + 5)];
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						int1 = ImageHelper_SetPixel(arg1[1], arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
						output = globals[9][int1];
						break;
					case 69:
						// imageScale;
						valueStackSize -= 5;
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_Scale(arg1[1], arg2[1], arg3[1], arg4[1], arg5[1]);
						output = VALUE_NULL;
						break;
					case 70:
						// imageSessionStart;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_SessionStart(arg1[1], arg2[1]);
						output = VALUE_NULL;
						break;
					case 71:
						// imageSessionFinish;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_SessionFinish(arg1[1], arg2[1]);
						output = VALUE_NULL;
						break;
					case 72:
						// imageBlit;
						valueStackSize -= 10;
						arg10 = valueStack[(valueStackSize + 9)];
						arg9 = valueStack[(valueStackSize + 8)];
						arg8 = valueStack[(valueStackSize + 7)];
						arg7 = valueStack[(valueStackSize + 6)];
						arg6 = valueStack[(valueStackSize + 5)];
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_ImageBlit(arg1[1], arg2[1], arg3[1], arg4[1], arg5[1], arg6[1], arg7[1], arg8[1], arg9[1], arg10[1]);
						output = VALUE_NULL;
						break;
					case 73:
						// imageAtlasManifest;
						output = buildString(globals, vm[9][3]);
						break;
					case 74:
						// imageLoadChunk;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_LoadChunk(vm, arg1[1], arg2[1], arg3[1], arg4);
						output = VALUE_NULL;
						break;
					case 75:
						// imageGetChunkSync;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						ImageHelper_GetChunkSync(arg1[1], arg2[1], arg3[1]);
						output = VALUE_NULL;
						break;
					case 76:
						// makeByteList;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = buildBoolean(globals, makeByteListNativeData(arg1[1], arg2));
						break;
					case 77:
						// bytesObjToList;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						byteObjToList(globals[9], arg1[1], arg2[1]);
						output = VALUE_NULL;
						break;
					case 78:
						// httpSend;
						valueStackSize -= 9;
						arg9 = valueStack[(valueStackSize + 8)];
						arg8 = valueStack[(valueStackSize + 7)];
						arg7 = valueStack[(valueStackSize + 6)];
						arg6 = valueStack[(valueStackSize + 5)];
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						intArray1 = null;
						string1 = null;
						int1 = arg5[1];
						if ((int1 == 1)) {
							string1 = arg7[1];
						} else if ((int1 == 2)) {
							intArray1 = (arg7[1])[3][0];
						}
						list1 = arg8[1];
						stringList = PST$createNewArray(list1[1]);
						i = 0;
						while ((i < list1[1])) {
							stringList[i] = list1[2][i][1];
							i += 1;
						}
						objInstance1 = arg9[1];
						objInstance1[3] = PST$createNewArray(1);
						COMMON.httpSend(vm, arg1, arg2, arg3[1], arg4[1], arg6[1], intArray1, string1, stringList, arg9, objInstance1[3]);
						output = VALUE_NULL;
						break;
					case 79:
						// imageFromBytes;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = buildInteger(globals, ImageHelper_fromBytes(globals, arg1[1], true, arg2, arg3[1], arg4));
						break;
					case 80:
						// imageB64BytesPreferred;
						output = buildBoolean(globals, true);
						break;
					case 81:
						// imageEncode;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						value = ImageHelper_ImageEncode(globals, (arg1[1])[3][0], arg2[1]);
						valueList1 = [];
						valueList1.push(buildBoolean(globals, (value[0] == 5)));
						valueList1.push(value);
						output = buildList(valueList1);
						break;
					case 82:
						// argVerifyIntRange;
						_len = valueStack[(valueStackSize - 1)][1];
						int1 = (valueStackSize - _len - 1);
						int2 = valueStack[(int1 - 2)][1];
						int3 = valueStack[(int1 - 1)][1];
						valueStackSize = (int1 - 3);
						bool1 = false;
						i = 0;
						while ((i < _len)) {
							value = valueStack[(int1 + i)];
							if ((value[0] != 3)) {
								bool1 = true;
							} else {
								j = value[1];
								if (((j < int2) || (j > int3))) {
									bool1 = true;
								}
							}
							i += 1;
						}
						if (bool1) {
							hasInterrupt = EX_InvalidArgument(ec, valueStack[valueStackSize][1]);
						}
						break;
					case 83:
						// argVerifyNums;
						arg1 = valueStack[--valueStackSize];
						_len = arg1[1];
						int1 = (valueStackSize - _len);
						valueStackSize = (int1 - 1);
						i = 0;
						while ((i < _len)) {
							value = valueStack[(int1 + i)];
							if (((value[0] != 3) && (value[0] != 4))) {
								i += _len;
								hasInterrupt = EX_InvalidArgument(ec, valueStack[valueStackSize][1]);
							}
							i += 1;
						}
						break;
					case 84:
						// xmlParse;
						arg1 = valueStack[--valueStackSize];
						output = xml_parse(vm, arg1[1]);
						break;
					case 85:
						// launchBrowser;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						COMMON.launchBrowser(arg1[1], arg2[1], arg3[1]);
						output = vm[14];
						break;
					case 86:
						// cryptoDigest;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = crypto_digest(globals, arg1[1], arg2[1]);
						break;
					case 87:
						// bytesToText;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = textencoding_convertBytesToText(vm, arg1, arg2, arg3);
						break;
					case 88:
						// textToBytes;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = textencoding_convertTextToBytes(vm, arg1, arg2, arg3, arg4);
						break;
					case 89:
						// jsonSerialize;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = JsonHelper_serialize(intBuffer, vm, arg1, arg2[1], arg3[1], arg4[1]);
						if ((intBuffer[0] == 1)) {
							hasInterrupt = EX_InvalidArgument(ec, output[1]);
						}
						break;
					case 90:
						// environmentDescriptor;
						output = buildString(globals, COMMON.environment.descriptor);
						break;
					case 91:
						// jsonParse;
						arg1 = valueStack[--valueStackSize];
						output = C$common$parseJson(globals, arg1[1]);
						if ((output == null)) {
							output = VALUE_NULL;
						}
						break;
					case 92:
						// processRun;
						valueStackSize -= 6;
						arg6 = valueStack[(valueStackSize + 5)];
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = buildInteger(globals, ProcessHelper_processRun(vm, arg1[1], arg2[1], arg3[1], arg4, arg5[1], arg6[1]));
						break;
					case 93:
						// processKill;
						arg1 = valueStack[--valueStackSize];
						output = buildBoolean(globals, ProcessHelper_kill(arg1[1]));
						break;
					case 94:
						// timedCallback;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						if ((arg2[0] == 3)) {
							float1 = arg2[1];
						} else {
							float1 = arg2[1];
						}
						COMMON.timedCallback(vm, arg1, float1);
						break;
					case 95:
						// diskGetUserDirectory;
						output = lib_fileiocommon_getUserDirectory(vm);
						break;
					case 96:
						// diskInitializeDisk;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_initializeDisk(vm, arg1, arg2);
						break;
					case 97:
						// diskGetCurrentDirectory;
						output = lib_fileiocommon_getCurrentDirectory(vm);
						break;
					case 98:
						// diskFileInfo;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_fileInfo(vm, arg1, arg2, arg3, arg4);
						break;
					case 99:
						// diskFileWrite;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_fileWrite(vm, arg1, arg2, arg3, arg4);
						break;
					case 100:
						// diskFileRead;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_fileRead(vm, arg1, arg2, arg3, arg4);
						break;
					case 101:
						// diskFileDelete;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_fileDelete(vm, arg1, arg2);
						break;
					case 102:
						// diskFileMove;
						valueStackSize -= 5;
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_fileMove(vm, arg1, arg2, arg3, arg4, arg5);
						break;
					case 103:
						// diskDirectoryList;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_directoryList(vm, arg1, arg2, arg3, arg4);
						break;
					case 104:
						// diskDirectoryCreate;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_directoryCreate(vm, arg1, arg2, arg3);
						break;
					case 105:
						// diskDirectoryDelete;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_directoryDelete(vm, arg1, arg2);
						break;
					case 106:
						// diskDirectoryMove;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = lib_fileiocommon_directoryMove(vm, arg1, arg2, arg3);
						break;
					case 107:
						// processCurrentId;
						output = buildInteger(globals, 0);
						break;
					case 108:
						// processList;
						output = buildString(globals, "");
						break;
					case 109:
						// cookieGet;
						arg1 = valueStack[--valueStackSize];
						string1 = C$cookie$get(arg1[1]);
						if ((string1 == null)) {
							output = VALUE_NULL;
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 110:
						// cookieSet;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						C$cookie$set(arg1[1], arg2[1], arg3[1]);
						output = VALUE_NULL;
						break;
					case 111:
						// valueToFloatOrNull;
						arg1 = valueStack[--valueStackSize];
						int1 = arg1[0];
						if ((int1 == 4)) {
							output = arg1;
						} else if ((int1 == 3)) {
							int2 = arg1[1];
							if ((int2 == 0)) {
								output = globals[6];
							} else if ((int2 == 1)) {
								output = globals[7];
							} else {
								output = [4, (int2 + 0.0)];
							}
						} else {
							output = VALUE_NULL;
						}
						break;
					case 112:
						// browserInteropGetUrlPath;
						output = VALUE_NULL;
						string1 = COMMON.getUrlPath();
						if ((string1 != null)) {
							output = buildString(globals, string1);
						}
						break;
					case 113:
						// browserInteropSetUrlPath;
						arg1 = valueStack[--valueStackSize];
						output = VALUE_NULL;
						COMMON.setUrlPath(arg1[1]);
						break;
					case 114:
						// base64FromBytes;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = VALUE_NULL;
						if (Base64Helper_bytesToB64(arg1, arg2[1], arg3[1])) {
							output = arg2;
						}
						break;
					case 115:
						// environmentIsMobile;
						output = buildBoolean(globals, COMMON.environment.isMobile);
						break;
					case 116:
						// ipcUnixSocketClientCreate;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						string1 = IpcUnixSocketClient_create(vm, arg1[1], arg2[1], arg3, arg4);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 117:
						// ipcUnixSocketServerCreate;
						valueStackSize -= 3;
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						string1 = IpcUnixSocketServer_create(vm, arg1[1], arg2[1], arg3);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 118:
						// ipcUnixSocketClientSend;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						string1 = IpcUnixSocketClient_send((arg1[1])[3][0], arg2[1]);
						if ((string1 == null)) {
							output = globals[0];
						} else {
							output = buildString(globals, string1);
						}
						break;
					case 119:
						// ipcUnixSocketServerDisconnect;
						arg1 = valueStack[--valueStackSize];
						IpcUnixSocketServer_disconnect((arg1[1])[3][0]);
						output = VALUE_NULL;
						break;
					case 120:
						// reflectNamespaceFunctions;
						valueStackSize -= 2;
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						bool1 = Reflect_getNamespaceFunctions(vm, arg1[1], arg2[1]);
						output = buildBoolean(globals, bool1);
						break;
					case 121:
						// waxSend;
						valueStackSize -= 4;
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						output = vm[14];
						prepareToSuspend(ec, stack, valueStackSize, pc);
						ec[13] = [3, 0, "", 0.0, null];
						hasInterrupt = true;
						C$wax$send(vm[11][4], vm[11][6], arg4[1], arg1[1], arg2[1], arg3);
						break;
					case 122:
						// waxServiceGetPayload;
						output = buildString(globals, vm[11][7]);
						break;
					case 123:
						// waxServiceSendResponse;
						arg1 = valueStack[--valueStackSize];
						vm[11][8] = arg1[1];
						break;
					case 124:
						// resourceGetBytes;
						valueStackSize -= 5;
						arg5 = valueStack[(valueStackSize + 4)];
						arg4 = valueStack[(valueStackSize + 3)];
						arg3 = valueStack[(valueStackSize + 2)];
						arg2 = valueStack[(valueStackSize + 1)];
						arg1 = valueStack[valueStackSize];
						COMMON.res.readBytes(vm[11][5], arg1[1], arg2[1], arg3[1], [], globals[9], arg5);
						output = VALUE_NULL;
						break;
					case 125:
						// execResume;
						arg1 = valueStack[--valueStackSize];
						C$common$queueExecContext(vm[11][4], arg1[1]);
						break;
					case 126:
						// execSuspend;
						prepareToSuspend(ec, stack, valueStackSize, pc);
						ec[13] = [6, 0, "", 0.0, null];
						hasInterrupt = true;
						break;
					case 127:
						// vmId;
						output = buildInteger(globals, vm[17]);
						break;
				}
				if ((row[1] == 1)) {
					if ((valueStackSize == valueStackCapacity)) {
						valueStack = valueStackIncreaseCapacity(ec);
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize] = output;
					valueStackSize += 1;
				}
				break;
			case 20:
				// DEBUG_SYMBOLS;
				applyDebugSymbolData(vm, row, stringArgs[pc], metadata[21]);
				break;
			case 21:
				// DEF_DICT;
				intIntDict1 = {};
				stringIntDict1 = {};
				valueList2 = [];
				valueList1 = [];
				_len = row[0];
				type = 3;
				first = true;
				i = 0;
				valueStackSize -= (2 * _len);
				while (((i < _len) && !hasInterrupt)) {
					value = valueStack[(valueStackSize + (2 * i) + 1)];
					value2 = valueStack[(valueStackSize + (2 * i))];
					if (first) {
						type = value2[0];
						first = false;
					} else if ((type != value2[0])) {
						hasInterrupt = EX_InvalidKey(ec, "Dictionary keys must be of the same type.");
					}
					if (!hasInterrupt) {
						if ((type == 3)) {
							intKey = value2[1];
						} else if ((type == 5)) {
							stringKey = value2[1];
						} else if ((type == 8)) {
							objInstance1 = value2[1];
							intKey = objInstance1[1];
						} else {
							hasInterrupt = EX_InvalidKey(ec, "Only integers, strings, and objects can be used as dictionary keys.");
						}
					}
					if (!hasInterrupt) {
						if ((type == 5)) {
							stringIntDict1[stringKey] = valueList1.length;
						} else {
							intIntDict1[intKey] = valueList1.length;
						}
						valueList2.push(value2);
						valueList1.push(value);
						i += 1;
					}
				}
				if (!hasInterrupt) {
					if ((type == 5)) {
						i = Object.keys(stringIntDict1).length;
					} else {
						i = Object.keys(intIntDict1).length;
					}
					if ((i != _len)) {
						hasInterrupt = EX_InvalidKey(ec, "Key collision");
					}
				}
				if (!hasInterrupt) {
					i = row[1];
					classId = 0;
					if ((i > 0)) {
						type = row[2];
						if ((type == 8)) {
							classId = row[3];
						}
						int1 = row.length;
						intArray1 = PST$createNewArray((int1 - i));
						while ((i < int1)) {
							intArray1[(i - row[1])] = row[i];
							i += 1;
						}
					} else {
						intArray1 = null;
					}
					if ((valueStackSize == valueStackCapacity)) {
						valueStack = valueStackIncreaseCapacity(ec);
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize] = [7, [_len, type, classId, intArray1, intIntDict1, stringIntDict1, valueList2, valueList1]];
					valueStackSize += 1;
				}
				break;
			case 22:
				// DEF_LIST;
				int1 = row[0];
				list1 = makeEmptyList(null, int1);
				if ((row[1] != 0)) {
					list1[0] = PST$createNewArray((row.length - 1));
					i = 1;
					while ((i < row.length)) {
						list1[0][(i - 1)] = row[i];
						i += 1;
					}
				}
				list1[1] = int1;
				while ((int1 > 0)) {
					valueStackSize -= 1;
					list1[2].push(valueStack[valueStackSize]);
					int1 -= 1;
				}
				list1[2].reverse();
				value = [6, list1];
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				valueStack[valueStackSize] = value;
				valueStackSize += 1;
				break;
			case 23:
				// DEF_ORIGINAL_CODE;
				defOriginalCodeImpl(vm, row, stringArgs[pc]);
				break;
			case 24:
				// DEREF_CLOSURE;
				bool1 = true;
				closure = stack[12];
				i = row[0];
				if (((closure != null) && (closure[i] !== undefined))) {
					value = closure[i][0];
					if ((value != null)) {
						bool1 = false;
						if ((valueStackSize == valueStackCapacity)) {
							valueStack = valueStackIncreaseCapacity(ec);
							valueStackCapacity = valueStack.length;
						}
						valueStack[valueStackSize++] = value;
					}
				}
				if (bool1) {
					hasInterrupt = EX_UnassignedVariable(ec, "Variable used before it was set.");
				}
				break;
			case 25:
				// DEREF_DOT;
				value = valueStack[(valueStackSize - 1)];
				nameId = row[0];
				int2 = row[1];
				switch (value[0]) {
					case 8:
						objInstance1 = value[1];
						classId = objInstance1[0];
						classInfo = classTable[classId];
						if ((classId == row[4])) {
							int1 = row[5];
						} else {
							intIntDict1 = classInfo[14];
							int1 = intIntDict1[int2];
							if (int1 === undefined) int1 = -1;
							if ((int1 != -1)) {
								int3 = classInfo[12][int1];
								if ((int3 > 1)) {
									if ((int3 == 2)) {
										if ((classId != row[2])) {
											int1 = -2;
										}
									} else {
										if (((int3 == 3) || (int3 == 5))) {
											if ((classInfo[3] != row[3])) {
												int1 = -3;
											}
										}
										if (((int3 == 4) || (int3 == 5))) {
											i = row[2];
											if ((classId == i)) {
											} else {
												classInfo = classTable[classInfo[0]];
												while (((classInfo[2] != -1) && (int1 < classTable[classInfo[2]][12].length))) {
													classInfo = classTable[classInfo[2]];
												}
												j = classInfo[0];
												if ((j != i)) {
													bool1 = false;
													while (((i != -1) && (classTable[i][2] != -1))) {
														i = classTable[i][2];
														if ((i == j)) {
															bool1 = true;
															i = -1;
														}
													}
													if (!bool1) {
														int1 = -4;
													}
												}
											}
											classInfo = classTable[classId];
										}
									}
								}
								row[4] = objInstance1[0];
								row[5] = int1;
							}
						}
						if ((int1 > -1)) {
							functionId = classInfo[9][int1];
							if ((functionId == -1)) {
								output = objInstance1[2][int1];
							} else {
								output = [9, [2, value, objInstance1[0], functionId, null]];
							}
						} else {
							output = null;
						}
						break;
					case 5:
						if ((metadata[13] == nameId)) {
							output = buildInteger(globals, (value[1]).length);
						} else {
							output = null;
						}
						break;
					case 6:
						if ((metadata[13] == nameId)) {
							output = buildInteger(globals, (value[1])[1]);
						} else {
							output = null;
						}
						break;
					case 7:
						if ((metadata[13] == nameId)) {
							output = buildInteger(globals, (value[1])[0]);
						} else {
							output = null;
						}
						break;
					default:
						if ((value[0] == 1)) {
							hasInterrupt = EX_NullReference(ec, "Derferenced a field from null.");
							output = VALUE_NULL;
						} else {
							output = null;
						}
						break;
				}
				if ((output == null)) {
					output = generatePrimitiveMethodReference(globalNameIdToPrimitiveMethodName, nameId, value);
					if ((output == null)) {
						if ((value[0] == 1)) {
							hasInterrupt = EX_NullReference(ec, "Tried to dereference a field on null.");
						} else if (((value[0] == 8) && (int1 < -1))) {
							string1 = identifiers[row[0]];
							if ((int1 == -2)) {
								string2 = "private";
							} else if ((int1 == -3)) {
								string2 = "internal";
							} else {
								string2 = "protected";
							}
							hasInterrupt = EX_UnknownField(ec, ["The field '", string1, "' is marked as ", string2, " and cannot be accessed from here."].join(''));
						} else {
							if ((value[0] == 8)) {
								classId = (value[1])[0];
								classInfo = classTable[classId];
								string1 = classInfo[16] + " instance";
							} else {
								string1 = getTypeFromId(value[0]);
							}
							hasInterrupt = EX_UnknownField(ec, [string1, " does not have a field called '", vm[4][0][row[0]], "'."].join(''));
						}
					}
				}
				valueStack[(valueStackSize - 1)] = output;
				break;
			case 26:
				// DEREF_INSTANCE_FIELD;
				objInstance1 = stack[6][1];
				value = objInstance1[2][row[0]];
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				valueStack[valueStackSize++] = value;
				break;
			case 27:
				// DEREF_STATIC_FIELD;
				classInfo = classTable[row[0]];
				staticConstructorNotInvoked = true;
				if ((classInfo[4] < 2)) {
					stack[0] = pc;
					stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST$intBuffer16);
					if ((PST$intBuffer16[0] == 1)) {
						return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
					}
					if ((stackFrame2 != null)) {
						staticConstructorNotInvoked = false;
						stack = stackFrame2;
						pc = stack[0];
						localsStackSetToken = stack[1];
						localsStackOffset = stack[2];
					}
				}
				if (staticConstructorNotInvoked) {
					if ((valueStackSize == valueStackCapacity)) {
						valueStack = valueStackIncreaseCapacity(ec);
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize++] = classInfo[5][row[1]];
				}
				break;
			case 28:
				// DUPLICATE_STACK_TOP;
				if ((row[0] == 1)) {
					value = valueStack[(valueStackSize - 1)];
					if ((valueStackSize == valueStackCapacity)) {
						valueStack = valueStackIncreaseCapacity(ec);
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize++] = value;
				} else if ((row[0] == 2)) {
					if (((valueStackSize + 1) > valueStackCapacity)) {
						valueStackIncreaseCapacity(ec);
						valueStack = ec[4];
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize] = valueStack[(valueStackSize - 2)];
					valueStack[(valueStackSize + 1)] = valueStack[(valueStackSize - 1)];
					valueStackSize += 2;
				} else {
					hasInterrupt = EX_Fatal(ec, "?");
				}
				break;
			case 29:
				// EQUALS;
				valueStackSize -= 2;
				rightValue = valueStack[(valueStackSize + 1)];
				leftValue = valueStack[valueStackSize];
				if ((leftValue[0] == rightValue[0])) {
					switch (leftValue[0]) {
						case 1:
							bool1 = true;
							break;
						case 2:
							bool1 = (leftValue[1] == rightValue[1]);
							break;
						case 3:
							bool1 = (leftValue[1] == rightValue[1]);
							break;
						case 5:
							bool1 = (leftValue[1] == rightValue[1]);
							break;
						default:
							bool1 = (doEqualityComparisonAndReturnCode(leftValue, rightValue) == 1);
							break;
					}
				} else {
					int1 = doEqualityComparisonAndReturnCode(leftValue, rightValue);
					if ((int1 == 0)) {
						bool1 = false;
					} else if ((int1 == 1)) {
						bool1 = true;
					} else {
						hasInterrupt = EX_UnsupportedOperation(ec, "== and != not defined here.");
					}
				}
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				if ((bool1 != ((row[0] == 1)))) {
					valueStack[valueStackSize] = VALUE_TRUE;
				} else {
					valueStack[valueStackSize] = VALUE_FALSE;
				}
				valueStackSize += 1;
				break;
			case 30:
				// ESF_LOOKUP;
				esfData = generateEsfData(args.length, row);
				metadata[17] = esfData;
				break;
			case 31:
				// EXCEPTION_HANDLED_TOGGLE;
				ec[9] = (row[0] == 1);
				break;
			case 32:
				// FIELD_TYPE_INFO;
				initializeClassFieldTypeInfo(vm, row);
				break;
			case 33:
				// FINALIZE_INITIALIZATION;
				finalizeInitializationImpl(vm, stringArgs[pc], row[0]);
				identifiers = vm[4][0];
				literalTable = vm[4][3];
				globalNameIdToPrimitiveMethodName = vm[4][12];
				funcArgs = vm[8];
				break;
			case 34:
				// FINALLY_END;
				value = ec[10];
				if (((value == null) || ec[9])) {
					switch (stack[10]) {
						case 0:
							ec[10] = null;
							break;
						case 1:
							ec[10] = null;
							int1 = row[0];
							if ((int1 == 1)) {
								pc += row[1];
							} else if ((int1 == 2)) {
								intArray1 = esfData[pc];
								pc = intArray1[1];
							} else {
								hasInterrupt = EX_Fatal(ec, "break exists without a loop");
							}
							break;
						case 2:
							ec[10] = null;
							int1 = row[2];
							if ((int1 == 1)) {
								pc += row[3];
							} else if ((int1 == 2)) {
								intArray1 = esfData[pc];
								pc = intArray1[1];
							} else {
								hasInterrupt = EX_Fatal(ec, "continue exists without a loop");
							}
							break;
						case 3:
							if ((stack[8] != 0)) {
								markClassAsInitialized(vm, stack, stack[8]);
							}
							if (stack[5]) {
								valueStackSize = stack[7];
								value = stack[11];
								stack = stack[4];
								if ((valueStackSize == valueStackCapacity)) {
									valueStack = valueStackIncreaseCapacity(ec);
									valueStackCapacity = valueStack.length;
								}
								valueStack[valueStackSize] = value;
								valueStackSize += 1;
							} else {
								valueStackSize = stack[7];
								stack = stack[4];
							}
							pc = stack[0];
							localsStackOffset = stack[2];
							localsStackSetToken = stack[1];
							break;
					}
				} else {
					ec[9] = false;
					stack[0] = pc;
					intArray1 = esfData[pc];
					value = ec[10];
					objInstance1 = value[1];
					objArray1 = objInstance1[3];
					bool1 = true;
					if ((objArray1[0] != null)) {
						bool1 = objArray1[0];
					}
					intList1 = objArray1[1];
					while (((stack != null) && ((intArray1 == null) || bool1))) {
						stack = stack[4];
						if ((stack != null)) {
							pc = stack[0];
							intList1.push(pc);
							intArray1 = esfData[pc];
						}
					}
					if ((stack == null)) {
						return uncaughtExceptionResult(vm, value);
					}
					int1 = intArray1[0];
					if ((int1 < pc)) {
						int1 = intArray1[1];
					}
					pc = (int1 - 1);
					stack[0] = pc;
					localsStackOffset = stack[2];
					localsStackSetToken = stack[1];
					ec[1] = stack;
					stack[10] = 0;
					ec[2] = valueStackSize;
				}
				break;
			case 35:
				// FUNCTION_DEFINITION;
				initializeFunction(vm, row, pc, stringArgs[pc]);
				pc += row[7];
				functionTable = metadata[10];
				break;
			case 36:
				// INDEX;
				value = valueStack[--valueStackSize];
				root = valueStack[(valueStackSize - 1)];
				if ((root[0] == 6)) {
					if ((value[0] != 3)) {
						hasInterrupt = EX_InvalidArgument(ec, "List index must be an integer.");
					} else {
						i = value[1];
						list1 = root[1];
						if ((i < 0)) {
							i += list1[1];
						}
						if (((i < 0) || (i >= list1[1]))) {
							hasInterrupt = EX_IndexOutOfRange(ec, "List index is out of bounds");
						} else {
							valueStack[(valueStackSize - 1)] = list1[2][i];
						}
					}
				} else if ((root[0] == 7)) {
					dictImpl = root[1];
					keyType = value[0];
					if ((keyType != dictImpl[1])) {
						if ((dictImpl[0] == 0)) {
							hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.");
						} else {
							hasInterrupt = EX_InvalidKey(ec, ["Incorrect key type. This dictionary contains ", getTypeFromId(dictImpl[1]), " keys. Provided key is a ", getTypeFromId(keyType), "."].join(''));
						}
					} else {
						if ((keyType == 3)) {
							intKey = value[1];
						} else if ((keyType == 5)) {
							stringKey = value[1];
						} else if ((keyType == 8)) {
							intKey = (value[1])[1];
						} else if ((dictImpl[0] == 0)) {
							hasInterrupt = EX_KeyNotFound(ec, "Key not found. Dictionary is empty.");
						} else {
							hasInterrupt = EX_KeyNotFound(ec, "Key not found.");
						}
						if (!hasInterrupt) {
							if ((keyType == 5)) {
								stringIntDict1 = dictImpl[5];
								int1 = stringIntDict1[stringKey];
								if (int1 === undefined) int1 = -1;
								if ((int1 == -1)) {
									hasInterrupt = EX_KeyNotFound(ec, ["Key not found: '", stringKey, "'"].join(''));
								} else {
									valueStack[(valueStackSize - 1)] = dictImpl[7][int1];
								}
							} else {
								intIntDict1 = dictImpl[4];
								int1 = intIntDict1[intKey];
								if (int1 === undefined) int1 = -1;
								if ((int1 == -1)) {
									hasInterrupt = EX_KeyNotFound(ec, "Key not found.");
								} else {
									valueStack[(valueStackSize - 1)] = dictImpl[7][intIntDict1[intKey]];
								}
							}
						}
					}
				} else if ((root[0] == 5)) {
					string1 = root[1];
					if ((value[0] != 3)) {
						hasInterrupt = EX_InvalidArgument(ec, "String indices must be integers.");
					} else {
						int1 = value[1];
						if ((int1 < 0)) {
							int1 += string1.length;
						}
						if (((int1 < 0) || (int1 >= string1.length))) {
							hasInterrupt = EX_IndexOutOfRange(ec, "String index out of range.");
						} else {
							valueStack[(valueStackSize - 1)] = buildCommonString(globals, string1.charAt(int1));
						}
					}
				} else {
					hasInterrupt = EX_InvalidArgument(ec, "Cannot index into this type: " + getTypeFromId(root[0]));
				}
				break;
			case 37:
				// IS_COMPARISON;
				value = valueStack[(valueStackSize - 1)];
				output = VALUE_FALSE;
				if ((value[0] == 8)) {
					objInstance1 = value[1];
					if (isClassASubclassOf(vm, objInstance1[0], row[0])) {
						output = VALUE_TRUE;
					}
				}
				valueStack[(valueStackSize - 1)] = output;
				break;
			case 38:
				// ITERATION_STEP;
				int1 = (localsStackOffset + row[2]);
				value3 = localsStack[int1];
				i = value3[1];
				value = localsStack[(localsStackOffset + row[3])];
				if ((value[0] == 6)) {
					list1 = value[1];
					_len = list1[1];
					bool1 = true;
				} else {
					string2 = value[1];
					_len = string2.length;
					bool1 = false;
				}
				if ((i < _len)) {
					if (bool1) {
						value = list1[2][i];
					} else {
						value = buildCommonString(globals, string2.charAt(i));
					}
					int3 = (localsStackOffset + row[1]);
					localsStackSet[int3] = localsStackSetToken;
					localsStack[int3] = value;
				} else {
					pc += row[0];
				}
				i += 1;
				if ((i < 2049)) {
					localsStack[int1] = INTEGER_POSITIVE_CACHE[i];
				} else {
					localsStack[int1] = [3, i];
				}
				break;
			case 39:
				// JUMP;
				pc += row[0];
				break;
			case 40:
				// JUMP_IF_EXCEPTION_OF_TYPE;
				value = ec[10];
				objInstance1 = value[1];
				int1 = objInstance1[0];
				i = (row.length - 1);
				while ((i >= 2)) {
					if (isClassASubclassOf(vm, int1, row[i])) {
						i = 0;
						pc += row[0];
						int2 = row[1];
						if ((int2 > -1)) {
							int1 = (localsStackOffset + int2);
							localsStack[int1] = value;
							localsStackSet[int1] = localsStackSetToken;
						}
					}
					i -= 1;
				}
				break;
			case 41:
				// JUMP_IF_FALSE;
				value = valueStack[--valueStackSize];
				if ((value[0] != 2)) {
					hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
				} else if (!value[1]) {
					pc += row[0];
				}
				break;
			case 42:
				// JUMP_IF_FALSE_NON_POP;
				value = valueStack[(valueStackSize - 1)];
				if ((value[0] != 2)) {
					hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
				} else if (value[1]) {
					valueStackSize -= 1;
				} else {
					pc += row[0];
				}
				break;
			case 43:
				// JUMP_IF_TRUE;
				value = valueStack[--valueStackSize];
				if ((value[0] != 2)) {
					hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
				} else if (value[1]) {
					pc += row[0];
				}
				break;
			case 44:
				// JUMP_IF_TRUE_NO_POP;
				value = valueStack[(valueStackSize - 1)];
				if ((value[0] != 2)) {
					hasInterrupt = EX_InvalidArgument(ec, "Boolean expected.");
				} else if (value[1]) {
					pc += row[0];
				} else {
					valueStackSize -= 1;
				}
				break;
			case 45:
				// LAMBDA;
				if (!(metadata[11][pc] !== undefined)) {
					int1 = (4 + row[4] + 1);
					_len = row[int1];
					intArray1 = PST$createNewArray(_len);
					i = 0;
					while ((i < _len)) {
						intArray1[i] = row[(int1 + i + 1)];
						i += 1;
					}
					_len = row[4];
					intArray2 = PST$createNewArray(_len);
					i = 0;
					while ((i < _len)) {
						intArray2[i] = row[(5 + i)];
						i += 1;
					}
					metadata[11][pc] = [pc, 0, pc, row[0], row[1], 5, 0, row[2], intArray2, "lambda", intArray1];
				}
				closure = {};
				parentClosure = stack[12];
				if ((parentClosure == null)) {
					parentClosure = {};
					stack[12] = parentClosure;
					parentClosure[-1] = [stack[6]];
				}
				closure[-1] = parentClosure[-1];
				functionInfo = metadata[11][pc];
				intArray1 = functionInfo[10];
				_len = intArray1.length;
				i = 0;
				while ((i < _len)) {
					j = intArray1[i];
					if ((parentClosure[j] !== undefined)) {
						closure[j] = parentClosure[j];
					} else {
						closure[j] = [null];
						parentClosure[j] = closure[j];
					}
					i += 1;
				}
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				valueStack[valueStackSize] = [9, [5, null, 0, pc, closure]];
				valueStackSize += 1;
				pc += row[3];
				break;
			case 46:
				// UNUSED_LIB_DECLARATION;
				break;
			case 47:
				// LIST_SLICE;
				if ((row[2] == 1)) {
					valueStackSize -= 1;
					arg3 = valueStack[valueStackSize];
				} else {
					arg3 = null;
				}
				if ((row[1] == 1)) {
					valueStackSize -= 1;
					arg2 = valueStack[valueStackSize];
				} else {
					arg2 = null;
				}
				if ((row[0] == 1)) {
					valueStackSize -= 1;
					arg1 = valueStack[valueStackSize];
				} else {
					arg1 = null;
				}
				value = valueStack[(valueStackSize - 1)];
				value = performListSlice(globals, ec, value, arg1, arg2, arg3);
				hasInterrupt = (ec[13] != null);
				if (!hasInterrupt) {
					valueStack[(valueStackSize - 1)] = value;
				}
				break;
			case 48:
				// LITERAL;
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				valueStack[valueStackSize++] = literalTable[row[0]];
				break;
			case 49:
				// LITERAL_STREAM;
				int1 = row.length;
				if (((valueStackSize + int1) > valueStackCapacity)) {
					while (((valueStackSize + int1) > valueStackCapacity)) {
						valueStackIncreaseCapacity(ec);
						valueStack = ec[4];
						valueStackCapacity = valueStack.length;
					}
				}
				i = int1;
				while ((--i >= 0)) {
					valueStack[valueStackSize++] = literalTable[row[i]];
				}
				break;
			case 50:
				// LOCAL;
				int1 = (localsStackOffset + row[0]);
				if ((localsStackSet[int1] == localsStackSetToken)) {
					if ((valueStackSize == valueStackCapacity)) {
						valueStack = valueStackIncreaseCapacity(ec);
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize++] = localsStack[int1];
				} else {
					string1 = ["Variable used before it was set: '", vm[4][0][row[1]], "'"].join('');
					hasInterrupt = EX_UnassignedVariable(ec, string1);
				}
				break;
			case 51:
				// LOC_TABLE;
				initLocTable(vm, row);
				break;
			case 52:
				// NEGATIVE_SIGN;
				value = valueStack[(valueStackSize - 1)];
				type = value[0];
				if ((type == 3)) {
					valueStack[(valueStackSize - 1)] = buildInteger(globals, -value[1]);
				} else if ((type == 4)) {
					valueStack[(valueStackSize - 1)] = buildFloat(globals, -value[1]);
				} else {
					hasInterrupt = EX_InvalidArgument(ec, ["Negative sign can only be applied to numbers. Found ", getTypeFromId(type), " instead."].join(''));
				}
				break;
			case 53:
				// POP;
				valueStackSize -= 1;
				break;
			case 54:
				// POP_IF_NULL_OR_JUMP;
				value = valueStack[(valueStackSize - 1)];
				if ((value[0] == 1)) {
					valueStackSize -= 1;
				} else {
					pc += row[0];
				}
				break;
			case 55:
				// PUSH_FUNC_REF;
				value = null;
				switch (row[1]) {
					case 0:
						value = [9, [1, null, 0, row[0], null]];
						break;
					case 1:
						value = [9, [2, stack[6], row[2], row[0], null]];
						break;
					case 2:
						classId = row[2];
						classInfo = classTable[classId];
						staticConstructorNotInvoked = true;
						if ((classInfo[4] < 2)) {
							stack[0] = pc;
							stackFrame2 = maybeInvokeStaticConstructor(vm, ec, stack, classInfo, valueStackSize, PST$intBuffer16);
							if ((PST$intBuffer16[0] == 1)) {
								return generateException(vm, stack, pc, valueStackSize, ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
							}
							if ((stackFrame2 != null)) {
								staticConstructorNotInvoked = false;
								stack = stackFrame2;
								pc = stack[0];
								localsStackSetToken = stack[1];
								localsStackOffset = stack[2];
							}
						}
						if (staticConstructorNotInvoked) {
							value = [9, [3, null, classId, row[0], null]];
						} else {
							value = null;
						}
						break;
				}
				if ((value != null)) {
					if ((valueStackSize == valueStackCapacity)) {
						valueStack = valueStackIncreaseCapacity(ec);
						valueStackCapacity = valueStack.length;
					}
					valueStack[valueStackSize] = value;
					valueStackSize += 1;
				}
				break;
			case 56:
				// RETURN;
				if ((esfData[pc] != null)) {
					intArray1 = esfData[pc];
					pc = (intArray1[1] - 1);
					if ((row[0] == 0)) {
						stack[11] = VALUE_NULL;
					} else {
						stack[11] = valueStack[(valueStackSize - 1)];
					}
					valueStackSize = stack[7];
					stack[10] = 3;
				} else {
					if ((stack[4] == null)) {
						return interpreterFinished(vm, ec);
					}
					if ((stack[8] != 0)) {
						markClassAsInitialized(vm, stack, stack[8]);
					}
					if (stack[5]) {
						if ((row[0] == 0)) {
							valueStackSize = stack[7];
							stack = stack[4];
							if ((valueStackSize == valueStackCapacity)) {
								valueStack = valueStackIncreaseCapacity(ec);
								valueStackCapacity = valueStack.length;
							}
							valueStack[valueStackSize] = VALUE_NULL;
						} else {
							value = valueStack[(valueStackSize - 1)];
							valueStackSize = stack[7];
							stack = stack[4];
							valueStack[valueStackSize] = value;
						}
						valueStackSize += 1;
					} else {
						valueStackSize = stack[7];
						stack = stack[4];
					}
					pc = stack[0];
					localsStackOffset = stack[2];
					localsStackSetToken = stack[1];
				}
				break;
			case 57:
				// STACK_INSERTION_FOR_INCREMENT;
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				valueStack[valueStackSize] = valueStack[(valueStackSize - 1)];
				valueStack[(valueStackSize - 1)] = valueStack[(valueStackSize - 2)];
				valueStack[(valueStackSize - 2)] = valueStack[(valueStackSize - 3)];
				valueStack[(valueStackSize - 3)] = valueStack[valueStackSize];
				valueStackSize += 1;
				break;
			case 58:
				// STACK_SWAP_POP;
				valueStackSize -= 1;
				valueStack[(valueStackSize - 1)] = valueStack[valueStackSize];
				break;
			case 59:
				// SWITCH_INT;
				value = valueStack[--valueStackSize];
				if ((value[0] == 3)) {
					intKey = value[1];
					integerSwitch = integerSwitchesByPc[pc];
					if ((integerSwitch == null)) {
						integerSwitch = initializeIntSwitchStatement(vm, pc, row);
					}
					i = integerSwitch[intKey];
					if (i === undefined) i = -1;
					if ((i == -1)) {
						pc += row[0];
					} else {
						pc += i;
					}
				} else {
					hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects an integer.");
				}
				break;
			case 60:
				// SWITCH_STRING;
				value = valueStack[--valueStackSize];
				if ((value[0] == 5)) {
					stringKey = value[1];
					stringSwitch = stringSwitchesByPc[pc];
					if ((stringSwitch == null)) {
						stringSwitch = initializeStringSwitchStatement(vm, pc, row);
					}
					i = stringSwitch[stringKey];
					if (i === undefined) i = -1;
					if ((i == -1)) {
						pc += row[0];
					} else {
						pc += i;
					}
				} else {
					hasInterrupt = EX_InvalidArgument(ec, "Switch statement expects a string.");
				}
				break;
			case 61:
				// THIS;
				if ((valueStackSize == valueStackCapacity)) {
					valueStack = valueStackIncreaseCapacity(ec);
					valueStackCapacity = valueStack.length;
				}
				valueStack[valueStackSize] = stack[6];
				valueStackSize += 1;
				break;
			case 62:
				// THROW;
				valueStackSize -= 1;
				value = valueStack[valueStackSize];
				bool2 = (value[0] == 8);
				if (bool2) {
					objInstance1 = value[1];
					if (!isClassASubclassOf(vm, objInstance1[0], magicNumbers[0])) {
						bool2 = false;
					}
				}
				if (bool2) {
					objArray1 = objInstance1[3];
					intList1 = [];
					objArray1[1] = intList1;
					if (!isPcFromCore(vm, pc)) {
						intList1.push(pc);
					}
					ec[10] = value;
					ec[9] = false;
					stack[0] = pc;
					intArray1 = esfData[pc];
					value = ec[10];
					objInstance1 = value[1];
					objArray1 = objInstance1[3];
					bool1 = true;
					if ((objArray1[0] != null)) {
						bool1 = objArray1[0];
					}
					intList1 = objArray1[1];
					while (((stack != null) && ((intArray1 == null) || bool1))) {
						stack = stack[4];
						if ((stack != null)) {
							pc = stack[0];
							intList1.push(pc);
							intArray1 = esfData[pc];
						}
					}
					if ((stack == null)) {
						return uncaughtExceptionResult(vm, value);
					}
					int1 = intArray1[0];
					if ((int1 < pc)) {
						int1 = intArray1[1];
					}
					pc = (int1 - 1);
					stack[0] = pc;
					localsStackOffset = stack[2];
					localsStackSetToken = stack[1];
					ec[1] = stack;
					stack[10] = 0;
					ec[2] = valueStackSize;
				} else {
					hasInterrupt = EX_InvalidArgument(ec, "Thrown value is not an exception.");
				}
				break;
			case 63:
				// TOKEN_DATA;
				tokenDataImpl(vm, row);
				break;
			case 64:
				// USER_CODE_START;
				metadata[15] = row[0];
				break;
			case 65:
				// VERIFY_TYPE_IS_ITERABLE;
				value = valueStack[--valueStackSize];
				i = (localsStackOffset + row[0]);
				localsStack[i] = value;
				localsStackSet[i] = localsStackSetToken;
				int1 = value[0];
				if (((int1 != 6) && (int1 != 5))) {
					hasInterrupt = EX_InvalidArgument(ec, ["Expected an iterable type, such as a list or string. Found ", getTypeFromId(int1), " instead."].join(''));
				}
				i = (localsStackOffset + row[1]);
				localsStack[i] = VALUE_INT_ZERO;
				localsStackSet[i] = localsStackSetToken;
				break;
			default:
				// THIS SHOULD NEVER HAPPEN;
				return generateException(vm, stack, pc, valueStackSize, ec, 0, "Bad op code: " + ('' + ops[pc]));
		}
		if (hasInterrupt) {
			var interrupt = ec[13];
			ec[13] = null;
			if ((interrupt[0] == 1)) {
				return generateException(vm, stack, pc, valueStackSize, ec, interrupt[1], interrupt[2]);
			}
			if ((interrupt[0] == 3)) {
				return [5, "", interrupt[3], 0, false, ""];
			}
			if ((interrupt[0] == 4)) {
				return [6, "", 0.0, 0, false, interrupt[2]];
			}
			if ((interrupt[0] == 6)) {
				return [2, "", 0.0, 0, false, ""];
			}
		}
		++pc;
	}
};

var invokeNamedCallback = function(vm, id, args) {
	var cb = vm[12][0][id];
	return cb(args);
};

var IpcNamedPipe_create = function(objValue, nameValue) {
	var obj = objValue[1];
	obj[3] = PST$createNewArray(1);
	obj[3][0] = (() => {})();
	return null;
};

var IpcNamedPipe_flush = function(objValue) {
	var pipe = getNativeDataItem(objValue, 0);
	return (() => {})();
};

var IpcNamedPipe_send = function(objValue, strValue) {
	var pipe = getNativeDataItem(objValue, 0);
	return (() => {})();
};

var IpcNamedPipeServer_close = function(objValue) {
	var obj = objValue[1];
	var pipe = obj[3][0];
	return (() => {})();
};

var IpcNamedPipeServer_create = function(vm, objValue, nameValue, startFn, dataFn, closeFn) {
	var obj = objValue[1];
	obj[3] = PST$createNewArray(1);
	obj[3][0] = (() => {})();
	return null;
};

var IpcUnixSocketClient_create = function(vm, inst, path, onReadyCb, onDisconnectCb) {
	inst[3] = PST$createNewArray(1);
	return (() => {})();
};

var IpcUnixSocketClient_send = function(client, msg) {
	return (() => {})();
};

var IpcUnixSocketServer_create = function(vm, inst, path, onRecvCb) {
	inst[3] = PST$createNewArray(1);
	return (() => {})();
};

var IpcUnixSocketServer_disconnect = function(server) {
	(() => {})();
	return 0;
};

var isClassASubclassOf = function(vm, subClassId, parentClassId) {
	if ((subClassId == parentClassId)) {
		return true;
	}
	var classTable = vm[4][9];
	var classIdWalker = subClassId;
	while ((classIdWalker != -1)) {
		if ((classIdWalker == parentClassId)) {
			return true;
		}
		var classInfo = classTable[classIdWalker];
		classIdWalker = classInfo[2];
	}
	return false;
};

var isPcFromCore = function(vm, pc) {
	if ((vm[3] == null)) {
		return false;
	}
	var tokens = vm[3][0][pc];
	if ((tokens == null)) {
		return false;
	}
	var token = tokens[0];
	var filename = tokenHelperGetFileLine(vm, token[2], 0);
	return "[Core]" == filename;
};

var isStringEqual = function(a, b) {
	if ((a == b)) {
		return true;
	}
	return false;
};

var isVmResultRootExecContext = function(result) {
	return result[4];
};

var JsonHelper_serialize = function(statusOut, vm, root, depth, isPretty, omitDictNull) {
	var sb = [];
	var errorOut = PST$createNewArray(1);
	errorOut[0] = null;
	JsonHelper_serializeImpl(vm, 0, root, depth, isPretty, omitDictNull, sb, errorOut);
	statusOut[0] = 0;
	if ((errorOut[0] != null)) {
		statusOut[0] = 1;
		return buildString(vm[13], errorOut[0]);
	}
	return buildString(vm[13], sb.join(""));
};

var JsonHelper_serializeImpl = function(vm, currentIndent, root, depth, isPretty, omitDictNull, sb, errorOut) {
	var i = 0;
	switch (root[0]) {
		case 1:
			sb.push("null");
			break;
		case 2:
			if (root[1]) {
				sb.push("true");
			} else {
				sb.push("false");
			}
			break;
		case 4:
			sb.push(valueToString(vm, root));
			break;
		case 3:
			sb.push(('' + root[1]));
			break;
		case 5:
			sb.push(JSON.stringify(root[1]));
			break;
		case 6:
			if ((depth == 0)) {
				errorOut[0] = "Maximum recursion depth exceeded.";
				return;
			}
			var list = root[1];
			if ((list[1] == 0)) {
				sb.push("[]");
			} else {
				var newIndent = (currentIndent + 1);
				sb.push("[");
				i = 0;
				while ((i < list[1])) {
					if ((errorOut[0] != null)) {
						return;
					}
					if ((i > 0)) {
						sb.push(",");
					}
					if (isPretty) {
						sb.push("\n");
						var s = 0;
						while ((s < newIndent)) {
							sb.push("  ");
							s += 1;
						}
					}
					JsonHelper_serializeImpl(vm, newIndent, list[2][i], (depth - 1), isPretty, omitDictNull, sb, errorOut);
					i += 1;
				}
				if (isPretty) {
					sb.push("\n");
					i = 0;
					while ((i < currentIndent)) {
						sb.push("  ");
						i += 1;
					}
				}
				sb.push("]");
			}
			break;
		case 7:
			if ((depth == 0)) {
				errorOut[0] = "Maximum recursion depth exceeded.";
				return;
			}
			var dict = root[1];
			if ((dict[0] == 0)) {
				sb.push("{}");
			} else {
				var newIndent = (currentIndent + 1);
				sb.push("{");
				var keys = dict[6];
				var values = dict[7];
				if ((keys[0][0] != 5)) {
					errorOut[0] = "Only string dictionaries can be used.";
					return;
				}
				var start = 0;
				var end = keys.length;
				if (omitDictNull) {
					while (((start < end) && (values[start][0] == 1))) {
						start += 1;
					}
				}
				i = start;
				while ((i < end)) {
					if ((errorOut[0] != null)) {
						return;
					}
					if (((values[i][0] != 1) || !omitDictNull)) {
						if ((i > start)) {
							sb.push(",");
						}
						if (isPretty) {
							sb.push("\n");
							var s = 0;
							while ((s < newIndent)) {
								sb.push("  ");
								s += 1;
							}
						}
						sb.push(JSON.stringify(keys[i][1]));
						if (isPretty) {
							sb.push(": ");
						} else {
							sb.push(":");
						}
						JsonHelper_serializeImpl(vm, newIndent, values[i], (depth - 1), isPretty, omitDictNull, sb, errorOut);
					}
					i += 1;
				}
				if (isPretty) {
					sb.push("\n");
					i = 0;
					while ((i < currentIndent)) {
						sb.push("  ");
						i += 1;
					}
				}
				sb.push("}");
			}
			break;
		default:
			errorOut[0] = "This type cannot be serialized to JSON.";
			break;
	}
	return;
};

var lib_fileiocommon_directoryCreate = function(vm, arg1, arg2, arg3) {
	var bool1 = false;
	var i = 0;
	var int1 = 0;
	var stringList1 = null;
	var hostObject = arg1;
	var path = arg2[1];
	if (arg3[1]) {
		int1 = 0;
		if (!COMMON.fakeDisk.dirExists(lib_fileiocommon_getDiskObject(hostObject), '/')) {
			int1 = 4;
		} else {
			stringList1 = [];
			bool1 = true;
			while ((bool1 && !COMMON.fakeDisk.dirExists(lib_fileiocommon_getDiskObject(hostObject), path))) {
				stringList1.push(path);
				int1 = COMMON.fakeDisk.getPathParent(path, PST$stringBuffer16);
				path = PST$stringBuffer16[0];
				if ((int1 != 0)) {
					bool1 = false;
				}
			}
			if (bool1) {
				i = (stringList1.length - 1);
				while ((i >= 0)) {
					path = stringList1[i];
					int1 = COMMON.fakeDisk.mkdir(lib_fileiocommon_getDiskObject(hostObject), path);
					if ((int1 != 0)) {
						i = -1;
					}
					i -= 1;
				}
			}
		}
	} else {
		int1 = COMMON.fakeDisk.mkdir(lib_fileiocommon_getDiskObject(hostObject), path);
	}
	return buildInteger(vm[13], int1);
};

var lib_fileiocommon_directoryDelete = function(vm, arg1, arg2) {
	var sc = COMMON.fakeDisk.rmdir(lib_fileiocommon_getDiskObject(arg1), arg2[1]);
	return buildInteger(vm[13], sc);
};

var lib_fileiocommon_directoryList = function(vm, arg1, arg2, arg3, arg4) {
	var diskhost = arg1;
	var path = arg2[1];
	var useFullPath = arg3[1];
	var outputList = arg4[1];
	var stringList1 = [];
	var sc = COMMON.fakeDisk.listdir(lib_fileiocommon_getDiskObject(diskhost), path, useFullPath, stringList1);
	if ((sc == 0)) {
		var i = 0;
		while ((i < stringList1.length)) {
			addToList(outputList, buildString(vm[13], stringList1[i]));
			i += 1;
		}
	}
	return buildInteger(vm[13], sc);
};

var lib_fileiocommon_directoryMove = function(vm, arg1, arg2, arg3) {
	var statusCode = COMMON.fakeDisk.movedir(lib_fileiocommon_getDiskObject(arg1), arg2[1], arg3[1]);
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileDelete = function(vm, arg1, arg2) {
	var statusCode = COMMON.fakeDisk.fileDelete(lib_fileiocommon_getDiskObject(arg1), arg2[1]);
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileInfo = function(vm, arg1, arg2, arg3, arg4) {
	var mask = arg3[1];
	COMMON.fakeDisk.getPathInfoExt(lib_fileiocommon_getDiskObject(arg1), arg2[1], mask, PST$intBuffer16, PST$floatBuffer16);
	var outputList = arg4[1];
	clearList(outputList);
	var globals = vm[13];
	addToList(outputList, buildBoolean(globals, (PST$intBuffer16[0] > 0)));
	addToList(outputList, buildBoolean(globals, (PST$intBuffer16[1] > 0)));
	if (((mask & 1) != 0)) {
		addToList(outputList, buildInteger(globals, PST$intBuffer16[2]));
	} else {
		addToList(outputList, globals[0]);
	}
	if (((mask & 2) != 0)) {
		addToList(outputList, buildBoolean(globals, (PST$intBuffer16[3] > 0)));
	} else {
		addToList(outputList, globals[0]);
	}
	if (((mask & 4) != 0)) {
		addToList(outputList, buildFloat(globals, PST$floatBuffer16[0]));
	} else {
		addToList(outputList, globals[0]);
	}
	if (((mask & 8) != 0)) {
		addToList(outputList, buildFloat(globals, PST$floatBuffer16[1]));
	} else {
		addToList(outputList, globals[0]);
	}
	return arg4;
};

var lib_fileiocommon_fileMove = function(vm, arg1, arg2, arg3, arg4, arg5) {
	var statusCode = COMMON.fakeDisk.fileMove(lib_fileiocommon_getDiskObject(arg1), arg2[1], arg3[1], arg4[1], arg5[1]);
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileRead = function(vm, arg1, arg2, arg3, arg4) {
	var diskHostObject = arg1;
	var sandboxedPath = arg2[1];
	var readDataAsBytes = arg3[1];
	var outputList = arg4[1];
	var tList = [];
	var statusCode = COMMON.fakeDisk.fileRead(lib_fileiocommon_getDiskObject(diskHostObject), sandboxedPath, readDataAsBytes, PST$stringBuffer16, vm[13][9], tList);
	if (((statusCode == 0) && !readDataAsBytes)) {
		addToList(outputList, buildString(vm[13], PST$stringBuffer16[0]));
	} else {
		outputList[2] = tList;
		outputList[1] = tList.length;
	}
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_fileWrite = function(vm, arg1, arg2, arg3, arg4) {
	var ints = vm[13][9];
	if ((arg4[0] != 3)) {
		return ints[3];
	}
	var statusCode = 0;
	var contentString = null;
	var byteArrayRef = null;
	var format = arg4[1];
	if ((format == 0)) {
		byteArrayRef = lib_fileiocommon_listToBytes(arg3[1]);
		if ((byteArrayRef == null)) {
			return ints[6];
		}
	} else if ((arg3[0] != 5)) {
		return ints[6];
	} else {
		contentString = arg3[1];
	}
	if ((statusCode == 0)) {
		statusCode = COMMON.fakeDisk.fileWrite(lib_fileiocommon_getDiskObject(arg1), arg2[1], format, contentString, byteArrayRef);
	}
	return buildInteger(vm[13], statusCode);
};

var lib_fileiocommon_getCurrentDirectory = function(vm) {
	return buildString(vm[13], '/');
};

var lib_fileiocommon_getDiskObject = function(diskObjectArg) {
	var objInst = diskObjectArg[1];
	return objInst[3][0];
};

var lib_fileiocommon_getUserDirectory = function(vm) {
	return buildString(vm[13], '/');
};

var lib_fileiocommon_initializeDisk = function(vm, arg1, arg2) {
	var objInstance1 = arg1[1];
	var objArray1 = PST$createNewArray(1);
	objInstance1[3] = objArray1;
	var object1 = COMMON.fakeDisk.create(arg2[1]);
	objArray1[0] = object1;
	return vm[13][0];
};

var lib_fileiocommon_listToBytes = function(listOfMaybeInts) {
	var bytes = PST$createNewArray(listOfMaybeInts[1]);
	var intValue = null;
	var byteValue = 0;
	var i = (listOfMaybeInts[1] - 1);
	while ((i >= 0)) {
		intValue = listOfMaybeInts[2][i];
		if ((intValue[0] != 3)) {
			return null;
		}
		byteValue = intValue[1];
		if ((byteValue >= 256)) {
			return null;
		}
		if ((byteValue < 0)) {
			if ((byteValue < -128)) {
				return null;
			}
			byteValue += 256;
		}
		bytes[i] = byteValue;
		i -= 1;
	}
	return bytes;
};

var listImplToBytes = function(list) {
	var size = list[1];
	var bytes = PST$createNewArray(size);
	var nv = null;
	var n = 0;
	var values = list[2];
	var i = 0;
	while ((i < size)) {
		nv = values[i];
		if ((nv[0] != 3)) {
			return null;
		}
		n = nv[1];
		if (((n < 0) || (n > 255))) {
			return null;
		}
		bytes[i] = n;
		++i;
	}
	return bytes;
};

var listImplToStringArray = function(list) {
	var output = PST$createNewArray(list[1]);
	var v = null;
	var i = 0;
	while ((i < list[1])) {
		v = list[2][i];
		if ((v[0] == 5)) {
			output[i] = v[1];
		} else {
			output[i] = null;
		}
		i += 1;
	}
	return output;
};

var makeByteListNativeData = function(obj, vList) {
	if ((vList[0] != 6)) {
		return false;
	}
	var list = vList[1];
	var bytes = listImplToBytes(list);
	if ((bytes == null)) {
		return false;
	}
	obj[3] = PST$createNewArray(1);
	obj[3][0] = bytes;
	return true;
};

var makeEmptyList = function(type, capacity) {
	return [type, 0, []];
};

var markClassAsInitialized = function(vm, stack, classId) {
	var classInfo = vm[4][9][stack[8]];
	classInfo[4] = 2;
	vm[7].pop();
	return 0;
};

var maybeInvokeStaticConstructor = function(vm, ec, stack, classInfo, valueStackSize, intOutParam) {
	PST$intBuffer16[0] = 0;
	var classId = classInfo[0];
	if ((classInfo[4] == 1)) {
		var classIdsBeingInitialized = vm[7];
		if ((classIdsBeingInitialized[(classIdsBeingInitialized.length - 1)] != classId)) {
			PST$intBuffer16[0] = 1;
		}
		return null;
	}
	classInfo[4] = 1;
	vm[7].push(classId);
	var functionInfo = vm[4][10][classInfo[6]];
	stack[0] -= 1;
	var newFrameLocalsSize = functionInfo[7];
	var currentFrameLocalsEnd = stack[3];
	if ((ec[5].length <= (currentFrameLocalsEnd + newFrameLocalsSize))) {
		increaseLocalsStackCapacity(ec, newFrameLocalsSize);
	}
	if ((ec[7] > 2000000000)) {
		resetLocalsStackTokens(ec, stack);
	}
	ec[7] += 1;
	return [functionInfo[2], ec[7], currentFrameLocalsEnd, (currentFrameLocalsEnd + newFrameLocalsSize), stack, false, null, valueStackSize, classId, (stack[9] + 1), 0, null, null, null];
};

var multiplyString = function(globals, strValue, str, n) {
	if ((n <= 2)) {
		if ((n == 1)) {
			return strValue;
		}
		if ((n == 2)) {
			return buildString(globals, str + str);
		}
		return globals[8];
	}
	var builder = [];
	while ((n > 0)) {
		n -= 1;
		builder.push(str);
	}
	str = builder.join("");
	return buildString(globals, str);
};

var nextPowerOf2 = function(value) {
	if ((((value - 1) & value) == 0)) {
		return value;
	}
	var output = 1;
	while ((output < value)) {
		output *= 2;
	}
	return output;
};

var noop = function() {
	return 0;
};

var performListSlice = function(globals, ec, value, arg1, arg2, arg3) {
	var begin = 0;
	var end = 0;
	var step = 0;
	var length = 0;
	var i = 0;
	var isForward = false;
	var isString = false;
	var originalString = "";
	var originalList = null;
	var outputList = null;
	var outputString = null;
	var status = 0;
	if ((arg3 != null)) {
		if ((arg3[0] == 3)) {
			step = arg3[1];
			if ((step == 0)) {
				status = 2;
			}
		} else {
			status = 3;
			step = 1;
		}
	} else {
		step = 1;
	}
	isForward = (step > 0);
	if ((arg2 != null)) {
		if ((arg2[0] == 3)) {
			end = arg2[1];
		} else {
			status = 3;
		}
	}
	if ((arg1 != null)) {
		if ((arg1[0] == 3)) {
			begin = arg1[1];
		} else {
			status = 3;
		}
	}
	if ((value[0] == 5)) {
		isString = true;
		originalString = value[1];
		length = originalString.length;
	} else if ((value[0] == 6)) {
		isString = false;
		originalList = value[1];
		length = originalList[1];
	} else {
		EX_InvalidArgument(ec, ["Cannot apply slicing to ", getTypeFromId(value[0]), ". Must be string or list."].join(''));
		return globals[0];
	}
	if ((status >= 2)) {
		var msg = null;
		if (isString) {
			msg = "String";
		} else {
			msg = "List";
		}
		if ((status == 3)) {
			msg += " slice indexes must be integers. Found ";
			if (((arg1 != null) && (arg1[0] != 3))) {
				EX_InvalidArgument(ec, [msg, getTypeFromId(arg1[0]), " for begin index."].join(''));
				return globals[0];
			}
			if (((arg2 != null) && (arg2[0] != 3))) {
				EX_InvalidArgument(ec, [msg, getTypeFromId(arg2[0]), " for end index."].join(''));
				return globals[0];
			}
			if (((arg3 != null) && (arg3[0] != 3))) {
				EX_InvalidArgument(ec, [msg, getTypeFromId(arg3[0]), " for step amount."].join(''));
				return globals[0];
			}
			EX_InvalidArgument(ec, "Invalid slice arguments.");
			return globals[0];
		} else {
			EX_InvalidArgument(ec, msg + " slice step cannot be 0.");
			return globals[0];
		}
	}
	status = canonicalizeListSliceArgs(PST$intBuffer16, arg1, arg2, begin, end, step, length, isForward);
	if ((status == 1)) {
		begin = PST$intBuffer16[0];
		end = PST$intBuffer16[1];
		if (isString) {
			outputString = [];
			if (isForward) {
				if ((step == 1)) {
					return buildString(globals, originalString.substring(begin, begin + (end - begin)));
				} else {
					while ((begin < end)) {
						outputString.push(originalString.charAt(begin));
						begin += step;
					}
				}
			} else {
				while ((begin > end)) {
					outputString.push(originalString.charAt(begin));
					begin += step;
				}
			}
			value = buildString(globals, outputString.join(""));
		} else {
			outputList = makeEmptyList(originalList[0], 10);
			if (isForward) {
				while ((begin < end)) {
					addToList(outputList, originalList[2][begin]);
					begin += step;
				}
			} else {
				while ((begin > end)) {
					addToList(outputList, originalList[2][begin]);
					begin += step;
				}
			}
			value = [6, outputList];
		}
	} else if ((status == 0)) {
		if (isString) {
			value = globals[8];
		} else {
			value = [6, makeEmptyList(originalList[0], 0)];
		}
	} else if ((status == 2)) {
		if (!isString) {
			outputList = makeEmptyList(originalList[0], length);
			i = 0;
			while ((i < length)) {
				addToList(outputList, originalList[2][i]);
				i += 1;
			}
			value = [6, outputList];
		}
	} else {
		var msg = null;
		if (isString) {
			msg = "String";
		} else {
			msg = "List";
		}
		if ((status == 3)) {
			msg += " slice begin index is out of range.";
		} else if (isForward) {
			msg += " slice begin index must occur before the end index when step is positive.";
		} else {
			msg += " slice begin index must occur after the end index when the step is negative.";
		}
		EX_IndexOutOfRange(ec, msg);
		return globals[0];
	}
	return value;
};

var prepareToSuspend = function(ec, stack, valueStackSize, currentPc) {
	ec[1] = stack;
	ec[2] = valueStackSize;
	stack[0] = (currentPc + 1);
	return 0;
};

var primitiveMethodsInitializeLookup = function(nameLookups) {
	var length = Object.keys(nameLookups).length;
	var lookup = PST$createNewArray(length);
	var i = 0;
	while ((i < length)) {
		lookup[i] = -1;
		i += 1;
	}
	if ((nameLookups["add"] !== undefined)) {
		lookup[nameLookups["add"]] = 0;
	}
	if ((nameLookups["argCountMax"] !== undefined)) {
		lookup[nameLookups["argCountMax"]] = 1;
	}
	if ((nameLookups["argCountMin"] !== undefined)) {
		lookup[nameLookups["argCountMin"]] = 2;
	}
	if ((nameLookups["choice"] !== undefined)) {
		lookup[nameLookups["choice"]] = 3;
	}
	if ((nameLookups["clear"] !== undefined)) {
		lookup[nameLookups["clear"]] = 4;
	}
	if ((nameLookups["clone"] !== undefined)) {
		lookup[nameLookups["clone"]] = 5;
	}
	if ((nameLookups["concat"] !== undefined)) {
		lookup[nameLookups["concat"]] = 6;
	}
	if ((nameLookups["contains"] !== undefined)) {
		lookup[nameLookups["contains"]] = 7;
	}
	if ((nameLookups["createInstance"] !== undefined)) {
		lookup[nameLookups["createInstance"]] = 8;
	}
	if ((nameLookups["endsWith"] !== undefined)) {
		lookup[nameLookups["endsWith"]] = 9;
	}
	if ((nameLookups["filter"] !== undefined)) {
		lookup[nameLookups["filter"]] = 10;
	}
	if ((nameLookups["get"] !== undefined)) {
		lookup[nameLookups["get"]] = 11;
	}
	if ((nameLookups["getName"] !== undefined)) {
		lookup[nameLookups["getName"]] = 12;
	}
	if ((nameLookups["indexOf"] !== undefined)) {
		lookup[nameLookups["indexOf"]] = 13;
	}
	if ((nameLookups["insert"] !== undefined)) {
		lookup[nameLookups["insert"]] = 14;
	}
	if ((nameLookups["invoke"] !== undefined)) {
		lookup[nameLookups["invoke"]] = 15;
	}
	if ((nameLookups["isA"] !== undefined)) {
		lookup[nameLookups["isA"]] = 16;
	}
	if ((nameLookups["join"] !== undefined)) {
		lookup[nameLookups["join"]] = 17;
	}
	if ((nameLookups["keys"] !== undefined)) {
		lookup[nameLookups["keys"]] = 18;
	}
	if ((nameLookups["lower"] !== undefined)) {
		lookup[nameLookups["lower"]] = 19;
	}
	if ((nameLookups["ltrim"] !== undefined)) {
		lookup[nameLookups["ltrim"]] = 20;
	}
	if ((nameLookups["map"] !== undefined)) {
		lookup[nameLookups["map"]] = 21;
	}
	if ((nameLookups["merge"] !== undefined)) {
		lookup[nameLookups["merge"]] = 22;
	}
	if ((nameLookups["pop"] !== undefined)) {
		lookup[nameLookups["pop"]] = 23;
	}
	if ((nameLookups["reduce"] !== undefined)) {
		lookup[nameLookups["reduce"]] = 24;
	}
	if ((nameLookups["remove"] !== undefined)) {
		lookup[nameLookups["remove"]] = 25;
	}
	if ((nameLookups["replace"] !== undefined)) {
		lookup[nameLookups["replace"]] = 26;
	}
	if ((nameLookups["reverse"] !== undefined)) {
		lookup[nameLookups["reverse"]] = 27;
	}
	if ((nameLookups["rtrim"] !== undefined)) {
		lookup[nameLookups["rtrim"]] = 28;
	}
	if ((nameLookups["shuffle"] !== undefined)) {
		lookup[nameLookups["shuffle"]] = 29;
	}
	if ((nameLookups["sort"] !== undefined)) {
		lookup[nameLookups["sort"]] = 30;
	}
	if ((nameLookups["split"] !== undefined)) {
		lookup[nameLookups["split"]] = 31;
	}
	if ((nameLookups["startsWith"] !== undefined)) {
		lookup[nameLookups["startsWith"]] = 32;
	}
	if ((nameLookups["trim"] !== undefined)) {
		lookup[nameLookups["trim"]] = 33;
	}
	if ((nameLookups["upper"] !== undefined)) {
		lookup[nameLookups["upper"]] = 34;
	}
	if ((nameLookups["values"] !== undefined)) {
		lookup[nameLookups["values"]] = 35;
	}
	return lookup;
};

var primitiveMethodWrongArgCountError = function(name, expected, actual) {
	var output = "";
	if ((expected == 0)) {
		output = name + " does not accept any arguments.";
	} else if ((expected == 1)) {
		output = name + " accepts exactly 1 argument.";
	} else {
		output = [name, " requires ", ('' + expected), " arguments."].join('');
	}
	return [output, " Found: ", ('' + actual)].join('');
};

var printToStdOut = function(prefix, line) {
	if ((prefix == null)) {
		PST$stdout(line);
	} else {
		var canonical = line.split("\r\n").join("\n").split("\r").join("\n");
		var lines = canonical.split("\n");
		var i = 0;
		while ((i < lines.length)) {
			PST$stdout([prefix, ": ", lines[i]].join(''));
			i += 1;
		}
	}
	return 0;
};

var ProcessHelper_kill = function(pid) {
	return false;
};

var ProcessHelper_processRun = function(vm, wrapper, exPath, args, onDataCb, cwd, flags) {
	if ((cwd.length == 0)) {
		cwd = null;
	}
	var intOut = PST$createNewArray(1);
	wrapper[3] = PST$createNewArray(1);
	wrapper[3][0] = (() => {})();
	return intOut[0];
};

var qsortHelper = function(keyStringList, keyNumList, indices, isString, startIndex, endIndex) {
	if (((endIndex - startIndex) <= 0)) {
		return 0;
	}
	if (((endIndex - startIndex) == 1)) {
		if (sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, endIndex)) {
			sortHelperSwap(keyStringList, keyNumList, indices, isString, startIndex, endIndex);
		}
		return 0;
	}
	var mid = ((endIndex + startIndex) >> 1);
	sortHelperSwap(keyStringList, keyNumList, indices, isString, mid, startIndex);
	var upperPointer = (endIndex + 1);
	var lowerPointer = (startIndex + 1);
	while ((upperPointer > lowerPointer)) {
		if (sortHelperIsRevOrder(keyStringList, keyNumList, isString, startIndex, lowerPointer)) {
			lowerPointer += 1;
		} else {
			upperPointer -= 1;
			sortHelperSwap(keyStringList, keyNumList, indices, isString, lowerPointer, upperPointer);
		}
	}
	var midIndex = (lowerPointer - 1);
	sortHelperSwap(keyStringList, keyNumList, indices, isString, midIndex, startIndex);
	qsortHelper(keyStringList, keyNumList, indices, isString, startIndex, (midIndex - 1));
	qsortHelper(keyStringList, keyNumList, indices, isString, (midIndex + 1), endIndex);
	return 0;
};

var queryValue = function(vm, execId, stackFrameOffset, steps) {
	if ((execId == -1)) {
		execId = vm[1];
	}
	var ec = vm[0][execId];
	var stackFrame = ec[1];
	while ((stackFrameOffset > 0)) {
		stackFrameOffset -= 1;
		stackFrame = stackFrame[4];
	}
	var current = null;
	var i = 0;
	var j = 0;
	var len = steps.length;
	i = 0;
	while ((i < steps.length)) {
		if (((current == null) && (i > 0))) {
			return null;
		}
		var step = steps[i];
		if (isStringEqual(".", step)) {
			return null;
		} else if (isStringEqual("this", step)) {
			current = stackFrame[6];
		} else if (isStringEqual("class", step)) {
			return null;
		} else if (isStringEqual("local", step)) {
			i += 1;
			step = steps[i];
			var localNamesByFuncPc = vm[3][5];
			var localNames = null;
			if (((localNamesByFuncPc == null) || (Object.keys(localNamesByFuncPc).length == 0))) {
				return null;
			}
			j = stackFrame[0];
			while ((j >= 0)) {
				if ((localNamesByFuncPc[j] !== undefined)) {
					localNames = localNamesByFuncPc[j];
					j = -1;
				}
				j -= 1;
			}
			if ((localNames == null)) {
				return null;
			}
			var localId = -1;
			if ((localNames != null)) {
				j = 0;
				while ((j < localNames.length)) {
					if (isStringEqual(localNames[j], step)) {
						localId = j;
						j = localNames.length;
					}
					j += 1;
				}
			}
			if ((localId == -1)) {
				return null;
			}
			var localOffset = (localId + stackFrame[2]);
			if ((ec[6][localOffset] != stackFrame[1])) {
				return null;
			}
			current = ec[5][localOffset];
		} else if (isStringEqual("index", step)) {
			return null;
		} else if (isStringEqual("key-int", step)) {
			return null;
		} else if (isStringEqual("key-str", step)) {
			return null;
		} else if (isStringEqual("key-obj", step)) {
			return null;
		} else {
			return null;
		}
		i += 1;
	}
	return current;
};

var read_integer = function(pindex, raw, length, alphaNums) {
	var num = 0;
	var c = raw.charAt(pindex[0]);
	pindex[0] = (pindex[0] + 1);
	if ((c == "%")) {
		var value = read_till(pindex, raw, length, "%");
		num = parseInt(value);
	} else if ((c == "@")) {
		num = read_integer(pindex, raw, length, alphaNums);
		num *= 62;
		num += read_integer(pindex, raw, length, alphaNums);
	} else if ((c == "#")) {
		num = read_integer(pindex, raw, length, alphaNums);
		num *= 62;
		num += read_integer(pindex, raw, length, alphaNums);
		num *= 62;
		num += read_integer(pindex, raw, length, alphaNums);
	} else if ((c == "^")) {
		num = (-1 * read_integer(pindex, raw, length, alphaNums));
	} else {
		// TODO: string.IndexOfChar(c);
		num = alphaNums.indexOf(c);
		if ((num == -1)) {
		}
	}
	return num;
};

var read_string = function(pindex, raw, length, alphaNums) {
	var b64 = read_till(pindex, raw, length, "%");
	return decodeURIComponent(Array.prototype.map.call(atob(b64), function(c) { return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2); }).join(''));
};

var read_till = function(index, raw, length, end) {
	var output = [];
	var ctn = true;
	var c = " ";
	while (ctn) {
		c = raw.charAt(index[0]);
		if ((c == end)) {
			ctn = false;
		} else {
			output.push(c);
		}
		index[0] = (index[0] + 1);
	}
	return output.join('');
};

var reallocIntArray = function(original, requiredCapacity) {
	var oldSize = original.length;
	var size = oldSize;
	while ((size < requiredCapacity)) {
		size *= 2;
	}
	var output = PST$createNewArray(size);
	var i = 0;
	while ((i < oldSize)) {
		output[i] = original[i];
		i += 1;
	}
	return output;
};

var Reflect_allClasses = function(vm) {
	var generics = PST$createNewArray(1);
	generics[0] = 10;
	var output = makeEmptyList(generics, 20);
	var classTable = vm[4][9];
	var i = 1;
	while ((i < classTable.length)) {
		var classInfo = classTable[i];
		if ((classInfo == null)) {
			i = classTable.length;
		} else {
			addToList(output, [10, [false, classInfo[0]]]);
		}
		i += 1;
	}
	return [6, output];
};

var Reflect_getMethods = function(vm, ec, methodSource) {
	var output = makeEmptyList(null, 8);
	if ((methodSource[0] == 8)) {
		var objInstance1 = methodSource[1];
		var classInfo = vm[4][9][objInstance1[0]];
		var i = 0;
		while ((i < classInfo[9].length)) {
			var functionId = classInfo[9][i];
			if ((functionId != -1)) {
				addToList(output, [9, [2, methodSource, objInstance1[0], functionId, null]]);
			}
			i += 1;
		}
	} else {
		var classValue = methodSource[1];
		var classInfo = vm[4][9][classValue[1]];
		EX_UnsupportedOperation(ec, "static method reflection not implemented yet.");
	}
	return [6, output];
};

var Reflect_getNamespaceFunctions = function(vm, ns, output) {
	return false;
};

var registerNamedCallback = function(vm, scope, functionName, callback) {
	var id = getNamedCallbackIdImpl(vm, scope, functionName, true);
	vm[12][0][id] = callback;
	return id;
};

var resetLocalsStackTokens = function(ec, stack) {
	var localsStack = ec[5];
	var localsStackSet = ec[6];
	var i = stack[3];
	while ((i < localsStackSet.length)) {
		localsStackSet[i] = 0;
		localsStack[i] = null;
		i += 1;
	}
	var stackWalker = stack;
	while ((stackWalker != null)) {
		var token = stackWalker[1];
		stackWalker[1] = 1;
		i = stackWalker[2];
		while ((i < stackWalker[3])) {
			if ((localsStackSet[i] == token)) {
				localsStackSet[i] = 1;
			} else {
				localsStackSet[i] = 0;
				localsStack[i] = null;
			}
			i += 1;
		}
		stackWalker = stackWalker[4];
	}
	ec[7] = 1;
	return -1;
};

var resolvePrimitiveMethodName2 = function(lookup, type, globalNameId) {
	var output = lookup[globalNameId];
	if ((output != -1)) {
		switch ((type + (11 * output))) {
			case 82:
				return output;
			case 104:
				return output;
			case 148:
				return output;
			case 214:
				return output;
			case 225:
				return output;
			case 291:
				return output;
			case 302:
				return output;
			case 313:
				return output;
			case 346:
				return output;
			case 357:
				return output;
			case 368:
				return output;
			case 379:
				return output;
			case 6:
				return output;
			case 39:
				return output;
			case 50:
				return output;
			case 61:
				return output;
			case 72:
				return output;
			case 83:
				return output;
			case 116:
				return output;
			case 160:
				return output;
			case 193:
				return output;
			case 237:
				return output;
			case 259:
				return output;
			case 270:
				return output;
			case 281:
				return output;
			case 303:
				return output;
			case 325:
				return output;
			case 336:
				return output;
			case 51:
				return output;
			case 62:
				return output;
			case 84:
				return output;
			case 128:
				return output;
			case 205:
				return output;
			case 249:
				return output;
			case 282:
				return output;
			case 392:
				return output;
			case 20:
				return output;
			case 31:
				return output;
			case 141:
				return output;
			case 174:
				return output;
			case 98:
				return output;
			case 142:
				return output;
			case 186:
				return output;
			default:
				return -1;
		}
	}
	return -1;
};

var resource_manager_getResourceOfType = function(vm, userPath, type) {
	var db = vm[9];
	var lookup = db[1];
	if ((lookup[userPath] !== undefined)) {
		var output = makeEmptyList(null, 2);
		var file = lookup[userPath];
		if (file[3] == type) {
			addToList(output, vm[13][1]);
			addToList(output, buildString(vm[13], file[1]));
		} else {
			addToList(output, vm[13][2]);
		}
		return [6, output];
	}
	return vm[13][0];
};

var resource_manager_populate_directory_lookup = function(dirs, path) {
	var parts = path.split("/");
	var pathBuilder = "";
	var file = "";
	var i = 0;
	while ((i < parts.length)) {
		file = parts[i];
		var files = null;
		if (!(dirs[pathBuilder] !== undefined)) {
			files = [];
			dirs[pathBuilder] = files;
		} else {
			files = dirs[pathBuilder];
		}
		files.push(file);
		if ((i > 0)) {
			pathBuilder = [pathBuilder, "/", file].join('');
		} else {
			pathBuilder = file;
		}
		i += 1;
	}
	return 0;
};

var resourceManagerInitialize = function(globals, manifest, imageAtlasManifest) {
	var filesPerDirectoryBuilder = {};
	var fileInfo = {};
	var dataList = [];
	var items = manifest.split("\n");
	var resourceInfo = null;
	var type = "";
	var userPath = "";
	var internalPath = "";
	var argument = "";
	var isText = false;
	var intType = 0;
	var i = 0;
	while ((i < items.length)) {
		var itemData = items[i].split(",");
		if ((itemData.length >= 3)) {
			type = itemData[0];
			isText = "TXT" == type;
			if (isText) {
				intType = 1;
			} else if (("IMGSH" == type || "IMG" == type)) {
				intType = 2;
			} else if ("SND" == type) {
				intType = 3;
			} else if ("TTF" == type) {
				intType = 4;
			} else if ("BIN" == type) {
				intType = 5;
			} else {
				intType = 6;
			}
			userPath = stringDecode(itemData[1]);
			internalPath = itemData[2];
			argument = "";
			if ((itemData.length > 3)) {
				argument = stringDecode(itemData[3]);
			}
			resourceInfo = [userPath, internalPath, isText, type, argument];
			fileInfo[userPath] = resourceInfo;
			resource_manager_populate_directory_lookup(filesPerDirectoryBuilder, userPath);
			dataList.push(buildString(globals, userPath));
			dataList.push(buildInteger(globals, intType));
			if ((internalPath != null)) {
				dataList.push(buildString(globals, internalPath));
			} else {
				dataList.push(globals[0]);
			}
		}
		i += 1;
	}
	var dirs = PST$dictionaryKeys(filesPerDirectoryBuilder);
	var filesPerDirectorySorted = {};
	i = 0;
	while ((i < dirs.length)) {
		var dir = dirs[i];
		var unsortedDirs = filesPerDirectoryBuilder[dir];
		var dirsSorted = PST$multiplyList(unsortedDirs, 1);
		dirsSorted = PST$sortedCopyOfArray(dirsSorted);
		filesPerDirectorySorted[dir] = dirsSorted;
		i += 1;
	}
	return [filesPerDirectorySorted, fileInfo, dataList, imageAtlasManifest];
};

var reverseList = function(list) {
	list[2].reverse();
};

var runInterpreter = function(vm, executionContextId) {
	var result = interpret(vm, executionContextId);
	result[3] = executionContextId;
	var status = result[0];
	if ((status == 1)) {
		if ((vm[0][executionContextId] !== undefined)) {
			delete vm[0][executionContextId];
		}
		runShutdownHandlers(vm);
	} else if ((status == 3)) {
		printToStdOut(vm[11][3], result[1]);
		runShutdownHandlers(vm);
	}
	if ((executionContextId == 0)) {
		result[4] = true;
	}
	return result;
};

var runInterpreterWithFunctionPointer = function(vm, fpValue, args) {
	var newId = (vm[1] + 1);
	vm[1] = newId;
	var argList = [];
	var i = 0;
	while ((i < args.length)) {
		argList.push(args[i]);
		i += 1;
	}
	var locals = PST$createNewArray(0);
	var localsSet = PST$createNewArray(0);
	var valueStack = PST$createNewArray(100);
	valueStack[0] = fpValue;
	valueStack[1] = buildList(argList);
	var stack = [(vm[2][0].length - 2), 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null];
	var executionContext = [newId, stack, 2, 100, valueStack, locals, localsSet, 1, 0, false, null, false, 0, null];
	vm[0][newId] = executionContext;
	return runInterpreter(vm, newId);
};

var runShutdownHandlers = function(vm) {
	while ((vm[10].length > 0)) {
		var handler = vm[10][0];
		vm[10].splice(0, 1);
		runInterpreterWithFunctionPointer(vm, handler, PST$createNewArray(0));
	}
	return 0;
};

var setItemInList = function(list, i, v) {
	list[2][i] = v;
};

var setVmId = function(vm, id) {
	vm[17] = id;
};

var sortHelperIsRevOrder = function(keyStringList, keyNumList, isString, indexLeft, indexRight) {
	if (isString) {
		return (keyStringList[indexLeft].localeCompare(keyStringList[indexRight]) > 0);
	}
	return (keyNumList[indexLeft] > keyNumList[indexRight]);
};

var sortHelperSwap = function(keyStringList, keyNumList, indices, isString, index1, index2) {
	if ((index1 == index2)) {
		return 0;
	}
	var t = indices[index1];
	indices[index1] = indices[index2];
	indices[index2] = t;
	if (isString) {
		var s = keyStringList[index1];
		keyStringList[index1] = keyStringList[index2];
		keyStringList[index2] = s;
	} else {
		var n = keyNumList[index1];
		keyNumList[index1] = keyNumList[index2];
		keyNumList[index2] = n;
	}
	return 0;
};

var sortLists = function(keyList, parallelList, intOutParam) {
	PST$intBuffer16[0] = 0;
	var length = keyList[1];
	if ((length < 2)) {
		return 0;
	}
	var i = 0;
	var item = null;
	item = keyList[2][0];
	var isString = (item[0] == 5);
	var stringKeys = null;
	var numKeys = null;
	if (isString) {
		stringKeys = PST$createNewArray(length);
	} else {
		numKeys = PST$createNewArray(length);
	}
	var indices = PST$createNewArray(length);
	var originalOrder = PST$createNewArray(length);
	i = 0;
	while ((i < length)) {
		indices[i] = i;
		originalOrder[i] = parallelList[2][i];
		item = keyList[2][i];
		switch (item[0]) {
			case 3:
				if (isString) {
					PST$intBuffer16[0] = 1;
					return 0;
				}
				numKeys[i] = item[1];
				break;
			case 4:
				if (isString) {
					PST$intBuffer16[0] = 1;
					return 0;
				}
				numKeys[i] = item[1];
				break;
			case 5:
				if (!isString) {
					PST$intBuffer16[0] = 1;
					return 0;
				}
				stringKeys[i] = item[1];
				break;
			default:
				PST$intBuffer16[0] = 1;
				return 0;
		}
		i += 1;
	}
	qsortHelper(stringKeys, numKeys, indices, isString, 0, (length - 1));
	i = 0;
	while ((i < length)) {
		parallelList[2][i] = originalOrder[indices[i]];
		i += 1;
	}
	return 0;
};

var SRandomQueuePopulate = function(globals, seed, queue, size) {
	var sign = 1;
	var num = 0;
	while ((size > 0)) {
		size -= 1;
		if (((seed & 2) == 0)) {
			sign = -1;
		} else {
			sign = 1;
		}
		num = ((seed >> 8) & 255);
		seed = (((seed * 20077) + 12345) & 65535);
		num = ((num * 256) + ((seed >> 8) & 255));
		seed = ((((seed * 20077) + 12345)) & 65535);
		num = ((num * 256) + ((seed >> 8) & 255));
		seed = ((((seed * 20077) + 12345)) & 65535);
		num = ((num * 256) + ((seed >> 8) & 255));
		seed = ((((seed * 20077) + 12345)) & 65535);
		addToList(queue, buildInteger(globals, (sign * num)));
	}
	return seed;
};

var stackItemIsLibrary = function(stackInfo) {
	if ((stackInfo.charAt(0) != "[")) {
		return false;
	}
	var cIndex = stackInfo.indexOf(":");
	return ((cIndex > 0) && (cIndex < stackInfo.indexOf("]")));
};

var startVm = function(vm) {
	return runInterpreter(vm, vm[1]);
};

var stringDecode = function(encoded) {
	if (!(encoded.indexOf("%") != -1)) {
		var length = encoded.length;
		var per = "%";
		var builder = [];
		var i = 0;
		while ((i < length)) {
			var c = encoded.charAt(i);
			if (((c == per) && ((i + 2) < length))) {
				builder.push(stringFromHex(["", encoded.charAt((i + 1)), encoded.charAt((i + 2))].join('')));
			} else {
				builder.push("" + c);
			}
			i += 1;
		}
		return builder.join("");
	}
	return encoded;
};

var stringFromHex = function(encoded) {
	encoded = encoded.toUpperCase();
	var hex = "0123456789ABCDEF";
	var output = [];
	var length = encoded.length;
	var a = 0;
	var b = 0;
	var c = null;
	var i = 0;
	while (((i + 1) < length)) {
		c = "" + encoded.charAt(i);
		a = hex.indexOf(c);
		if ((a == -1)) {
			return null;
		}
		c = "" + encoded.charAt((i + 1));
		b = hex.indexOf(c);
		if ((b == -1)) {
			return null;
		}
		a = ((a * 16) + b);
		output.push(String.fromCharCode(a));
		i += 2;
	}
	return output.join("");
};

var suspendInterpreter = function() {
	return [2, null, 0.0, 0, false, ""];
};

var textencoding_convertBytesToText = function(vm, arg1, arg2, arg3) {
	if ((arg1[0] != 6)) {
		return buildInteger(vm[13], 2);
	}
	var byteList = arg1[1];
	var format = arg2[1];
	var output = arg3[1];
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
	var sc = COMMON.textEncoding.bytesToText(unwrappedBytes, format, strOut);
	if ((sc == 0)) {
		addToList(output, buildString(vm[13], strOut[0]));
	}
	return buildInteger(vm[13], sc);
};

var textencoding_convertTextToBytes = function(vm, arg1, arg2, arg3, arg4) {
	var value = arg1[1];
	var format = arg2[1];
	var includeBom = arg3[1];
	var output = arg4[1];
	var byteList = [];
	var intOut = PST$intBuffer16;
	var sc = COMMON.textEncoding.textToBytes(value, includeBom, format, byteList, vm[13][9], intOut);
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

var tokenDataImpl = function(vm, row) {
	var tokensByPc = vm[3][0];
	var pc = (row[0] + vm[4][15]);
	var line = row[1];
	var col = row[2];
	var file = row[3];
	var tokens = tokensByPc[pc];
	if ((tokens == null)) {
		tokens = [];
		tokensByPc[pc] = tokens;
	}
	tokens.push([line, col, file]);
	return 0;
};

var tokenHelperConvertPcsToStackTraceStrings = function(vm, pcs) {
	var tokens = generateTokenListFromPcs(vm, pcs);
	var files = vm[3][1];
	var output = [];
	var i = 0;
	while ((i < tokens.length)) {
		var token = tokens[i];
		if ((token == null)) {
			output.push("[No stack information]");
		} else {
			var line = token[0];
			var col = token[1];
			var fileData = files[token[2]];
			var lines = fileData.split("\n");
			var filename = lines[0];
			var linevalue = lines[(line + 1)];
			output.push([filename, ", Line: ", ('' + (line + 1)), ", Col: ", ('' + (col + 1))].join(''));
		}
		i += 1;
	}
	return output;
};

var tokenHelperGetFileLine = function(vm, fileId, lineNum) {
	var sourceCode = vm[3][1][fileId];
	if ((sourceCode == null)) {
		return null;
	}
	return sourceCode.split("\n")[lineNum];
};

var tokenHelperGetFormattedPointerToToken = function(vm, token) {
	var line = tokenHelperGetFileLine(vm, token[2], (token[0] + 1));
	if ((line == null)) {
		return null;
	}
	var columnIndex = token[1];
	var lineLength = line.length;
	line = PST$stringTrimOneSide(line, true);
	line = line.split("\t").join(" ");
	var offset = (lineLength - line.length);
	columnIndex -= offset;
	var line2 = "";
	while ((columnIndex > 0)) {
		columnIndex -= 1;
		line2 = line2 + " ";
	}
	line2 = line2 + "^";
	return [line, "\n", line2].join('');
};

var tokenHelplerIsFilePathLibrary = function(vm, fileId, allFiles) {
	var filename = tokenHelperGetFileLine(vm, fileId, 0);
	return !PST$stringEndsWith(filename.toLowerCase(), ".cry");
};

var typeInfoToString = function(vm, typeInfo, i) {
	var output = [];
	typeToStringBuilder(vm, output, typeInfo, i);
	return output.join("");
};

var typeToString = function(vm, typeInfo, i) {
	var sb = [];
	typeToStringBuilder(vm, sb, typeInfo, i);
	return sb.join("");
};

var typeToStringBuilder = function(vm, sb, typeInfo, i) {
	switch (typeInfo[i]) {
		case -1:
			sb.push("void");
			return (i + 1);
		case 0:
			sb.push("object");
			return (i + 1);
		case 1:
			sb.push("object");
			return (i + 1);
		case 3:
			sb.push("int");
			return (i + 1);
		case 4:
			sb.push("float");
			return (i + 1);
		case 2:
			sb.push("bool");
			return (i + 1);
		case 5:
			sb.push("string");
			return (i + 1);
		case 6:
			sb.push("List<");
			i = typeToStringBuilder(vm, sb, typeInfo, (i + 1));
			sb.push(">");
			return i;
		case 7:
			sb.push("Dictionary<");
			i = typeToStringBuilder(vm, sb, typeInfo, (i + 1));
			sb.push(", ");
			i = typeToStringBuilder(vm, sb, typeInfo, i);
			sb.push(">");
			return i;
		case 8:
			var classId = typeInfo[(i + 1)];
			if ((classId == 0)) {
				sb.push("object");
			} else {
				var classInfo = vm[4][9][classId];
				sb.push(classInfo[16]);
			}
			return (i + 2);
		case 10:
			sb.push("Class");
			return (i + 1);
		case 9:
			var n = typeInfo[(i + 1)];
			var optCount = typeInfo[(i + 2)];
			i += 2;
			sb.push("function(");
			var ret = [];
			i = typeToStringBuilder(vm, ret, typeInfo, i);
			var j = 1;
			while ((j < n)) {
				if ((j > 1)) {
					sb.push(", ");
				}
				i = typeToStringBuilder(vm, sb, typeInfo, i);
				j += 1;
			}
			if ((n == 1)) {
				sb.push("void");
			}
			sb.push(" => ");
			var optStart = (n - optCount - 1);
			j = 0;
			while ((j < ret.length)) {
				if ((j >= optStart)) {
					sb.push("(opt) ");
				}
				sb.push(ret[j]);
				j += 1;
			}
			sb.push(")");
			return i;
		default:
			sb.push("UNKNOWN");
			return (i + 1);
	}
};

var typeToStringFromValue = function(vm, value) {
	var sb = null;
	switch (value[0]) {
		case 1:
			return "null";
		case 2:
			return "bool";
		case 3:
			return "int";
		case 4:
			return "float";
		case 5:
			return "string";
		case 10:
			return "class";
		case 8:
			var classId = (value[1])[0];
			var classInfo = vm[4][9][classId];
			return classInfo[16];
		case 6:
			sb = [];
			sb.push("List<");
			var list = value[1];
			if ((list[0] == null)) {
				sb.push("object");
			} else {
				typeToStringBuilder(vm, sb, list[0], 0);
			}
			sb.push(">");
			return sb.join("");
		case 7:
			var dict = value[1];
			sb = [];
			sb.push("Dictionary<");
			switch (dict[1]) {
				case 3:
					sb.push("int");
					break;
				case 5:
					sb.push("string");
					break;
				case 8:
					sb.push("object");
					break;
				default:
					sb.push("???");
					break;
			}
			sb.push(", ");
			if ((dict[3] == null)) {
				sb.push("object");
			} else {
				typeToStringBuilder(vm, sb, dict[3], 0);
			}
			sb.push(">");
			return sb.join("");
		case 9:
			return "Function";
		default:
			return "Unknown";
	}
};

var uint32Hack = function(left, right) {
	return (((left << 16)) | right);
};

var uncaughtExceptionResult = function(vm, exception) {
	return [3, unrollExceptionOutput(vm, exception), 0.0, 0, false, ""];
};

var unrollExceptionOutput = function(vm, exceptionInstance) {
	var objInstance = exceptionInstance[1];
	var classInfo = vm[4][9][objInstance[0]];
	var pcs = objInstance[3][1];
	var codeFormattedPointer = "";
	var exceptionName = classInfo[16];
	var message = valueToString(vm, objInstance[2][1]);
	var trace = tokenHelperConvertPcsToStackTraceStrings(vm, pcs);
	trace.pop();
	trace.push("Stack Trace:");
	trace.reverse();
	pcs.reverse();
	var showLibStack = vm[11][1];
	if ((!showLibStack && !stackItemIsLibrary(trace[0]))) {
		while (stackItemIsLibrary(trace[(trace.length - 1)])) {
			trace.pop();
			pcs.pop();
		}
	}
	if (((trace.length > 0) && (trace[(trace.length - 1)].indexOf("[Core:exceptions.cry]") == 0))) {
		trace.pop();
		pcs.pop();
	}
	var tokensAtPc = vm[3][0][pcs[(pcs.length - 1)]];
	if ((tokensAtPc != null)) {
		codeFormattedPointer = "\n\n" + tokenHelperGetFormattedPointerToToken(vm, tokensAtPc[0]);
	}
	var stackTrace = trace.join("\n");
	return [stackTrace, codeFormattedPointer, "\n", exceptionName, ": ", message].join('');
};

var valueConcatLists = function(a, b) {
	return [null, (a[1] + b[1]), a[2].concat(b[2])];
};

var valueMultiplyList = function(a, n) {
	var _len = (a[1] * n);
	var output = makeEmptyList(a[0], _len);
	if ((_len == 0)) {
		return output;
	}
	var aLen = a[1];
	var i = 0;
	var value = null;
	if ((aLen == 1)) {
		value = a[2][0];
		i = 0;
		while ((i < n)) {
			output[2].push(value);
			i += 1;
		}
	} else {
		var j = 0;
		i = 0;
		while ((i < n)) {
			j = 0;
			while ((j < aLen)) {
				output[2].push(a[2][j]);
				j += 1;
			}
			i += 1;
		}
	}
	output[1] = _len;
	return output;
};

var valueStackIncreaseCapacity = function(ec) {
	var stack = ec[4];
	var oldCapacity = stack.length;
	var newCapacity = (oldCapacity * 2);
	var newStack = PST$createNewArray(newCapacity);
	var i = (oldCapacity - 1);
	while ((i >= 0)) {
		newStack[i] = stack[i];
		i -= 1;
	}
	ec[4] = newStack;
	return newStack;
};

var valueToString = function(vm, wrappedValue) {
	var type = wrappedValue[0];
	if ((type == 1)) {
		return "null";
	}
	if ((type == 2)) {
		if (wrappedValue[1]) {
			return "true";
		}
		return "false";
	}
	if ((type == 4)) {
		var floatStr = '' + wrappedValue[1];
		if (!(floatStr.indexOf(".") != -1)) {
			floatStr += ".0";
		}
		return floatStr;
	}
	if ((type == 3)) {
		return ('' + wrappedValue[1]);
	}
	if ((type == 5)) {
		return wrappedValue[1];
	}
	if ((type == 6)) {
		var internalList = wrappedValue[1];
		var output = "[";
		var i = 0;
		while ((i < internalList[1])) {
			if ((i > 0)) {
				output += ", ";
			}
			output += valueToString(vm, internalList[2][i]);
			i += 1;
		}
		output += "]";
		return output;
	}
	if ((type == 8)) {
		var objInstance = wrappedValue[1];
		var classId = objInstance[0];
		var ptr = objInstance[1];
		var classInfo = vm[4][9][classId];
		var nameId = classInfo[1];
		var className = vm[4][0][nameId];
		return ["Instance<", className, "#", ('' + ptr), ">"].join('');
	}
	if ((type == 7)) {
		var dict = wrappedValue[1];
		if ((dict[0] == 0)) {
			return "{}";
		}
		var output = "{";
		var keyList = dict[6];
		var valueList = dict[7];
		var i = 0;
		while ((i < dict[0])) {
			if ((i > 0)) {
				output += ", ";
			}
			output += [valueToString(vm, dict[6][i]), ": ", valueToString(vm, dict[7][i])].join('');
			i += 1;
		}
		output += " }";
		return output;
	}
	if ((type == 9)) {
		var fp = wrappedValue[1];
		switch (fp[0]) {
			case 1:
				return "<FunctionPointer>";
			case 2:
				return "<ClassMethodPointer>";
			case 3:
				return "<ClassStaticMethodPointer>";
			case 4:
				return "<PrimitiveMethodPointer>";
			case 5:
				return "<Lambda>";
			default:
				return "<UnknownFunctionPointer>";
		}
	}
	return "<unknown>";
};

var vm_getCurrentExecutionContextId = function(vm) {
	return vm[1];
};

var vm_suspend_context_by_id = function(vm, execId, status) {
	return vm_suspend_for_context(getExecutionContext(vm, execId), 1);
};

var vm_suspend_for_context = function(ec, status) {
	ec[11] = true;
	ec[12] = status;
	return 0;
};

var vm_suspend_with_status_by_id = function(vm, execId, status) {
	return vm_suspend_for_context(getExecutionContext(vm, execId), status);
};

var vmEnableLibStackTrace = function(vm) {
	vm[11][1] = true;
};

var vmEnvSetCommandLineArgs = function(vm, args) {
	vm[11][0] = args;
};

var vmGetEventLoopObj = function(vm) {
	return vm[11][4];
};

var vmGetGlobals = function(vm) {
	return vm[13];
};

var vmGetResourceReaderObj = function(vm) {
	return vm[11][5];
};

var vmGetWaxResponse = function(vm) {
	return vm[11][8];
};

var vmSetEventLoopObj = function(vm, evLoop) {
	vm[11][4] = evLoop;
};

var vmSetResourceReaderObj = function(vm, rr) {
	vm[11][5] = rr;
};

var vmSetWaxHub = function(vm, wh) {
	vm[11][6] = wh;
};

var vmSetWaxPayload = function(vm, v) {
	vm[11][7] = v;
};

var xml_ampUnescape = function(value, entityLookup) {
	var ampParts = value.split("&");
	var i = 1;
	while ((i < ampParts.length)) {
		var component = ampParts[i];
		var semicolon = component.indexOf(";");
		if ((semicolon != -1)) {
			var entityCode = component.substring(0, 0 + semicolon);
			var entityValue = xml_getEntity(entityCode, entityLookup);
			if ((entityValue == null)) {
				entityValue = "&";
			} else {
				component = component.substring((semicolon + 1), (semicolon + 1) + ((component.length - semicolon - 1)));
			}
			ampParts[i] = entityValue + component;
		}
		i += 1;
	}
	return ampParts.join("");
};

var xml_error = function(xml, index, msg) {
	var loc = "";
	if ((index < xml.length)) {
		var line = 1;
		var col = 0;
		var i = 0;
		while ((i <= index)) {
			if ((xml.charAt(i) == "\n")) {
				line += 1;
				col = 0;
			} else {
				col += 1;
			}
			i += 1;
		}
		loc = [" on line ", ('' + line), ", col ", ('' + col)].join('');
	}
	return ["XML parse error", loc, ": ", msg].join('');
};

var xml_getEntity = function(code, entityLookup) {
	if ((entityLookup[code] !== undefined)) {
		return entityLookup[code];
	}
	return null;
};

var xml_isNext = function(xml, indexPtr, value) {
	return PST$checksubstring(xml, indexPtr[0], value);
};

var xml_parse = function(vm, xml) {
	var entityLookup = {};
	entityLookup["amp"] = "&";
	entityLookup["lt"] = "<";
	entityLookup["gt"] = ">";
	entityLookup["quot"] = "\"";
	entityLookup["apos"] = "'";
	var stringEnders = {};
	stringEnders[(" ").charCodeAt(0)] = 1;
	stringEnders[("\"").charCodeAt(0)] = 1;
	stringEnders[("'").charCodeAt(0)] = 1;
	stringEnders[("<").charCodeAt(0)] = 1;
	stringEnders[(">").charCodeAt(0)] = 1;
	stringEnders[("\t").charCodeAt(0)] = 1;
	stringEnders[("\r").charCodeAt(0)] = 1;
	stringEnders[("\n").charCodeAt(0)] = 1;
	stringEnders[("/").charCodeAt(0)] = 1;
	var output = [];
	var errMsg = xml_parseImpl(vm, xml, PST$createNewArray(1), output, entityLookup, stringEnders);
	if ((errMsg != null)) {
		return buildString(vm[13], errMsg);
	}
	return buildList(output);
};

var xml_parseElement = function(vm, xml, indexPtr, output, entityLookup, stringEnders) {
	var length = xml.length;
	var attributeKeys = [];
	var attributeValues = [];
	var children = [];
	var element = [];
	var error = null;
	if (!xml_popIfPresent(xml, indexPtr, "<")) {
		return xml_error(xml, indexPtr[0], "Expected: '<'");
	}
	var name = xml_popName(xml, indexPtr);
	xml_skipWhitespace(xml, indexPtr);
	var hasClosingTag = true;
	while (true) {
		if ((indexPtr[0] >= length)) {
			return xml_error(xml, length, "Unexpected EOF");
		}
		if (xml_popIfPresent(xml, indexPtr, ">")) {
			break;
		}
		if (xml_popIfPresent(xml, indexPtr, "/>")) {
			hasClosingTag = false;
			break;
		}
		var key = xml_popName(xml, indexPtr);
		if ((key.length == 0)) {
			return xml_error(xml, indexPtr[0], "Expected attribute name.");
		}
		attributeKeys.push(buildString(vm[13], key));
		xml_skipWhitespace(xml, indexPtr);
		if (!xml_popIfPresent(xml, indexPtr, "=")) {
			return xml_error(xml, indexPtr[0], "Expected: '='");
		}
		xml_skipWhitespace(xml, indexPtr);
		error = xml_popString(vm, xml, indexPtr, attributeValues, entityLookup, stringEnders);
		if ((error != null)) {
			return error;
		}
		xml_skipWhitespace(xml, indexPtr);
	}
	if (hasClosingTag) {
		var close = ["</", name, ">"].join('');
		while (!xml_popIfPresent(xml, indexPtr, close)) {
			if (xml_isNext(xml, indexPtr, "</")) {
				error = xml_error(xml, (indexPtr[0] - 2), "Unexpected close tag.");
			} else if (xml_isNext(xml, indexPtr, "<!--")) {
				error = xml_skipComment(xml, indexPtr);
			} else if (xml_isNext(xml, indexPtr, "<")) {
				error = xml_parseElement(vm, xml, indexPtr, children, entityLookup, stringEnders);
			} else {
				error = xml_parseText(vm, xml, indexPtr, children, entityLookup);
			}
			if (((error == null) && (indexPtr[0] >= length))) {
				error = xml_error(xml, length, "Unexpected EOF. Unclosed tag.");
			}
			if ((error != null)) {
				return error;
			}
		}
	}
	element.push(vm[15]);
	element.push(buildString(vm[13], name));
	element.push(buildList(attributeKeys));
	element.push(buildList(attributeValues));
	element.push(buildList(children));
	output.push(buildList(element));
	return null;
};

var xml_parseImpl = function(vm, input, indexPtr, output, entityLookup, stringEnders) {
	indexPtr[0] = 0;
	xml_skipWhitespace(input, indexPtr);
	if (xml_popIfPresent(input, indexPtr, "<?xml")) {
		var newBegin = input.indexOf("?>");
		if ((newBegin == -1)) {
			return xml_error(input, (indexPtr[0] - 5), "XML Declaration is not closed.");
		}
		indexPtr[0] = (newBegin + 2);
	}
	var error = xml_skipStuff(input, indexPtr);
	if ((error != null)) {
		return error;
	}
	error = xml_parseElement(vm, input, indexPtr, output, entityLookup, stringEnders);
	if ((error != null)) {
		return error;
	}
	xml_skipStuff(input, indexPtr);
	if ((indexPtr[0] != input.length)) {
		return xml_error(input, indexPtr[0], "Unexpected text.");
	}
	return null;
};

var xml_parseText = function(vm, xml, indexPtr, output, entityLookup) {
	var length = xml.length;
	var start = indexPtr[0];
	var i = start;
	var ampFound = false;
	var c = " ";
	while ((i < length)) {
		c = xml.charAt(i);
		if ((c == "<")) {
			break;
		} else if ((c == "&")) {
			ampFound = true;
		}
		i += 1;
	}
	if ((i > start)) {
		indexPtr[0] = i;
		var textValue = xml.substring(start, start + (i - start));
		if (ampFound) {
			textValue = xml_ampUnescape(textValue, entityLookup);
		}
		var textElement = [];
		textElement.push(vm[16]);
		textElement.push(buildString(vm[13], textValue));
		output.push(buildList(textElement));
	}
	return null;
};

var xml_popIfPresent = function(xml, indexPtr, s) {
	if (PST$checksubstring(xml, indexPtr[0], s)) {
		indexPtr[0] = (indexPtr[0] + s.length);
		return true;
	}
	return false;
};

var xml_popName = function(xml, indexPtr) {
	var length = xml.length;
	var i = indexPtr[0];
	var start = i;
	var c = " ";
	while ((i < length)) {
		c = xml.charAt(i);
		if ((((c >= "a") && (c <= "z")) || ((c >= "A") && (c <= "Z")) || ((c >= "0") && (c <= "9")) || (c == "_") || (c == ".") || (c == ":") || (c == "-"))) {
		} else {
			break;
		}
		i += 1;
	}
	var output = xml.substring(start, start + (i - start));
	indexPtr[0] = i;
	return output;
};

var xml_popString = function(vm, xml, indexPtr, attributeValueOut, entityLookup, stringEnders) {
	var length = xml.length;
	var start = indexPtr[0];
	var end = length;
	var i = start;
	var stringType = xml.charCodeAt(i);
	var unwrapped = ((stringType != ("\"").charCodeAt(0)) && (stringType != ("'").charCodeAt(0)));
	var ampFound = false;
	var c = (" ").charCodeAt(0);
	if (unwrapped) {
		while ((i < length)) {
			c = xml.charCodeAt(i);
			if ((stringEnders[c] !== undefined)) {
				end = i;
				break;
			} else if ((c == ("&").charCodeAt(0))) {
				ampFound = true;
			}
			i += 1;
		}
	} else {
		i += 1;
		start = i;
		while ((i < length)) {
			c = xml.charCodeAt(i);
			if ((c == stringType)) {
				end = i;
				i += 1;
				break;
			} else if ((c == ("&").charCodeAt(0))) {
				ampFound = true;
			}
			i += 1;
		}
	}
	indexPtr[0] = i;
	var output = xml.substring(start, start + (end - start));
	if (ampFound) {
		output = xml_ampUnescape(output, entityLookup);
	}
	attributeValueOut.push(buildString(vm[13], output));
	return null;
};

var xml_skipComment = function(xml, indexPtr) {
	if (xml_popIfPresent(xml, indexPtr, "<!--")) {
		var i = xml.indexOf("-->", indexPtr[0]);
		if ((i == -1)) {
			return xml_error(xml, (indexPtr[0] - 4), "Unclosed comment.");
		}
		indexPtr[0] = (i + 3);
	}
	return null;
};

var xml_skipStuff = function(xml, indexPtr) {
	var index = (indexPtr[0] - 1);
	while ((index < indexPtr[0])) {
		index = indexPtr[0];
		xml_skipWhitespace(xml, indexPtr);
		var error = xml_skipComment(xml, indexPtr);
		if ((error != null)) {
			return error;
		}
	}
	return null;
};

var xml_skipWhitespace = function(xml, indexPtr) {
	var length = xml.length;
	var i = indexPtr[0];
	while ((i < length)) {
		var c = xml.charAt(i);
		if (((c != " ") && (c != "\t") && (c != "\n") && (c != "\r"))) {
			indexPtr[0] = i;
			return 0;
		}
		i += 1;
	}
	indexPtr[0] = i;
	return 0;
};
