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

var v_addLiteralImpl = function(v_vm, v_row, v_stringArg) {
	var v_g = v_vm[12];
	var v_type = v_row[0];
	if ((v_type == 1)) {
		v_vm[4][4].push(v_g[0]);
	} else {
		if ((v_type == 2)) {
			v_vm[4][4].push(v_buildBoolean(v_g, (v_row[1] == 1)));
		} else {
			if ((v_type == 3)) {
				v_vm[4][4].push(v_buildInteger(v_g, v_row[1]));
			} else {
				if ((v_type == 4)) {
					v_vm[4][4].push(v_buildFloat(v_g, parseFloat(v_stringArg)));
				} else {
					if ((v_type == 5)) {
						v_vm[4][4].push(v_buildCommonString(v_g, v_stringArg));
					} else {
						if ((v_type == 9)) {
							var v_index = v_vm[4][4].length;
							v_vm[4][4].push(v_buildCommonString(v_g, v_stringArg));
							v_vm[4][20][v_stringArg] = v_index;
						} else {
							if ((v_type == 10)) {
								var v_cv = [false, v_row[1]];
								v_vm[4][4].push([10, v_cv]);
							}
						}
					}
				}
			}
		}
	}
	return 0;
};

var v_addNameImpl = function(v_vm, v_nameValue) {
	var v_index = v_vm[4][1].length;
	v_vm[4][2][v_nameValue] = v_index;
	v_vm[4][1].push(v_nameValue);
	if ("length" == v_nameValue) {
		v_vm[4][14] = v_index;
	}
	return 0;
};

var v_addToList = function(v_list, v_item) {
	v_list[2].push(v_item);
	v_list[1] += 1;
};

var v_applyDebugSymbolData = function(v_vm, v_opArgs, v_stringData, v_recentlyDefinedFunction) {
	return 0;
};

var v_buildBoolean = function(v_g, v_value) {
	if (v_value) {
		return v_g[1];
	}
	return v_g[2];
};

var v_buildCommonString = function(v_g, v_s) {
	var v_value = null;
	v_value = v_g[11][v_s];
	if (v_value === undefined) v_value = null;
	if ((v_value == null)) {
		v_value = v_buildString(v_g, v_s);
		v_g[11][v_s] = v_value;
	}
	return v_value;
};

var v_buildFloat = function(v_g, v_value) {
	if ((v_value == 0.0)) {
		return v_g[6];
	}
	if ((v_value == 1.0)) {
		return v_g[7];
	}
	return [4, v_value];
};

var v_buildInteger = function(v_g, v_num) {
	if ((v_num < 0)) {
		if ((v_num > -257)) {
			return v_g[10][-v_num];
		}
	} else {
		if ((v_num < 2049)) {
			return v_g[9][v_num];
		}
	}
	return [3, v_num];
};

var v_buildList = function(v_valueList) {
	return v_buildListWithType(null, v_valueList);
};

var v_buildListWithType = function(v_type, v_valueList) {
	return [6, [v_type, v_valueList.length, v_valueList]];
};

var v_buildNull = function(v_globals) {
	return v_globals[0];
};

var v_buildRelayObj = function(v_type, v_iarg1, v_iarg2, v_iarg3, v_farg1, v_sarg1) {
	return [v_type, v_iarg1, v_iarg2, v_iarg3, v_farg1, v_sarg1];
};

var v_buildString = function(v_g, v_s) {
	if ((v_s.length == 0)) {
		return v_g[8];
	}
	return [5, v_s];
};

var v_buildStringDictionary = function(v_globals, v_stringKeys, v_values) {
	var v_size = v_stringKeys.length;
	var v_d = [v_size, 5, 0, null, {}, {}, [], []];
	var v_k = null;
	var v_i = 0;
	while ((v_i < v_size)) {
		v_k = v_stringKeys[v_i];
		if ((v_d[5][v_k] !== undefined)) {
			v_d[7][v_d[5][v_k]] = v_values[v_i];
		} else {
			v_d[5][v_k] = v_d[7].length;
			v_d[7].push(v_values[v_i]);
			v_d[6].push(v_buildString(v_globals, v_k));
		}
		v_i += 1;
	}
	v_d[0] = v_d[7].length;
	return [7, v_d];
};

var v_canAssignGenericToGeneric = function(v_vm, v_gen1, v_gen1Index, v_gen2, v_gen2Index, v_newIndexOut) {
	if ((v_gen2 == null)) {
		return true;
	}
	if ((v_gen1 == null)) {
		return false;
	}
	var v_t1 = v_gen1[v_gen1Index];
	var v_t2 = v_gen2[v_gen2Index];
	switch (v_t1) {
		case 0:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return (v_t2 == v_t1);
		case 1:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return (v_t2 == v_t1);
		case 2:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return (v_t2 == v_t1);
		case 4:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return (v_t2 == v_t1);
		case 5:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return (v_t2 == v_t1);
		case 10:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return (v_t2 == v_t1);
		case 3:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			return ((v_t2 == 3) || (v_t2 == 4));
		case 8:
			v_newIndexOut[0] = (v_gen1Index + 1);
			v_newIndexOut[1] = (v_gen2Index + 2);
			if ((v_t2 != 8)) {
				return false;
			}
			var v_c1 = v_gen1[(v_gen1Index + 1)];
			var v_c2 = v_gen2[(v_gen2Index + 1)];
			if ((v_c1 == v_c2)) {
				return true;
			}
			return v_isClassASubclassOf(v_vm, v_c1, v_c2);
		case 6:
			if ((v_t2 != 6)) {
				return false;
			}
			return v_canAssignGenericToGeneric(v_vm, v_gen1, (v_gen1Index + 1), v_gen2, (v_gen2Index + 1), v_newIndexOut);
		case 7:
			if ((v_t2 != 7)) {
				return false;
			}
			if (!v_canAssignGenericToGeneric(v_vm, v_gen1, (v_gen1Index + 1), v_gen2, (v_gen2Index + 1), v_newIndexOut)) {
				return false;
			}
			return v_canAssignGenericToGeneric(v_vm, v_gen1, v_newIndexOut[0], v_gen2, v_newIndexOut[1], v_newIndexOut);
		case 9:
			if ((v_t2 != 9)) {
				return false;
			}
			return false;
		default:
			return false;
	}
};

var v_canAssignTypeToGeneric = function(v_vm, v_value, v_generics, v_genericIndex) {
	switch (v_value[0]) {
		case 1:
			switch (v_generics[v_genericIndex]) {
				case 5:
					return v_value;
				case 8:
					return v_value;
				case 10:
					return v_value;
				case 9:
					return v_value;
				case 6:
					return v_value;
				case 7:
					return v_value;
			}
			return null;
		case 2:
			if ((v_generics[v_genericIndex] == v_value[0])) {
				return v_value;
			}
			return null;
		case 5:
			if ((v_generics[v_genericIndex] == v_value[0])) {
				return v_value;
			}
			return null;
		case 10:
			if ((v_generics[v_genericIndex] == v_value[0])) {
				return v_value;
			}
			return null;
		case 3:
			if ((v_generics[v_genericIndex] == 3)) {
				return v_value;
			}
			if ((v_generics[v_genericIndex] == 4)) {
				return v_buildFloat(v_vm[12], (0.0 + v_value[1]));
			}
			return null;
		case 4:
			if ((v_generics[v_genericIndex] == 4)) {
				return v_value;
			}
			return null;
		case 6:
			var v_list = v_value[1];
			var v_listType = v_list[0];
			v_genericIndex += 1;
			if ((v_listType == null)) {
				if (((v_generics[v_genericIndex] == 1) || (v_generics[v_genericIndex] == 0))) {
					return v_value;
				}
				return null;
			}
			var v_i = 0;
			while ((v_i < v_listType.length)) {
				if ((v_listType[v_i] != v_generics[(v_genericIndex + v_i)])) {
					return null;
				}
				v_i += 1;
			}
			return v_value;
		case 7:
			var v_dict = v_value[1];
			var v_j = v_genericIndex;
			switch (v_dict[1]) {
				case 3:
					if ((v_generics[1] == v_dict[1])) {
						v_j += 2;
					} else {
						return null;
					}
					break;
				case 5:
					if ((v_generics[1] == v_dict[1])) {
						v_j += 2;
					} else {
						return null;
					}
					break;
				case 8:
					if ((v_generics[1] == 8)) {
						v_j += 3;
					} else {
						return null;
					}
					break;
			}
			var v_valueType = v_dict[3];
			if ((v_valueType == null)) {
				if (((v_generics[v_j] == 0) || (v_generics[v_j] == 1))) {
					return v_value;
				}
				return null;
			}
			var v_k = 0;
			while ((v_k < v_valueType.length)) {
				if ((v_valueType[v_k] != v_generics[(v_j + v_k)])) {
					return null;
				}
				v_k += 1;
			}
			return v_value;
		case 8:
			if ((v_generics[v_genericIndex] == 8)) {
				var v_targetClassId = v_generics[(v_genericIndex + 1)];
				var v_givenClassId = (v_value[1])[0];
				if ((v_targetClassId == v_givenClassId)) {
					return v_value;
				}
				if (v_isClassASubclassOf(v_vm, v_givenClassId, v_targetClassId)) {
					return v_value;
				}
			}
			return null;
	}
	return null;
};

var v_canonicalizeAngle = function(v_a) {
	var v_twopi = 6.28318530717958;
	v_a = (v_a % v_twopi);
	if ((v_a < 0)) {
		v_a += v_twopi;
	}
	return v_a;
};

var v_canonicalizeListSliceArgs = function(v_outParams, v_beginValue, v_endValue, v_beginIndex, v_endIndex, v_stepAmount, v_length, v_isForward) {
	if ((v_beginValue == null)) {
		if (v_isForward) {
			v_beginIndex = 0;
		} else {
			v_beginIndex = (v_length - 1);
		}
	}
	if ((v_endValue == null)) {
		if (v_isForward) {
			v_endIndex = v_length;
		} else {
			v_endIndex = (-1 - v_length);
		}
	}
	if ((v_beginIndex < 0)) {
		v_beginIndex += v_length;
	}
	if ((v_endIndex < 0)) {
		v_endIndex += v_length;
	}
	if (((v_beginIndex == 0) && (v_endIndex == v_length) && (v_stepAmount == 1))) {
		return 2;
	}
	if (v_isForward) {
		if ((v_beginIndex >= v_length)) {
			return 0;
		}
		if ((v_beginIndex < 0)) {
			return 3;
		}
		if ((v_endIndex < v_beginIndex)) {
			return 4;
		}
		if ((v_beginIndex == v_endIndex)) {
			return 0;
		}
		if ((v_endIndex > v_length)) {
			v_endIndex = v_length;
		}
	} else {
		if ((v_beginIndex < 0)) {
			return 0;
		}
		if ((v_beginIndex >= v_length)) {
			return 3;
		}
		if ((v_endIndex > v_beginIndex)) {
			return 4;
		}
		if ((v_beginIndex == v_endIndex)) {
			return 0;
		}
		if ((v_endIndex < -1)) {
			v_endIndex = -1;
		}
	}
	v_outParams[0] = v_beginIndex;
	v_outParams[1] = v_endIndex;
	return 1;
};

var v_classIdToString = function(v_vm, v_classId) {
	return v_vm[4][9][v_classId][16];
};

var v_clearList = function(v_a) {
	PST$clearList(v_a[2]);
	v_a[1] = 0;
	return 0;
};

var v_cloneDictionary = function(v_original, v_clone) {
	var v_type = v_original[1];
	var v_i = 0;
	var v_size = v_original[0];
	var v_kInt = 0;
	var v_kString = null;
	if ((v_clone == null)) {
		v_clone = [0, v_type, v_original[2], v_original[3], {}, {}, [], []];
		if ((v_type == 5)) {
			while ((v_i < v_size)) {
				v_clone[5][v_original[6][v_i][1]] = v_i;
				v_i += 1;
			}
		} else {
			while ((v_i < v_size)) {
				if ((v_type == 8)) {
					v_kInt = (v_original[6][v_i][1])[1];
				} else {
					v_kInt = v_original[6][v_i][1];
				}
				v_clone[4][v_kInt] = v_i;
				v_i += 1;
			}
		}
		v_i = 0;
		while ((v_i < v_size)) {
			v_clone[6].push(v_original[6][v_i]);
			v_clone[7].push(v_original[7][v_i]);
			v_i += 1;
		}
	} else {
		v_i = 0;
		while ((v_i < v_size)) {
			if ((v_type == 5)) {
				v_kString = v_original[6][v_i][1];
				if ((v_clone[5][v_kString] !== undefined)) {
					v_clone[7][v_clone[5][v_kString]] = v_original[7][v_i];
				} else {
					v_clone[5][v_kString] = v_clone[7].length;
					v_clone[7].push(v_original[7][v_i]);
					v_clone[6].push(v_original[6][v_i]);
				}
			} else {
				if ((v_type == 3)) {
					v_kInt = v_original[6][v_i][1];
				} else {
					v_kInt = (v_original[6][v_i][1])[1];
				}
				if ((v_clone[4][v_kInt] !== undefined)) {
					v_clone[7][v_clone[4][v_kInt]] = v_original[7][v_i];
				} else {
					v_clone[4][v_kInt] = v_clone[7].length;
					v_clone[7].push(v_original[7][v_i]);
					v_clone[6].push(v_original[6][v_i]);
				}
			}
			v_i += 1;
		}
	}
	v_clone[0] = (Object.keys(v_clone[4]).length + Object.keys(v_clone[5]).length);
	return v_clone;
};

var v_createInstanceType = function(v_classId) {
	var v_o = PST$createNewArray(2);
	v_o[0] = 8;
	v_o[1] = v_classId;
	return v_o;
};

var v_createVm = function(v_rawByteCode, v_resourceManifest) {
	var v_globals = v_initializeConstantValues();
	var v_resources = v_resourceManagerInitialize(v_globals, v_resourceManifest);
	var v_byteCode = v_initializeByteCode(v_rawByteCode);
	var v_localsStack = PST$createNewArray(10);
	var v_localsStackSet = PST$createNewArray(10);
	var v_i = 0;
	v_i = (v_localsStack.length - 1);
	while ((v_i >= 0)) {
		v_localsStack[v_i] = null;
		v_localsStackSet[v_i] = 0;
		v_i -= 1;
	}
	var v_stack = [0, 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null];
	var v_executionContext = [0, v_stack, 0, 100, PST$createNewArray(100), v_localsStack, v_localsStackSet, 1, 0, false, null, false, 0, null];
	var v_executionContexts = {};
	v_executionContexts[0] = v_executionContext;
	var v_vm = [v_executionContexts, v_executionContext[0], v_byteCode, [PST$createNewArray(v_byteCode[0].length), null, [], null, null, {}, {}], [null, [], {}, null, [], null, [], null, [], PST$createNewArray(100), PST$createNewArray(100), {}, null, {}, -1, PST$createNewArray(10), 0, null, null, [0, 0, 0], {}, {}, null], 0, false, [], null, v_resources, [], [PST$createNewArray(0), false, null, null], v_globals, v_globals[0], v_globals[1], v_globals[2]];
	return v_vm;
};

var v_debuggerClearBreakpoint = function(v_vm, v_id) {
	return 0;
};

var v_debuggerFindPcForLine = function(v_vm, v_path, v_line) {
	return -1;
};

var v_debuggerSetBreakpoint = function(v_vm, v_path, v_line) {
	return -1;
};

var v_debugSetStepOverBreakpoint = function(v_vm) {
	return false;
};

var v_defOriginalCodeImpl = function(v_vm, v_row, v_fileContents) {
	var v_fileId = v_row[0];
	var v_codeLookup = v_vm[3][2];
	while ((v_codeLookup.length <= v_fileId)) {
		v_codeLookup.push(null);
	}
	v_codeLookup[v_fileId] = v_fileContents;
	return 0;
};

var v_dictKeyInfoToString = function(v_vm, v_dict) {
	if ((v_dict[1] == 5)) {
		return "string";
	}
	if ((v_dict[1] == 3)) {
		return "int";
	}
	if ((v_dict[2] == 0)) {
		return "instance";
	}
	return v_classIdToString(v_vm, v_dict[2]);
};

var v_doEqualityComparisonAndReturnCode = function(v_a, v_b) {
	var v_leftType = v_a[0];
	var v_rightType = v_b[0];
	if ((v_leftType == v_rightType)) {
		var v_output = 0;
		switch (v_leftType) {
			case 1:
				v_output = 1;
				break;
			case 3:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 4:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 2:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 5:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 6:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 7:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 8:
				if ((v_a[1] == v_b[1])) {
					v_output = 1;
				}
				break;
			case 9:
				var v_f1 = v_a[1];
				var v_f2 = v_b[1];
				if ((v_f1[3] == v_f2[3])) {
					if (((v_f1[0] == 2) || (v_f1[0] == 4))) {
						if ((v_doEqualityComparisonAndReturnCode(v_f1[1], v_f2[1]) == 1)) {
							v_output = 1;
						}
					} else {
						v_output = 1;
					}
				}
				break;
			case 10:
				var v_c1 = v_a[1];
				var v_c2 = v_b[1];
				if ((v_c1[1] == v_c2[1])) {
					v_output = 1;
				}
				break;
			default:
				v_output = 2;
				break;
		}
		return v_output;
	}
	if ((v_rightType == 1)) {
		return 0;
	}
	if (((v_leftType == 3) && (v_rightType == 4))) {
		if ((v_a[1] == v_b[1])) {
			return 1;
		}
	} else {
		if (((v_leftType == 4) && (v_rightType == 3))) {
			if ((v_a[1] == v_b[1])) {
				return 1;
			}
		}
	}
	return 0;
};

var v_encodeBreakpointData = function(v_vm, v_breakpoint, v_pc) {
	return null;
};

var v_errorResult = function(v_error) {
	return [3, v_error, 0.0, 0, false, ""];
};

var v_EX_AssertionFailed = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 2, v_exMsg);
};

var v_EX_DivisionByZero = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 3, v_exMsg);
};

var v_EX_Fatal = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 0, v_exMsg);
};

var v_EX_IndexOutOfRange = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 4, v_exMsg);
};

var v_EX_InvalidArgument = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 5, v_exMsg);
};

var v_EX_InvalidAssignment = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 6, v_exMsg);
};

var v_EX_InvalidInvocation = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 7, v_exMsg);
};

var v_EX_InvalidKey = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 8, v_exMsg);
};

var v_EX_KeyNotFound = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 9, v_exMsg);
};

var v_EX_NullReference = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 10, v_exMsg);
};

var v_EX_UnassignedVariable = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 11, v_exMsg);
};

var v_EX_UnknownField = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 12, v_exMsg);
};

var v_EX_UnsupportedOperation = function(v_ec, v_exMsg) {
	return v_generateException2(v_ec, 13, v_exMsg);
};

var v_finalizeInitializationImpl = function(v_vm, v_projectId, v_localeCount) {
	v_vm[3][1] = PST$multiplyList(v_vm[3][2], 1);
	v_vm[3][2] = null;
	v_vm[4][19][2] = v_localeCount;
	v_vm[4][0] = PST$multiplyList(v_vm[4][1], 1);
	v_vm[4][3] = PST$multiplyList(v_vm[4][4], 1);
	v_vm[4][12] = v_primitiveMethodsInitializeLookup(v_vm[4][2]);
	v_vm[8] = PST$createNewArray(v_vm[4][0].length);
	v_vm[4][17] = v_projectId;
	v_vm[4][1] = null;
	v_vm[4][4] = null;
	v_vm[6] = true;
	return 0;
};

var v_fixFuzzyFloatPrecision = function(v_x) {
	if (((v_x % 1) != 0)) {
		var v_u = (v_x % 1);
		if ((v_u < 0)) {
			v_u += 1.0;
		}
		var v_roundDown = false;
		if ((v_u > 0.9999999999)) {
			v_roundDown = true;
			v_x += 0.1;
		} else {
			if ((v_u < 0.00000000002250000000)) {
				v_roundDown = true;
			}
		}
		if (v_roundDown) {
			if ((true || (v_x > 0))) {
				v_x = (Math.floor(v_x) + 0.0);
			} else {
				v_x = (Math.floor(v_x) - 1.0);
			}
		}
	}
	return v_x;
};

var v_generateEsfData = function(v_byteCodeLength, v_esfArgs) {
	var v_output = PST$createNewArray(v_byteCodeLength);
	var v_esfTokenStack = [];
	var v_esfTokenStackTop = null;
	var v_esfArgIterator = 0;
	var v_esfArgLength = v_esfArgs.length;
	var v_j = 0;
	var v_pc = 0;
	while ((v_pc < v_byteCodeLength)) {
		if (((v_esfArgIterator < v_esfArgLength) && (v_pc == v_esfArgs[v_esfArgIterator]))) {
			v_esfTokenStackTop = PST$createNewArray(2);
			v_j = 1;
			while ((v_j < 3)) {
				v_esfTokenStackTop[(v_j - 1)] = v_esfArgs[(v_esfArgIterator + v_j)];
				v_j += 1;
			}
			v_esfTokenStack.push(v_esfTokenStackTop);
			v_esfArgIterator += 3;
		}
		while (((v_esfTokenStackTop != null) && (v_esfTokenStackTop[1] <= v_pc))) {
			v_esfTokenStack.pop();
			if ((v_esfTokenStack.length == 0)) {
				v_esfTokenStackTop = null;
			} else {
				v_esfTokenStackTop = v_esfTokenStack[(v_esfTokenStack.length - 1)];
			}
		}
		v_output[v_pc] = v_esfTokenStackTop;
		v_pc += 1;
	}
	return v_output;
};

var v_generateException = function(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, v_type, v_message) {
	v_ec[2] = v_valueStackSize;
	v_stack[0] = v_pc;
	var v_mn = v_vm[4][19];
	var v_generateExceptionFunctionId = v_mn[1];
	var v_functionInfo = v_vm[4][10][v_generateExceptionFunctionId];
	v_pc = v_functionInfo[2];
	if ((v_ec[5].length <= (v_functionInfo[7] + v_stack[3]))) {
		v_increaseLocalsStackCapacity(v_ec, v_functionInfo[7]);
	}
	var v_localsIndex = v_stack[3];
	var v_localsStackSetToken = (v_ec[7] + 1);
	v_ec[7] = v_localsStackSetToken;
	v_ec[5][v_localsIndex] = v_buildInteger(v_vm[12], v_type);
	v_ec[5][(v_localsIndex + 1)] = v_buildString(v_vm[12], v_message);
	v_ec[6][v_localsIndex] = v_localsStackSetToken;
	v_ec[6][(v_localsIndex + 1)] = v_localsStackSetToken;
	v_ec[1] = [(v_pc + 1), v_localsStackSetToken, v_stack[3], (v_stack[3] + v_functionInfo[7]), v_stack, false, null, v_valueStackSize, 0, (v_stack[9] + 1), 0, null, null, null];
	return [5, null, 0.0, 0, false, ""];
};

var v_generateException2 = function(v_ec, v_exceptionType, v_exMsg) {
	v_ec[13] = [1, v_exceptionType, v_exMsg, 0.0, null];
	return true;
};

var v_generatePrimitiveMethodReference = function(v_lookup, v_globalNameId, v_context) {
	var v_functionId = v_resolvePrimitiveMethodName2(v_lookup, v_context[0], v_globalNameId);
	if ((v_functionId < 0)) {
		return null;
	}
	return [9, [4, v_context, 0, v_functionId, null]];
};

var v_generateTokenListFromPcs = function(v_vm, v_pcs) {
	var v_output = [];
	var v_tokensByPc = v_vm[3][0];
	var v_token = null;
	var v_i = 0;
	while ((v_i < v_pcs.length)) {
		var v_localTokens = v_tokensByPc[v_pcs[v_i]];
		if ((v_localTokens == null)) {
			if ((v_output.length > 0)) {
				v_output.push(null);
			}
		} else {
			v_token = v_localTokens[0];
			v_output.push(v_token);
		}
		v_i += 1;
	}
	return v_output;
};

var v_getBinaryOpFromId = function(v_id) {
	switch (v_id) {
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

var v_getClassTable = function(v_vm, v_classId) {
	var v_oldTable = v_vm[4][9];
	var v_oldLength = v_oldTable.length;
	if ((v_classId < v_oldLength)) {
		return v_oldTable;
	}
	var v_newLength = (v_oldLength * 2);
	if ((v_classId >= v_newLength)) {
		v_newLength = (v_classId + 100);
	}
	var v_newTable = PST$createNewArray(v_newLength);
	var v_i = (v_oldLength - 1);
	while ((v_i >= 0)) {
		v_newTable[v_i] = v_oldTable[v_i];
		v_i -= 1;
	}
	v_vm[4][9] = v_newTable;
	return v_newTable;
};

var v_getExecutionContext = function(v_vm, v_id) {
	if ((v_id == -1)) {
		v_id = v_vm[1];
	}
	if ((v_vm[0][v_id] !== undefined)) {
		return v_vm[0][v_id];
	}
	return null;
};

var v_getFloat = function(v_num) {
	if ((v_num[0] == 4)) {
		return v_num[1];
	}
	return (v_num[1] + 0.0);
};

var v_getFunctionTable = function(v_vm, v_functionId) {
	var v_oldTable = v_vm[4][10];
	var v_oldLength = v_oldTable.length;
	if ((v_functionId < v_oldLength)) {
		return v_oldTable;
	}
	var v_newLength = (v_oldLength * 2);
	if ((v_functionId >= v_newLength)) {
		v_newLength = (v_functionId + 100);
	}
	var v_newTable = PST$createNewArray(v_newLength);
	var v_i = 0;
	while ((v_i < v_oldLength)) {
		v_newTable[v_i] = v_oldTable[v_i];
		v_i += 1;
	}
	v_vm[4][10] = v_newTable;
	return v_newTable;
};

var v_getItemFromList = function(v_list, v_i) {
	return v_list[2][v_i];
};

var v_getNativeDataItem = function(v_objValue, v_index) {
	var v_obj = v_objValue[1];
	return v_obj[3][v_index];
};

var v_getTypeFromId = function(v_id) {
	switch (v_id) {
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

var v_getVmReinvokeDelay = function(v_result) {
	return v_result[2];
};

var v_getVmResultAssemblyInfo = function(v_result) {
	return v_result[5];
};

var v_getVmResultExecId = function(v_result) {
	return v_result[3];
};

var v_getVmResultStatus = function(v_result) {
	return v_result[0];
};

var v_increaseListCapacity = function(v_list) {
};

var v_increaseLocalsStackCapacity = function(v_ec, v_newScopeSize) {
	var v_oldLocals = v_ec[5];
	var v_oldSetIndicator = v_ec[6];
	var v_oldCapacity = v_oldLocals.length;
	var v_newCapacity = ((v_oldCapacity * 2) + v_newScopeSize);
	var v_newLocals = PST$createNewArray(v_newCapacity);
	var v_newSetIndicator = PST$createNewArray(v_newCapacity);
	var v_i = 0;
	while ((v_i < v_oldCapacity)) {
		v_newLocals[v_i] = v_oldLocals[v_i];
		v_newSetIndicator[v_i] = v_oldSetIndicator[v_i];
		v_i += 1;
	}
	v_ec[5] = v_newLocals;
	v_ec[6] = v_newSetIndicator;
	return 0;
};

var v_initFileNameSymbolData = function(v_vm) {
	var v_symbolData = v_vm[3];
	if ((v_symbolData == null)) {
		return 0;
	}
	if ((v_symbolData[3] == null)) {
		var v_i = 0;
		var v_filenames = PST$createNewArray(v_symbolData[1].length);
		var v_fileIdByPath = {};
		v_i = 0;
		while ((v_i < v_filenames.length)) {
			var v_sourceCode = v_symbolData[1][v_i];
			if ((v_sourceCode != null)) {
				var v_colon = v_sourceCode.indexOf("\n");
				if ((v_colon != -1)) {
					var v_filename = v_sourceCode.substring(0, 0 + v_colon);
					v_filenames[v_i] = v_filename;
					v_fileIdByPath[v_filename] = v_i;
				}
			}
			v_i += 1;
		}
		v_symbolData[3] = v_filenames;
		v_symbolData[4] = v_fileIdByPath;
	}
	return 0;
};

var v_initializeByteCode = function(v_raw) {
	var v_index = PST$createNewArray(1);
	v_index[0] = 0;
	var v_length = v_raw.length;
	var v_header = v_read_till(v_index, v_raw, v_length, "@");
	if ((v_header != "CRAYON")) {
	}
	var v_alphaNums = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	var v_opCount = v_read_integer(v_index, v_raw, v_length, v_alphaNums);
	var v_ops = PST$createNewArray(v_opCount);
	var v_iargs = PST$createNewArray(v_opCount);
	var v_sargs = PST$createNewArray(v_opCount);
	var v_c = " ";
	var v_argc = 0;
	var v_j = 0;
	var v_stringarg = null;
	var v_stringPresent = false;
	var v_iarg = 0;
	var v_iarglist = null;
	var v_i = 0;
	v_i = 0;
	while ((v_i < v_opCount)) {
		v_c = v_raw.charAt(v_index[0]);
		v_index[0] = (v_index[0] + 1);
		v_argc = 0;
		v_stringPresent = true;
		if ((v_c == "!")) {
			v_argc = 1;
		} else {
			if ((v_c == "&")) {
				v_argc = 2;
			} else {
				if ((v_c == "*")) {
					v_argc = 3;
				} else {
					if ((v_c != "~")) {
						v_stringPresent = false;
						v_index[0] = (v_index[0] - 1);
					}
					v_argc = v_read_integer(v_index, v_raw, v_length, v_alphaNums);
				}
			}
		}
		v_iarglist = PST$createNewArray((v_argc - 1));
		v_j = 0;
		while ((v_j < v_argc)) {
			v_iarg = v_read_integer(v_index, v_raw, v_length, v_alphaNums);
			if ((v_j == 0)) {
				v_ops[v_i] = v_iarg;
			} else {
				v_iarglist[(v_j - 1)] = v_iarg;
			}
			v_j += 1;
		}
		v_iargs[v_i] = v_iarglist;
		if (v_stringPresent) {
			v_stringarg = v_read_string(v_index, v_raw, v_length, v_alphaNums);
		} else {
			v_stringarg = null;
		}
		v_sargs[v_i] = v_stringarg;
		v_i += 1;
	}
	var v_hasBreakpoint = PST$createNewArray(v_opCount);
	var v_breakpointInfo = PST$createNewArray(v_opCount);
	v_i = 0;
	while ((v_i < v_opCount)) {
		v_hasBreakpoint[v_i] = false;
		v_breakpointInfo[v_i] = null;
		v_i += 1;
	}
	return [v_ops, v_iargs, v_sargs, PST$createNewArray(v_opCount), PST$createNewArray(v_opCount), [v_hasBreakpoint, v_breakpointInfo, {}, 1, 0]];
};

var v_initializeClass = function(v_pc, v_vm, v_args, v_className) {
	var v_i = 0;
	var v_memberId = 0;
	var v_globalId = 0;
	var v_functionId = 0;
	var v_t = 0;
	var v_classId = v_args[0];
	var v_baseClassId = v_args[1];
	var v_globalNameId = v_args[2];
	var v_constructorFunctionId = v_args[3];
	var v_staticConstructorFunctionId = v_args[4];
	var v_staticInitializationState = 0;
	if ((v_staticConstructorFunctionId == -1)) {
		v_staticInitializationState = 2;
	}
	var v_staticFieldCount = v_args[5];
	var v_assemblyId = v_args[6];
	var v_staticFields = PST$createNewArray(v_staticFieldCount);
	v_i = 0;
	while ((v_i < v_staticFieldCount)) {
		v_staticFields[v_i] = v_vm[12][0];
		v_i += 1;
	}
	var v_classInfo = [v_classId, v_globalNameId, v_baseClassId, v_assemblyId, v_staticInitializationState, v_staticFields, v_staticConstructorFunctionId, v_constructorFunctionId, 0, null, null, null, null, null, v_vm[4][21][v_classId], null, v_className];
	var v_classTable = v_getClassTable(v_vm, v_classId);
	v_classTable[v_classId] = v_classInfo;
	var v_classChain = [];
	v_classChain.push(v_classInfo);
	var v_classIdWalker = v_baseClassId;
	while ((v_classIdWalker != -1)) {
		var v_walkerClass = v_classTable[v_classIdWalker];
		v_classChain.push(v_walkerClass);
		v_classIdWalker = v_walkerClass[2];
	}
	var v_baseClass = null;
	if ((v_baseClassId != -1)) {
		v_baseClass = v_classChain[1];
	}
	var v_functionIds = [];
	var v_fieldInitializationCommand = [];
	var v_fieldInitializationLiteral = [];
	var v_fieldAccessModifier = [];
	var v_globalNameIdToMemberId = {};
	if ((v_baseClass != null)) {
		v_i = 0;
		while ((v_i < v_baseClass[8])) {
			v_functionIds.push(v_baseClass[9][v_i]);
			v_fieldInitializationCommand.push(v_baseClass[10][v_i]);
			v_fieldInitializationLiteral.push(v_baseClass[11][v_i]);
			v_fieldAccessModifier.push(v_baseClass[12][v_i]);
			v_i += 1;
		}
		var v_keys = PST$dictionaryKeys(v_baseClass[13]);
		v_i = 0;
		while ((v_i < v_keys.length)) {
			v_t = v_keys[v_i];
			v_globalNameIdToMemberId[v_t] = v_baseClass[13][v_t];
			v_i += 1;
		}
		v_keys = PST$dictionaryKeys(v_baseClass[14]);
		v_i = 0;
		while ((v_i < v_keys.length)) {
			v_t = v_keys[v_i];
			v_classInfo[14][v_t] = v_baseClass[14][v_t];
			v_i += 1;
		}
	}
	var v_accessModifier = 0;
	v_i = 7;
	while ((v_i < v_args.length)) {
		v_memberId = v_args[(v_i + 1)];
		v_globalId = v_args[(v_i + 2)];
		v_accessModifier = v_args[(v_i + 5)];
		while ((v_memberId >= v_functionIds.length)) {
			v_functionIds.push(-1);
			v_fieldInitializationCommand.push(-1);
			v_fieldInitializationLiteral.push(null);
			v_fieldAccessModifier.push(0);
		}
		v_globalNameIdToMemberId[v_globalId] = v_memberId;
		v_fieldAccessModifier[v_memberId] = v_accessModifier;
		if ((v_args[v_i] == 0)) {
			v_fieldInitializationCommand[v_memberId] = v_args[(v_i + 3)];
			v_t = v_args[(v_i + 4)];
			if ((v_t == -1)) {
				v_fieldInitializationLiteral[v_memberId] = v_vm[12][0];
			} else {
				v_fieldInitializationLiteral[v_memberId] = v_vm[4][3][v_t];
			}
		} else {
			v_functionId = v_args[(v_i + 3)];
			v_functionIds[v_memberId] = v_functionId;
		}
		v_i += 6;
	}
	v_classInfo[9] = PST$multiplyList(v_functionIds, 1);
	v_classInfo[10] = PST$multiplyList(v_fieldInitializationCommand, 1);
	v_classInfo[11] = PST$multiplyList(v_fieldInitializationLiteral, 1);
	v_classInfo[12] = PST$multiplyList(v_fieldAccessModifier, 1);
	v_classInfo[8] = v_functionIds.length;
	v_classInfo[13] = v_globalNameIdToMemberId;
	v_classInfo[15] = PST$createNewArray(v_classInfo[8]);
	if ((v_baseClass != null)) {
		v_i = 0;
		while ((v_i < v_baseClass[15].length)) {
			v_classInfo[15][v_i] = v_baseClass[15][v_i];
			v_i += 1;
		}
	}
	if ("Core.Exception" == v_className) {
		var v_mn = v_vm[4][19];
		v_mn[0] = v_classId;
	}
	return 0;
};

var v_initializeClassFieldTypeInfo = function(v_vm, v_opCodeRow) {
	var v_classInfo = v_vm[4][9][v_opCodeRow[0]];
	var v_memberId = v_opCodeRow[1];
	var v_len = v_opCodeRow.length;
	var v_typeInfo = PST$createNewArray((v_len - 2));
	var v_i = 2;
	while ((v_i < v_len)) {
		v_typeInfo[(v_i - 2)] = v_opCodeRow[v_i];
		v_i += 1;
	}
	v_classInfo[15][v_memberId] = v_typeInfo;
	return 0;
};

var v_initializeConstantValues = function() {
	var v_pos = PST$createNewArray(2049);
	var v_neg = PST$createNewArray(257);
	var v_i = 0;
	while ((v_i < 2049)) {
		v_pos[v_i] = [3, v_i];
		v_i += 1;
	}
	v_i = 1;
	while ((v_i < 257)) {
		v_neg[v_i] = [3, -v_i];
		v_i += 1;
	}
	v_neg[0] = v_pos[0];
	var v_globals = [[1, null], [2, true], [2, false], v_pos[0], v_pos[1], v_neg[1], [4, 0.0], [4, 1.0], [5, ""], v_pos, v_neg, {}, PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(1), PST$createNewArray(2)];
	v_globals[11][""] = v_globals[8];
	v_globals[12][0] = 2;
	v_globals[13][0] = 3;
	v_globals[15][0] = 4;
	v_globals[14][0] = 5;
	v_globals[16][0] = 10;
	v_globals[17][0] = 8;
	v_globals[17][1] = 0;
	return v_globals;
};

var v_initializeFunction = function(v_vm, v_args, v_currentPc, v_stringArg) {
	var v_functionId = v_args[0];
	var v_nameId = v_args[1];
	var v_minArgCount = v_args[2];
	var v_maxArgCount = v_args[3];
	var v_functionType = v_args[4];
	var v_classId = v_args[5];
	var v_localsCount = v_args[6];
	var v_numPcOffsetsForOptionalArgs = v_args[8];
	var v_pcOffsetsForOptionalArgs = PST$createNewArray((v_numPcOffsetsForOptionalArgs + 1));
	var v_i = 0;
	while ((v_i < v_numPcOffsetsForOptionalArgs)) {
		v_pcOffsetsForOptionalArgs[(v_i + 1)] = v_args[(9 + v_i)];
		v_i += 1;
	}
	var v_functionTable = v_getFunctionTable(v_vm, v_functionId);
	v_functionTable[v_functionId] = [v_functionId, v_nameId, v_currentPc, v_minArgCount, v_maxArgCount, v_functionType, v_classId, v_localsCount, v_pcOffsetsForOptionalArgs, v_stringArg, null];
	v_vm[4][22] = v_functionTable[v_functionId];
	if ((v_nameId >= 0)) {
		var v_name = v_vm[4][0][v_nameId];
		if ("_LIB_CORE_list_filter" == v_name) {
			v_vm[4][15][0] = v_functionId;
		} else {
			if ("_LIB_CORE_list_map" == v_name) {
				v_vm[4][15][1] = v_functionId;
			} else {
				if ("_LIB_CORE_list_sort_by_key" == v_name) {
					v_vm[4][15][2] = v_functionId;
				} else {
					if ("_LIB_CORE_invoke" == v_name) {
						v_vm[4][15][3] = v_functionId;
					} else {
						if ("_LIB_CORE_generateException" == v_name) {
							var v_mn = v_vm[4][19];
							v_mn[1] = v_functionId;
						}
					}
				}
			}
		}
	}
	return 0;
};

var v_initializeIntSwitchStatement = function(v_vm, v_pc, v_args) {
	var v_output = {};
	var v_i = 1;
	while ((v_i < v_args.length)) {
		v_output[v_args[v_i]] = v_args[(v_i + 1)];
		v_i += 2;
	}
	v_vm[2][3][v_pc] = v_output;
	return v_output;
};

var v_initializeStringSwitchStatement = function(v_vm, v_pc, v_args) {
	var v_output = {};
	var v_i = 1;
	while ((v_i < v_args.length)) {
		var v_s = v_vm[4][3][v_args[v_i]][1];
		v_output[v_s] = v_args[(v_i + 1)];
		v_i += 2;
	}
	v_vm[2][4][v_pc] = v_output;
	return v_output;
};

var v_initLocTable = function(v_vm, v_row) {
	var v_classId = v_row[0];
	var v_memberCount = v_row[1];
	var v_nameId = 0;
	var v_totalLocales = v_vm[4][19][2];
	var v_lookup = {};
	var v_i = 2;
	while ((v_i < v_row.length)) {
		var v_localeId = v_row[v_i];
		v_i += 1;
		var v_j = 0;
		while ((v_j < v_memberCount)) {
			v_nameId = v_row[(v_i + v_j)];
			if ((v_nameId != -1)) {
				v_lookup[((v_nameId * v_totalLocales) + v_localeId)] = v_j;
			}
			v_j += 1;
		}
		v_i += v_memberCount;
	}
	v_vm[4][21][v_classId] = v_lookup;
	return 0;
};

var v_interpret = function(v_vm, v_executionContextId) {
	var v_output = v_interpretImpl(v_vm, v_executionContextId);
	while (((v_output[0] == 5) && (v_output[2] == 0))) {
		v_output = v_interpretImpl(v_vm, v_executionContextId);
	}
	return v_output;
};

var v_interpreterFinished = function(v_vm, v_ec) {
	if ((v_ec != null)) {
		var v_id = v_ec[0];
		if ((v_vm[0][v_id] !== undefined)) {
			delete v_vm[0][v_id];
		}
	}
	return [1, null, 0.0, 0, false, ""];
};

var v_interpreterGetExecutionContext = function(v_vm, v_executionContextId) {
	var v_executionContexts = v_vm[0];
	if (!(v_executionContexts[v_executionContextId] !== undefined)) {
		return null;
	}
	return v_executionContexts[v_executionContextId];
};

var v_interpretImpl = function(v_vm, v_executionContextId) {
	var v_metadata = v_vm[4];
	var v_globals = v_vm[12];
	var v_VALUE_NULL = v_globals[0];
	var v_VALUE_TRUE = v_globals[1];
	var v_VALUE_FALSE = v_globals[2];
	var v_VALUE_INT_ONE = v_globals[4];
	var v_VALUE_INT_ZERO = v_globals[3];
	var v_VALUE_FLOAT_ZERO = v_globals[6];
	var v_VALUE_FLOAT_ONE = v_globals[7];
	var v_INTEGER_POSITIVE_CACHE = v_globals[9];
	var v_INTEGER_NEGATIVE_CACHE = v_globals[10];
	var v_executionContexts = v_vm[0];
	var v_ec = v_interpreterGetExecutionContext(v_vm, v_executionContextId);
	if ((v_ec == null)) {
		return v_interpreterFinished(v_vm, null);
	}
	v_ec[8] += 1;
	var v_stack = v_ec[1];
	var v_ops = v_vm[2][0];
	var v_args = v_vm[2][1];
	var v_stringArgs = v_vm[2][2];
	var v_classTable = v_vm[4][9];
	var v_functionTable = v_vm[4][10];
	var v_literalTable = v_vm[4][3];
	var v_identifiers = v_vm[4][0];
	var v_valueStack = v_ec[4];
	var v_valueStackSize = v_ec[2];
	var v_valueStackCapacity = v_valueStack.length;
	var v_hasInterrupt = false;
	var v_type = 0;
	var v_nameId = 0;
	var v_classId = 0;
	var v_functionId = 0;
	var v_localeId = 0;
	var v_classInfo = null;
	var v_len = 0;
	var v_root = null;
	var v_row = null;
	var v_argCount = 0;
	var v_stringList = null;
	var v_returnValueUsed = false;
	var v_output = null;
	var v_functionInfo = null;
	var v_keyType = 0;
	var v_intKey = 0;
	var v_stringKey = null;
	var v_first = false;
	var v_primitiveMethodToCoreLibraryFallback = false;
	var v_bool1 = false;
	var v_bool2 = false;
	var v_staticConstructorNotInvoked = true;
	var v_int1 = 0;
	var v_int2 = 0;
	var v_int3 = 0;
	var v_i = 0;
	var v_j = 0;
	var v_float1 = 0.0;
	var v_float2 = 0.0;
	var v_float3 = 0.0;
	var v_floatList1 = PST$createNewArray(2);
	var v_value = null;
	var v_value2 = null;
	var v_value3 = null;
	var v_string1 = null;
	var v_string2 = null;
	var v_objInstance1 = null;
	var v_objInstance2 = null;
	var v_list1 = null;
	var v_list2 = null;
	var v_valueList1 = null;
	var v_valueList2 = null;
	var v_dictImpl = null;
	var v_dictImpl2 = null;
	var v_stringList1 = null;
	var v_intList1 = null;
	var v_valueArray1 = null;
	var v_intArray1 = null;
	var v_intArray2 = null;
	var v_objArray1 = null;
	var v_functionPointer1 = null;
	var v_intIntDict1 = null;
	var v_stringIntDict1 = null;
	var v_stackFrame2 = null;
	var v_leftValue = null;
	var v_rightValue = null;
	var v_classValue = null;
	var v_arg1 = null;
	var v_arg2 = null;
	var v_arg3 = null;
	var v_tokenList = null;
	var v_globalNameIdToPrimitiveMethodName = v_vm[4][12];
	var v_magicNumbers = v_vm[4][19];
	var v_integerSwitchesByPc = v_vm[2][3];
	var v_stringSwitchesByPc = v_vm[2][4];
	var v_integerSwitch = null;
	var v_stringSwitch = null;
	var v_esfData = v_vm[4][18];
	var v_closure = null;
	var v_parentClosure = null;
	var v_intBuffer = PST$createNewArray(16);
	var v_localsStack = v_ec[5];
	var v_localsStackSet = v_ec[6];
	var v_localsStackSetToken = v_stack[1];
	var v_localsStackCapacity = v_localsStack.length;
	var v_localsStackOffset = v_stack[2];
	var v_funcArgs = v_vm[8];
	var v_pc = v_stack[0];
	var v_nativeFp = null;
	var v_debugData = v_vm[2][5];
	var v_isBreakPointPresent = v_debugData[0];
	var v_breakpointInfo = null;
	var v_debugBreakPointTemporaryDisable = false;
	while (true) {
		v_row = v_args[v_pc];
		switch (v_ops[v_pc]) {
			case 0:
				// ADD_LITERAL;
				v_addLiteralImpl(v_vm, v_row, v_stringArgs[v_pc]);
				break;
			case 1:
				// ADD_NAME;
				v_addNameImpl(v_vm, v_stringArgs[v_pc]);
				break;
			case 2:
				// ARG_TYPE_VERIFY;
				v_len = v_row[0];
				v_i = 1;
				v_j = 0;
				while ((v_j < v_len)) {
					v_j += 1;
				}
				break;
			case 3:
				// ASSIGN_CLOSURE;
				v_value = v_valueStack[--v_valueStackSize];
				v_i = v_row[0];
				if ((v_stack[12] == null)) {
					v_closure = {};
					v_stack[12] = v_closure;
					v_closure[v_i] = [v_value];
				} else {
					v_closure = v_stack[12];
					if ((v_closure[v_i] !== undefined)) {
						v_closure[v_i][0] = v_value;
					} else {
						v_closure[v_i] = [v_value];
					}
				}
				break;
			case 4:
				// ASSIGN_INDEX;
				v_valueStackSize -= 3;
				v_value = v_valueStack[(v_valueStackSize + 2)];
				v_value2 = v_valueStack[(v_valueStackSize + 1)];
				v_root = v_valueStack[v_valueStackSize];
				v_type = v_root[0];
				v_bool1 = (v_row[0] == 1);
				if ((v_type == 6)) {
					if ((v_value2[0] == 3)) {
						v_i = v_value2[1];
						v_list1 = v_root[1];
						if ((v_list1[0] != null)) {
							v_value3 = v_canAssignTypeToGeneric(v_vm, v_value, v_list1[0], 0);
							if ((v_value3 == null)) {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, ["Cannot convert a ", v_typeToStringFromValue(v_vm, v_value), " into a ", v_typeToString(v_vm, v_list1[0], 0)].join(''));
							}
							v_value = v_value3;
						}
						if (!v_hasInterrupt) {
							if ((v_i >= v_list1[1])) {
								v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index is out of range.");
							} else {
								if ((v_i < 0)) {
									v_i += v_list1[1];
									if ((v_i < 0)) {
										v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index is out of range.");
									}
								}
							}
							if (!v_hasInterrupt) {
								v_list1[2][v_i] = v_value;
							}
						}
					} else {
						v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List index must be an integer.");
					}
				} else {
					if ((v_type == 7)) {
						v_dictImpl = v_root[1];
						if ((v_dictImpl[3] != null)) {
							v_value3 = v_canAssignTypeToGeneric(v_vm, v_value, v_dictImpl[3], 0);
							if ((v_value3 == null)) {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot assign a value to this dictionary of this type.");
							} else {
								v_value = v_value3;
							}
						}
						v_keyType = v_value2[0];
						if ((v_keyType == 3)) {
							v_intKey = v_value2[1];
						} else {
							if ((v_keyType == 5)) {
								v_stringKey = v_value2[1];
							} else {
								if ((v_keyType == 8)) {
									v_objInstance1 = v_value2[1];
									v_intKey = v_objInstance1[1];
								} else {
									v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid key for a dictionary.");
								}
							}
						}
						if (!v_hasInterrupt) {
							v_bool2 = (v_dictImpl[0] == 0);
							if ((v_dictImpl[1] != v_keyType)) {
								if ((v_dictImpl[3] != null)) {
									v_string1 = ["Cannot assign a key of type ", v_typeToStringFromValue(v_vm, v_value2), " to a dictionary that requires key types of ", v_dictKeyInfoToString(v_vm, v_dictImpl), "."].join('');
									v_hasInterrupt = v_EX_InvalidKey(v_ec, v_string1);
								} else {
									if (!v_bool2) {
										v_hasInterrupt = v_EX_InvalidKey(v_ec, "Cannot have multiple keys in one dictionary with different types.");
									}
								}
							} else {
								if (((v_keyType == 8) && (v_dictImpl[2] > 0) && (v_objInstance1[0] != v_dictImpl[2]))) {
									if (v_isClassASubclassOf(v_vm, v_objInstance1[0], v_dictImpl[2])) {
										v_hasInterrupt = v_EX_InvalidKey(v_ec, "Cannot use this type of object as a key for this dictionary.");
									}
								}
							}
						}
						if (!v_hasInterrupt) {
							if ((v_keyType == 5)) {
								v_int1 = v_dictImpl[5][v_stringKey];
								if (v_int1 === undefined) v_int1 = -1;
								if ((v_int1 == -1)) {
									v_dictImpl[5][v_stringKey] = v_dictImpl[0];
									v_dictImpl[0] += 1;
									v_dictImpl[6].push(v_value2);
									v_dictImpl[7].push(v_value);
									if (v_bool2) {
										v_dictImpl[1] = v_keyType;
									}
								} else {
									v_dictImpl[7][v_int1] = v_value;
								}
							} else {
								v_int1 = v_dictImpl[4][v_intKey];
								if (v_int1 === undefined) v_int1 = -1;
								if ((v_int1 == -1)) {
									v_dictImpl[4][v_intKey] = v_dictImpl[0];
									v_dictImpl[0] += 1;
									v_dictImpl[6].push(v_value2);
									v_dictImpl[7].push(v_value);
									if (v_bool2) {
										v_dictImpl[1] = v_keyType;
									}
								} else {
									v_dictImpl[7][v_int1] = v_value;
								}
							}
						}
					} else {
						v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, v_getTypeFromId(v_type) + " type does not support assigning to an index.");
					}
				}
				if (v_bool1) {
					v_valueStack[v_valueStackSize] = v_value;
					v_valueStackSize += 1;
				}
				break;
			case 6:
				// ASSIGN_STATIC_FIELD;
				v_classInfo = v_classTable[v_row[0]];
				v_staticConstructorNotInvoked = true;
				if ((v_classInfo[4] < 2)) {
					v_stack[0] = v_pc;
					v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST$intBuffer16);
					if ((PST$intBuffer16[0] == 1)) {
						return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
					}
					if ((v_stackFrame2 != null)) {
						v_staticConstructorNotInvoked = false;
						v_stack = v_stackFrame2;
						v_pc = v_stack[0];
						v_localsStackSetToken = v_stack[1];
						v_localsStackOffset = v_stack[2];
					}
				}
				if (v_staticConstructorNotInvoked) {
					v_valueStackSize -= 1;
					v_classInfo[5][v_row[1]] = v_valueStack[v_valueStackSize];
				}
				break;
			case 7:
				// ASSIGN_FIELD;
				v_valueStackSize -= 2;
				v_value = v_valueStack[(v_valueStackSize + 1)];
				v_value2 = v_valueStack[v_valueStackSize];
				v_nameId = v_row[2];
				if ((v_value2[0] == 8)) {
					v_objInstance1 = v_value2[1];
					v_classId = v_objInstance1[0];
					v_classInfo = v_classTable[v_classId];
					v_intIntDict1 = v_classInfo[14];
					if ((v_row[5] == v_classId)) {
						v_int1 = v_row[6];
					} else {
						v_int1 = v_intIntDict1[v_nameId];
						if (v_int1 === undefined) v_int1 = -1;
						if ((v_int1 != -1)) {
							v_int3 = v_classInfo[12][v_int1];
							if ((v_int3 > 1)) {
								if ((v_int3 == 2)) {
									if ((v_classId != v_row[3])) {
										v_int1 = -2;
									}
								} else {
									if (((v_int3 == 3) || (v_int3 == 5))) {
										if ((v_classInfo[3] != v_row[4])) {
											v_int1 = -3;
										}
									}
									if (((v_int3 == 4) || (v_int3 == 5))) {
										v_i = v_row[3];
										if ((v_classId == v_i)) {
										} else {
											v_classInfo = v_classTable[v_classInfo[0]];
											while (((v_classInfo[2] != -1) && (v_int1 < v_classTable[v_classInfo[2]][12].length))) {
												v_classInfo = v_classTable[v_classInfo[2]];
											}
											v_j = v_classInfo[0];
											if ((v_j != v_i)) {
												v_bool1 = false;
												while (((v_i != -1) && (v_classTable[v_i][2] != -1))) {
													v_i = v_classTable[v_i][2];
													if ((v_i == v_j)) {
														v_bool1 = true;
														v_i = -1;
													}
												}
												if (!v_bool1) {
													v_int1 = -4;
												}
											}
										}
										v_classInfo = v_classTable[v_classId];
									}
								}
							}
						}
						v_row[5] = v_classId;
						v_row[6] = v_int1;
					}
					if ((v_int1 > -1)) {
						v_int2 = v_classInfo[9][v_int1];
						if ((v_int2 == -1)) {
							v_intArray1 = v_classInfo[15][v_int1];
							if ((v_intArray1 == null)) {
								v_objInstance1[2][v_int1] = v_value;
							} else {
								v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_intArray1, 0);
								if ((v_value2 != null)) {
									v_objInstance1[2][v_int1] = v_value2;
								} else {
									v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot assign this type to this field.");
								}
							}
						} else {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot override a method with assignment.");
						}
					} else {
						if ((v_int1 < -1)) {
							v_string1 = v_identifiers[v_row[0]];
							if ((v_int1 == -2)) {
								v_string2 = "private";
							} else {
								if ((v_int1 == -3)) {
									v_string2 = "internal";
								} else {
									v_string2 = "protected";
								}
							}
							v_hasInterrupt = v_EX_UnknownField(v_ec, ["The field '", v_string1, "' is marked as ", v_string2, " and cannot be accessed from here."].join(''));
						} else {
							v_hasInterrupt = v_EX_InvalidAssignment(v_ec, ["'", v_classInfo[16], "' instances do not have a field called '", v_metadata[0][v_row[0]], "'"].join(''));
						}
					}
				} else {
					v_hasInterrupt = v_EX_InvalidAssignment(v_ec, "Cannot assign to a field on this type.");
				}
				if ((v_row[1] == 1)) {
					v_valueStack[v_valueStackSize++] = v_value;
				}
				break;
			case 8:
				// ASSIGN_THIS_FIELD;
				v_objInstance2 = v_stack[6][1];
				v_objInstance2[2][v_row[0]] = v_valueStack[--v_valueStackSize];
				break;
			case 5:
				// ASSIGN_LOCAL;
				v_i = (v_localsStackOffset + v_row[0]);
				v_localsStack[v_i] = v_valueStack[--v_valueStackSize];
				v_localsStackSet[v_i] = v_localsStackSetToken;
				break;
			case 9:
				// BINARY_OP;
				v_rightValue = v_valueStack[--v_valueStackSize];
				v_leftValue = v_valueStack[(v_valueStackSize - 1)];
				switch (((((v_leftValue[0] * 15) + v_row[0]) * 11) + v_rightValue[0])) {
					case 553:
						// int ** int;
						if ((v_rightValue[1] == 0)) {
							v_value = v_VALUE_INT_ONE;
						} else {
							if ((v_rightValue[1] > 0)) {
								v_value = v_buildInteger(v_globals, Math.floor(Math.pow(v_leftValue[1], v_rightValue[1])));
							} else {
								v_value = v_buildFloat(v_globals, Math.pow(v_leftValue[1], v_rightValue[1]));
							}
						}
						break;
					case 554:
						// int ** float;
						v_value = v_buildFloat(v_globals, (0.0 + Math.pow(v_leftValue[1], v_rightValue[1])));
						break;
					case 718:
						// float ** int;
						v_value = v_buildFloat(v_globals, (0.0 + Math.pow(v_leftValue[1], v_rightValue[1])));
						break;
					case 719:
						// float ** float;
						v_value = v_buildFloat(v_globals, (0.0 + Math.pow(v_leftValue[1], v_rightValue[1])));
						break;
					case 708:
						// float % float;
						v_float1 = v_rightValue[1];
						if ((v_float1 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
						} else {
							v_float3 = (v_leftValue[1] % v_float1);
							if ((v_float3 < 0)) {
								v_float3 += v_float1;
							}
							v_value = v_buildFloat(v_globals, v_float3);
						}
						break;
					case 707:
						// float % int;
						v_int1 = v_rightValue[1];
						if ((v_int1 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
						} else {
							v_float1 = (v_leftValue[1] % v_int1);
							if ((v_float1 < 0)) {
								v_float1 += v_int1;
							}
							v_value = v_buildFloat(v_globals, v_float1);
						}
						break;
					case 543:
						// int % float;
						v_float3 = v_rightValue[1];
						if ((v_float3 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
						} else {
							v_float1 = (v_leftValue[1] % v_float3);
							if ((v_float1 < 0)) {
								v_float1 += v_float3;
							}
							v_value = v_buildFloat(v_globals, v_float1);
						}
						break;
					case 542:
						// int % int;
						v_int2 = v_rightValue[1];
						if ((v_int2 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Modulo by 0.");
						} else {
							v_int1 = (v_leftValue[1] % v_int2);
							if ((v_int1 < 0)) {
								v_int1 += v_int2;
							}
							v_value = v_buildInteger(v_globals, v_int1);
						}
						break;
					case 996:
						// list + list;
						v_value = [6, v_valueConcatLists(v_leftValue[1], v_rightValue[1])];
						break;
					case 498:
						// int + int;
						v_int1 = (v_leftValue[1] + v_rightValue[1]);
						if ((v_int1 < 0)) {
							if ((v_int1 > -257)) {
								v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1];
							} else {
								v_value = [3, v_int1];
							}
						} else {
							if ((v_int1 < 2049)) {
								v_value = v_INTEGER_POSITIVE_CACHE[v_int1];
							} else {
								v_value = [3, v_int1];
							}
						}
						break;
					case 509:
						// int - int;
						v_int1 = (v_leftValue[1] - v_rightValue[1]);
						if ((v_int1 < 0)) {
							if ((v_int1 > -257)) {
								v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1];
							} else {
								v_value = [3, v_int1];
							}
						} else {
							if ((v_int1 < 2049)) {
								v_value = v_INTEGER_POSITIVE_CACHE[v_int1];
							} else {
								v_value = [3, v_int1];
							}
						}
						break;
					case 520:
						// int * int;
						v_int1 = (v_leftValue[1] * v_rightValue[1]);
						if ((v_int1 < 0)) {
							if ((v_int1 > -257)) {
								v_value = v_INTEGER_NEGATIVE_CACHE[-v_int1];
							} else {
								v_value = [3, v_int1];
							}
						} else {
							if ((v_int1 < 2049)) {
								v_value = v_INTEGER_POSITIVE_CACHE[v_int1];
							} else {
								v_value = [3, v_int1];
							}
						}
						break;
					case 531:
						// int / int;
						v_int1 = v_leftValue[1];
						v_int2 = v_rightValue[1];
						if ((v_int2 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
						} else {
							if ((v_int1 == 0)) {
								v_value = v_VALUE_INT_ZERO;
							} else {
								if (((v_int1 % v_int2) == 0)) {
									v_int3 = Math.floor(v_int1 / v_int2);
								} else {
									if ((((v_int1 < 0)) != ((v_int2 < 0)))) {
										v_float1 = (1 + ((-1.0 * v_int1) / v_int2));
										v_float1 -= (v_float1 % 1.0);
										v_int3 = Math.floor((-v_float1));
									} else {
										v_int3 = Math.floor(v_int1 / v_int2);
									}
								}
								if ((v_int3 < 0)) {
									if ((v_int3 > -257)) {
										v_value = v_INTEGER_NEGATIVE_CACHE[-v_int3];
									} else {
										v_value = [3, v_int3];
									}
								} else {
									if ((v_int3 < 2049)) {
										v_value = v_INTEGER_POSITIVE_CACHE[v_int3];
									} else {
										v_value = [3, v_int3];
									}
								}
							}
						}
						break;
					case 663:
						// float + int;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] + v_rightValue[1]));
						break;
					case 499:
						// int + float;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] + v_rightValue[1]));
						break;
					case 664:
						// float + float;
						v_float1 = (v_leftValue[1] + v_rightValue[1]);
						if ((v_float1 == 0)) {
							v_value = v_VALUE_FLOAT_ZERO;
						} else {
							if ((v_float1 == 1)) {
								v_value = v_VALUE_FLOAT_ONE;
							} else {
								v_value = [4, v_float1];
							}
						}
						break;
					case 510:
						// int - float;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] - v_rightValue[1]));
						break;
					case 674:
						// float - int;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] - v_rightValue[1]));
						break;
					case 675:
						// float - float;
						v_float1 = (v_leftValue[1] - v_rightValue[1]);
						if ((v_float1 == 0)) {
							v_value = v_VALUE_FLOAT_ZERO;
						} else {
							if ((v_float1 == 1)) {
								v_value = v_VALUE_FLOAT_ONE;
							} else {
								v_value = [4, v_float1];
							}
						}
						break;
					case 685:
						// float * int;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] * v_rightValue[1]));
						break;
					case 521:
						// int * float;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] * v_rightValue[1]));
						break;
					case 686:
						// float * float;
						v_value = v_buildFloat(v_globals, (v_leftValue[1] * v_rightValue[1]));
						break;
					case 532:
						// int / float;
						v_float1 = v_rightValue[1];
						if ((v_float1 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
						} else {
							v_value = v_buildFloat(v_globals, (v_leftValue[1] / v_float1));
						}
						break;
					case 696:
						// float / int;
						v_int1 = v_rightValue[1];
						if ((v_int1 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
						} else {
							v_value = v_buildFloat(v_globals, (v_leftValue[1] / v_int1));
						}
						break;
					case 697:
						// float / float;
						v_float1 = v_rightValue[1];
						if ((v_float1 == 0)) {
							v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Division by 0.");
						} else {
							v_value = v_buildFloat(v_globals, (v_leftValue[1] / v_float1));
						}
						break;
					case 564:
						// int & int;
						v_value = v_buildInteger(v_globals, (v_leftValue[1] & v_rightValue[1]));
						break;
					case 575:
						// int | int;
						v_value = v_buildInteger(v_globals, (v_leftValue[1] | v_rightValue[1]));
						break;
					case 586:
						// int ^ int;
						v_value = v_buildInteger(v_globals, (v_leftValue[1] ^ v_rightValue[1]));
						break;
					case 597:
						// int << int;
						v_int1 = v_rightValue[1];
						if ((v_int1 < 0)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot bit shift by a negative number.");
						} else {
							v_value = v_buildInteger(v_globals, (v_leftValue[1] << v_int1));
						}
						break;
					case 608:
						// int >> int;
						v_int1 = v_rightValue[1];
						if ((v_int1 < 0)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot bit shift by a negative number.");
						} else {
							v_value = v_buildInteger(v_globals, (v_leftValue[1] >> v_int1));
						}
						break;
					case 619:
						// int < int;
						if ((v_leftValue[1] < v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 630:
						// int <= int;
						if ((v_leftValue[1] <= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 784:
						// float < int;
						if ((v_leftValue[1] < v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 795:
						// float <= int;
						if ((v_leftValue[1] <= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 620:
						// int < float;
						if ((v_leftValue[1] < v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 631:
						// int <= float;
						if ((v_leftValue[1] <= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 785:
						// float < float;
						if ((v_leftValue[1] < v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 796:
						// float <= float;
						if ((v_leftValue[1] <= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 652:
						// int >= int;
						if ((v_leftValue[1] >= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 641:
						// int > int;
						if ((v_leftValue[1] > v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 817:
						// float >= int;
						if ((v_leftValue[1] >= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 806:
						// float > int;
						if ((v_leftValue[1] > v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 653:
						// int >= float;
						if ((v_leftValue[1] >= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 642:
						// int > float;
						if ((v_leftValue[1] > v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 818:
						// float >= float;
						if ((v_leftValue[1] >= v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 807:
						// float > float;
						if ((v_leftValue[1] > v_rightValue[1])) {
							v_value = v_VALUE_TRUE;
						} else {
							v_value = v_VALUE_FALSE;
						}
						break;
					case 830:
						// string + string;
						v_value = [5, v_leftValue[1] + v_rightValue[1]];
						break;
					case 850:
						// string * int;
						v_value = v_multiplyString(v_globals, v_leftValue, v_leftValue[1], v_rightValue[1]);
						break;
					case 522:
						// int * string;
						v_value = v_multiplyString(v_globals, v_rightValue, v_rightValue[1], v_leftValue[1]);
						break;
					case 1015:
						// list * int;
						v_int1 = v_rightValue[1];
						if ((v_int1 < 0)) {
							v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot multiply list by negative number.");
						} else {
							v_value = [6, v_valueMultiplyList(v_leftValue[1], v_int1)];
						}
						break;
					case 523:
						// int * list;
						v_int1 = v_leftValue[1];
						if ((v_int1 < 0)) {
							v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot multiply list by negative number.");
						} else {
							v_value = [6, v_valueMultiplyList(v_rightValue[1], v_int1)];
						}
						break;
					default:
						if (((v_row[0] == 0) && (((v_leftValue[0] == 5) || (v_rightValue[0] == 5))))) {
							v_value = [5, v_valueToString(v_vm, v_leftValue) + v_valueToString(v_vm, v_rightValue)];
						} else {
							// unrecognized op;
							v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, ["The '", v_getBinaryOpFromId(v_row[0]), "' operator is not supported for these types: ", v_getTypeFromId(v_leftValue[0]), " and ", v_getTypeFromId(v_rightValue[0])].join(''));
						}
						break;
				}
				v_valueStack[(v_valueStackSize - 1)] = v_value;
				break;
			case 10:
				// BOOLEAN_NOT;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				if ((v_value[0] != 2)) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
				} else {
					if (v_value[1]) {
						v_valueStack[(v_valueStackSize - 1)] = v_VALUE_FALSE;
					} else {
						v_valueStack[(v_valueStackSize - 1)] = v_VALUE_TRUE;
					}
				}
				break;
			case 11:
				// BREAK;
				if ((v_row[0] == 1)) {
					v_pc += v_row[1];
				} else {
					v_intArray1 = v_esfData[v_pc];
					v_pc = (v_intArray1[1] - 1);
					v_valueStackSize = v_stack[7];
					v_stack[10] = 1;
				}
				break;
			case 12:
				// CALL_FUNCTION;
				v_type = v_row[0];
				v_argCount = v_row[1];
				v_functionId = v_row[2];
				v_returnValueUsed = (v_row[3] == 1);
				v_classId = v_row[4];
				if (((v_type == 2) || (v_type == 6))) {
					// constructor or static method;
					v_classInfo = v_metadata[9][v_classId];
					v_staticConstructorNotInvoked = true;
					if ((v_classInfo[4] < 2)) {
						v_stack[0] = v_pc;
						v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST$intBuffer16);
						if ((PST$intBuffer16[0] == 1)) {
							return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
						}
						if ((v_stackFrame2 != null)) {
							v_staticConstructorNotInvoked = false;
							v_stack = v_stackFrame2;
							v_pc = v_stack[0];
							v_localsStackSetToken = v_stack[1];
							v_localsStackOffset = v_stack[2];
						}
					}
				} else {
					v_staticConstructorNotInvoked = true;
				}
				if (v_staticConstructorNotInvoked) {
					v_bool1 = true;
					// construct args array;
					if ((v_argCount == -1)) {
						v_valueStackSize -= 1;
						v_value = v_valueStack[v_valueStackSize];
						if ((v_value[0] == 1)) {
							v_argCount = 0;
						} else {
							if ((v_value[0] == 6)) {
								v_list1 = v_value[1];
								v_argCount = v_list1[1];
								v_i = (v_argCount - 1);
								while ((v_i >= 0)) {
									v_funcArgs[v_i] = v_list1[2][v_i];
									v_i -= 1;
								}
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Function pointers' .invoke method requires a list argument.");
							}
						}
					} else {
						v_i = (v_argCount - 1);
						while ((v_i >= 0)) {
							v_valueStackSize -= 1;
							v_funcArgs[v_i] = v_valueStack[v_valueStackSize];
							v_i -= 1;
						}
					}
					if (!v_hasInterrupt) {
						if ((v_type == 3)) {
							v_value = v_stack[6];
							v_objInstance1 = v_value[1];
							if ((v_objInstance1[0] != v_classId)) {
								v_int2 = v_row[5];
								if ((v_int2 != -1)) {
									v_classInfo = v_classTable[v_objInstance1[0]];
									v_functionId = v_classInfo[9][v_int2];
								}
							}
						} else {
							if ((v_type == 5)) {
								// field invocation;
								v_valueStackSize -= 1;
								v_value = v_valueStack[v_valueStackSize];
								v_localeId = v_row[5];
								switch (v_value[0]) {
									case 1:
										v_hasInterrupt = v_EX_NullReference(v_ec, "Invoked method on null.");
										break;
									case 8:
										// field invoked on an object instance.;
										v_objInstance1 = v_value[1];
										v_int1 = v_objInstance1[0];
										v_classInfo = v_classTable[v_int1];
										v_intIntDict1 = v_classInfo[14];
										v_int1 = ((v_row[4] * v_magicNumbers[2]) + v_row[5]);
										v_i = v_intIntDict1[v_int1];
										if (v_i === undefined) v_i = -1;
										if ((v_i != -1)) {
											v_int1 = v_intIntDict1[v_int1];
											v_functionId = v_classInfo[9][v_int1];
											if ((v_functionId > 0)) {
												v_type = 3;
											} else {
												v_value = v_objInstance1[2][v_int1];
												v_type = 4;
												v_valueStack[v_valueStackSize] = v_value;
												v_valueStackSize += 1;
											}
										} else {
											v_hasInterrupt = v_EX_UnknownField(v_ec, "Unknown field.");
										}
										break;
									case 10:
										// field invocation on a class object instance.;
										v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value[0], v_classId);
										if ((v_functionId < 0)) {
											v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "Class definitions do not have that method.");
										} else {
											v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value[0], v_classId);
											if ((v_functionId < 0)) {
												v_hasInterrupt = v_EX_InvalidInvocation(v_ec, v_getTypeFromId(v_value[0]) + " does not have that method.");
											} else {
												if ((v_globalNameIdToPrimitiveMethodName[v_classId] == 8)) {
													v_type = 6;
													v_classValue = v_value[1];
													if (v_classValue[0]) {
														v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot create an instance of an interface.");
													} else {
														v_classId = v_classValue[1];
														if (!v_returnValueUsed) {
															v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot create an instance and not use the output.");
														} else {
															v_classInfo = v_metadata[9][v_classId];
															v_functionId = v_classInfo[7];
														}
													}
												} else {
													v_type = 9;
												}
											}
										}
										break;
									default:
										// primitive method suspected.;
										v_functionId = v_resolvePrimitiveMethodName2(v_globalNameIdToPrimitiveMethodName, v_value[0], v_classId);
										if ((v_functionId < 0)) {
											v_hasInterrupt = v_EX_InvalidInvocation(v_ec, v_getTypeFromId(v_value[0]) + " does not have that method.");
										} else {
											v_type = 9;
										}
										break;
								}
							}
						}
					}
					if (((v_type == 4) && !v_hasInterrupt)) {
						// pointer provided;
						v_valueStackSize -= 1;
						v_value = v_valueStack[v_valueStackSize];
						if ((v_value[0] == 9)) {
							v_functionPointer1 = v_value[1];
							switch (v_functionPointer1[0]) {
								case 1:
									// pointer to a function;
									v_functionId = v_functionPointer1[3];
									v_type = 1;
									break;
								case 2:
									// pointer to a method;
									v_functionId = v_functionPointer1[3];
									v_value = v_functionPointer1[1];
									v_type = 3;
									break;
								case 3:
									// pointer to a static method;
									v_functionId = v_functionPointer1[3];
									v_classId = v_functionPointer1[2];
									v_type = 2;
									break;
								case 4:
									// pointer to a primitive method;
									v_value = v_functionPointer1[1];
									v_functionId = v_functionPointer1[3];
									v_type = 9;
									break;
								case 5:
									// lambda instance;
									v_value = v_functionPointer1[1];
									v_functionId = v_functionPointer1[3];
									v_type = 10;
									v_closure = v_functionPointer1[4];
									break;
							}
						} else {
							v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "This type cannot be invoked like a function.");
						}
					}
					if (((v_type == 9) && !v_hasInterrupt)) {
						// primitive method invocation;
						v_output = v_VALUE_NULL;
						v_primitiveMethodToCoreLibraryFallback = false;
						switch (v_value[0]) {
							case 5:
								// ...on a string;
								v_string1 = v_value[1];
								switch (v_functionId) {
									case 7:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string contains method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 5)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string contains method requires another string as input.");
											} else {
												if ((v_string1.indexOf(v_value2[1]) != -1)) {
													v_output = v_VALUE_TRUE;
												} else {
													v_output = v_VALUE_FALSE;
												}
											}
										}
										break;
									case 9:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string endsWith method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 5)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string endsWith method requires another string as input.");
											} else {
												if (PST$stringEndsWith(v_string1, v_value2[1])) {
													v_output = v_VALUE_TRUE;
												} else {
													v_output = v_VALUE_FALSE;
												}
											}
										}
										break;
									case 13:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string indexOf method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 5)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string indexOf method requires another string as input.");
											} else {
												v_output = v_buildInteger(v_globals, v_string1.indexOf(v_value2[1]));
											}
										}
										break;
									case 19:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string lower method", 0, v_argCount));
										} else {
											v_output = v_buildString(v_globals, v_string1.toLowerCase());
										}
										break;
									case 20:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string ltrim method", 0, v_argCount));
										} else {
											v_output = v_buildString(v_globals, PST$stringTrimOneSide(v_string1, true));
										}
										break;
									case 25:
										if ((v_argCount != 2)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string replace method", 2, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											v_value3 = v_funcArgs[1];
											if (((v_value2[0] != 5) || (v_value3[0] != 5))) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string replace method requires 2 strings as input.");
											} else {
												v_output = v_buildString(v_globals, v_string1.split(v_value2[1]).join(v_value3[1]));
											}
										}
										break;
									case 26:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string reverse method", 0, v_argCount));
										} else {
											v_output = v_buildString(v_globals, v_string1.split('').reverse().join(''));
										}
										break;
									case 27:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string rtrim method", 0, v_argCount));
										} else {
											v_output = v_buildString(v_globals, PST$stringTrimOneSide(v_string1, false));
										}
										break;
									case 30:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string split method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 5)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string split method requires another string as input.");
											} else {
												v_stringList = v_string1.split(v_value2[1]);
												v_len = v_stringList.length;
												v_list1 = v_makeEmptyList(v_globals[14], v_len);
												v_i = 0;
												while ((v_i < v_len)) {
													v_list1[2].push(v_buildString(v_globals, v_stringList[v_i]));
													v_i += 1;
												}
												v_list1[1] = v_len;
												v_output = [6, v_list1];
											}
										}
										break;
									case 31:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string startsWith method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 5)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "string startsWith method requires another string as input.");
											} else {
												if ((v_string1.indexOf(v_value2[1]) == 0)) {
													v_output = v_VALUE_TRUE;
												} else {
													v_output = v_VALUE_FALSE;
												}
											}
										}
										break;
									case 32:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string trim method", 0, v_argCount));
										} else {
											v_output = v_buildString(v_globals, v_string1.trim());
										}
										break;
									case 33:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("string upper method", 0, v_argCount));
										} else {
											v_output = v_buildString(v_globals, v_string1.toUpperCase());
										}
										break;
									default:
										v_output = null;
										break;
								}
								break;
							case 6:
								// ...on a list;
								v_list1 = v_value[1];
								switch (v_functionId) {
									case 0:
										if ((v_argCount == 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List add method requires at least one argument.");
										} else {
											v_intArray1 = v_list1[0];
											v_i = 0;
											while ((v_i < v_argCount)) {
												v_value = v_funcArgs[v_i];
												if ((v_intArray1 != null)) {
													v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_intArray1, 0);
													if ((v_value2 == null)) {
														v_hasInterrupt = v_EX_InvalidArgument(v_ec, ["Cannot convert a ", v_typeToStringFromValue(v_vm, v_value), " into a ", v_typeToString(v_vm, v_list1[0], 0)].join(''));
													}
													v_list1[2].push(v_value2);
												} else {
													v_list1[2].push(v_value);
												}
												v_i += 1;
											}
											v_list1[1] += v_argCount;
											v_output = v_VALUE_NULL;
										}
										break;
									case 3:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list choice method", 0, v_argCount));
										} else {
											v_len = v_list1[1];
											if ((v_len == 0)) {
												v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "Cannot use list.choice() method on an empty list.");
											} else {
												v_i = Math.floor(((Math.random() * v_len)));
												v_output = v_list1[2][v_i];
											}
										}
										break;
									case 4:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list clear method", 0, v_argCount));
										} else {
											if ((v_list1[1] > 0)) {
												PST$clearList(v_list1[2]);
												v_list1[1] = 0;
											}
										}
										break;
									case 5:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list clone method", 0, v_argCount));
										} else {
											v_len = v_list1[1];
											v_list2 = v_makeEmptyList(v_list1[0], v_len);
											v_i = 0;
											while ((v_i < v_len)) {
												v_list2[2].push(v_list1[2][v_i]);
												v_i += 1;
											}
											v_list2[1] = v_len;
											v_output = [6, v_list2];
										}
										break;
									case 6:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list concat method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 6)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list concat methods requires a list as an argument.");
											} else {
												v_list2 = v_value2[1];
												v_intArray1 = v_list1[0];
												if (((v_intArray1 != null) && !v_canAssignGenericToGeneric(v_vm, v_list2[0], 0, v_intArray1, 0, v_intBuffer))) {
													v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot concat a list: incompatible types.");
												} else {
													if (((v_intArray1 != null) && (v_intArray1[0] == 4) && (v_list2[0][0] == 3))) {
														v_bool1 = true;
													} else {
														v_bool1 = false;
													}
													v_len = v_list2[1];
													v_i = 0;
													while ((v_i < v_len)) {
														v_value = v_list2[2][v_i];
														if (v_bool1) {
															v_value = v_buildFloat(v_globals, (0.0 + v_value[1]));
														}
														v_list1[2].push(v_value);
														v_i += 1;
													}
												}
											}
										}
										break;
									case 7:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list contains method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											v_len = v_list1[1];
											v_output = v_VALUE_FALSE;
											v_i = 0;
											while ((v_i < v_len)) {
												v_value = v_list1[2][v_i];
												if ((v_doEqualityComparisonAndReturnCode(v_value2, v_value) == 1)) {
													v_output = v_VALUE_TRUE;
													v_i = v_len;
												}
												v_i += 1;
											}
										}
										break;
									case 10:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list filter method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 9)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list filter method requires a function pointer as its argument.");
											} else {
												v_primitiveMethodToCoreLibraryFallback = true;
												v_functionId = v_metadata[15][0];
												v_funcArgs[1] = v_value;
												v_argCount = 2;
												v_output = null;
											}
										}
										break;
									case 14:
										if ((v_argCount != 2)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list insert method", 1, v_argCount));
										} else {
											v_value = v_funcArgs[0];
											v_value2 = v_funcArgs[1];
											if ((v_value[0] != 3)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "First argument of list.insert must be an integer index.");
											} else {
												v_intArray1 = v_list1[0];
												if ((v_intArray1 != null)) {
													v_value3 = v_canAssignTypeToGeneric(v_vm, v_value2, v_intArray1, 0);
													if ((v_value3 == null)) {
														v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot insert this type into this type of list.");
													}
													v_value2 = v_value3;
												}
												if (!v_hasInterrupt) {
													v_int1 = v_value[1];
													v_len = v_list1[1];
													if ((v_int1 < 0)) {
														v_int1 += v_len;
													}
													if ((v_int1 == v_len)) {
														v_list1[2].push(v_value2);
														v_list1[1] += 1;
													} else {
														if (((v_int1 < 0) || (v_int1 >= v_len))) {
															v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index out of range.");
														} else {
															v_list1[2].splice(v_int1, 0, v_value2);
															v_list1[1] += 1;
														}
													}
												}
											}
										}
										break;
									case 17:
										if ((v_argCount != 1)) {
											if ((v_argCount == 0)) {
												v_value2 = v_globals[8];
											} else {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list join method", 1, v_argCount));
											}
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 5)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Argument of list.join needs to be a string.");
											}
										}
										if (!v_hasInterrupt) {
											v_stringList1 = [];
											v_string1 = v_value2[1];
											v_len = v_list1[1];
											v_i = 0;
											while ((v_i < v_len)) {
												v_value = v_list1[2][v_i];
												if ((v_value[0] != 5)) {
													v_string2 = v_valueToString(v_vm, v_value);
												} else {
													v_string2 = v_value[1];
												}
												v_stringList1.push(v_string2);
												v_i += 1;
											}
											v_output = v_buildString(v_globals, v_stringList1.join(v_string1));
										}
										break;
									case 21:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list map method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 9)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list map method requires a function pointer as its argument.");
											} else {
												v_primitiveMethodToCoreLibraryFallback = true;
												v_functionId = v_metadata[15][1];
												v_funcArgs[1] = v_value;
												v_argCount = 2;
												v_output = null;
											}
										}
										break;
									case 23:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list pop method", 0, v_argCount));
										} else {
											v_len = v_list1[1];
											if ((v_len < 1)) {
												v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Cannot pop from an empty list.");
											} else {
												v_len -= 1;
												v_value = v_list1[2].pop();
												if (v_returnValueUsed) {
													v_output = v_value;
												}
												v_list1[1] = v_len;
											}
										}
										break;
									case 24:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list remove method", 1, v_argCount));
										} else {
											v_value = v_funcArgs[0];
											if ((v_value[0] != 3)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Argument of list.remove must be an integer index.");
											} else {
												v_int1 = v_value[1];
												v_len = v_list1[1];
												if ((v_int1 < 0)) {
													v_int1 += v_len;
												}
												if (((v_int1 < 0) || (v_int1 >= v_len))) {
													v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "Index out of range.");
												} else {
													if (v_returnValueUsed) {
														v_output = v_list1[2][v_int1];
													}
													v_len = (v_list1[1] - 1);
													v_list1[1] = v_len;
													v_list1[2].splice(v_int1, 1);
												}
											}
										}
										break;
									case 26:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list reverse method", 0, v_argCount));
										} else {
											v_list1[2].reverse();
										}
										break;
									case 28:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("list shuffle method", 0, v_argCount));
										} else {
											PST$shuffle(v_list1[2]);
										}
										break;
									case 29:
										if ((v_argCount == 0)) {
											v_sortLists(v_list1, v_list1, PST$intBuffer16);
											if ((PST$intBuffer16[0] > 0)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid list to sort. All items must be numbers or all strings, but not mixed.");
											}
										} else {
											if ((v_argCount == 1)) {
												v_value2 = v_funcArgs[0];
												if ((v_value2[0] == 9)) {
													v_primitiveMethodToCoreLibraryFallback = true;
													v_functionId = v_metadata[15][2];
													v_funcArgs[1] = v_value;
													v_argCount = 2;
												} else {
													v_hasInterrupt = v_EX_InvalidArgument(v_ec, "list.sort(get_key_function) requires a function pointer as its argument.");
												}
												v_output = null;
											}
										}
										break;
									default:
										v_output = null;
										break;
								}
								break;
							case 7:
								// ...on a dictionary;
								v_dictImpl = v_value[1];
								switch (v_functionId) {
									case 4:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary clear method", 0, v_argCount));
										} else {
											if ((v_dictImpl[0] > 0)) {
												v_dictImpl[4] = {};
												v_dictImpl[5] = {};
												PST$clearList(v_dictImpl[6]);
												PST$clearList(v_dictImpl[7]);
												v_dictImpl[0] = 0;
											}
										}
										break;
									case 5:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary clone method", 0, v_argCount));
										} else {
											v_output = [7, v_cloneDictionary(v_dictImpl, null)];
										}
										break;
									case 7:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary contains method", 1, v_argCount));
										} else {
											v_value = v_funcArgs[0];
											v_output = v_VALUE_FALSE;
											if ((v_value[0] == 5)) {
												if ((v_dictImpl[5][v_value[1]] !== undefined)) {
													v_output = v_VALUE_TRUE;
												}
											} else {
												if ((v_value[0] == 3)) {
													v_i = v_value[1];
												} else {
													v_i = (v_value[1])[1];
												}
												if ((v_dictImpl[4][v_i] !== undefined)) {
													v_output = v_VALUE_TRUE;
												}
											}
										}
										break;
									case 11:
										if (((v_argCount != 1) && (v_argCount != 2))) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Dictionary get method requires 1 or 2 arguments.");
										} else {
											v_value = v_funcArgs[0];
											switch (v_value[0]) {
												case 3:
													v_int1 = v_value[1];
													v_i = v_dictImpl[4][v_int1];
													if (v_i === undefined) v_i = -1;
													break;
												case 8:
													v_int1 = (v_value[1])[1];
													v_i = v_dictImpl[4][v_int1];
													if (v_i === undefined) v_i = -1;
													break;
												case 5:
													v_string1 = v_value[1];
													v_i = v_dictImpl[5][v_string1];
													if (v_i === undefined) v_i = -1;
													break;
											}
											if ((v_i == -1)) {
												if ((v_argCount == 2)) {
													v_output = v_funcArgs[1];
												} else {
													v_output = v_VALUE_NULL;
												}
											} else {
												v_output = v_dictImpl[7][v_i];
											}
										}
										break;
									case 18:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary keys method", 0, v_argCount));
										} else {
											v_valueList1 = v_dictImpl[6];
											v_len = v_valueList1.length;
											if ((v_dictImpl[1] == 8)) {
												v_intArray1 = PST$createNewArray(2);
												v_intArray1[0] = 8;
												v_intArray1[0] = v_dictImpl[2];
											} else {
												v_intArray1 = PST$createNewArray(1);
												v_intArray1[0] = v_dictImpl[1];
											}
											v_list1 = v_makeEmptyList(v_intArray1, v_len);
											v_i = 0;
											while ((v_i < v_len)) {
												v_list1[2].push(v_valueList1[v_i]);
												v_i += 1;
											}
											v_list1[1] = v_len;
											v_output = [6, v_list1];
										}
										break;
									case 22:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary merge method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											if ((v_value2[0] != 7)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "dictionary merge method requires another dictionary as a parameeter.");
											} else {
												v_dictImpl2 = v_value2[1];
												if ((v_dictImpl2[0] > 0)) {
													if ((v_dictImpl[0] == 0)) {
														v_value[1] = v_cloneDictionary(v_dictImpl2, null);
													} else {
														if ((v_dictImpl2[1] != v_dictImpl[1])) {
															v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionaries with different key types cannot be merged.");
														} else {
															if (((v_dictImpl2[1] == 8) && (v_dictImpl2[2] != v_dictImpl[2]) && (v_dictImpl[2] != 0) && !v_isClassASubclassOf(v_vm, v_dictImpl2[2], v_dictImpl[2]))) {
																v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionary key types are incompatible.");
															} else {
																if ((v_dictImpl[3] == null)) {
																} else {
																	if ((v_dictImpl2[3] == null)) {
																		v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionaries with different value types cannot be merged.");
																	} else {
																		if (!v_canAssignGenericToGeneric(v_vm, v_dictImpl2[3], 0, v_dictImpl[3], 0, v_intBuffer)) {
																			v_hasInterrupt = v_EX_InvalidKey(v_ec, "The dictionary value types are incompatible.");
																		}
																	}
																}
																if (!v_hasInterrupt) {
																	v_cloneDictionary(v_dictImpl2, v_dictImpl);
																}
															}
														}
													}
												}
												v_output = v_VALUE_NULL;
											}
										}
										break;
									case 24:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary remove method", 1, v_argCount));
										} else {
											v_value2 = v_funcArgs[0];
											v_bool2 = false;
											v_keyType = v_dictImpl[1];
											if (((v_dictImpl[0] > 0) && (v_keyType == v_value2[0]))) {
												if ((v_keyType == 5)) {
													v_stringKey = v_value2[1];
													if ((v_dictImpl[5][v_stringKey] !== undefined)) {
														v_i = v_dictImpl[5][v_stringKey];
														v_bool2 = true;
													}
												} else {
													if ((v_keyType == 3)) {
														v_intKey = v_value2[1];
													} else {
														v_intKey = (v_value2[1])[1];
													}
													if ((v_dictImpl[4][v_intKey] !== undefined)) {
														v_i = v_dictImpl[4][v_intKey];
														v_bool2 = true;
													}
												}
												if (v_bool2) {
													v_len = (v_dictImpl[0] - 1);
													v_dictImpl[0] = v_len;
													if ((v_i == v_len)) {
														if ((v_keyType == 5)) {
															delete v_dictImpl[5][v_stringKey];
														} else {
															delete v_dictImpl[4][v_intKey];
														}
														v_dictImpl[6].splice(v_i, 1);
														v_dictImpl[7].splice(v_i, 1);
													} else {
														v_value = v_dictImpl[6][v_len];
														v_dictImpl[6][v_i] = v_value;
														v_dictImpl[7][v_i] = v_dictImpl[7][v_len];
														v_dictImpl[6].pop();
														v_dictImpl[7].pop();
														if ((v_keyType == 5)) {
															delete v_dictImpl[5][v_stringKey];
															v_stringKey = v_value[1];
															v_dictImpl[5][v_stringKey] = v_i;
														} else {
															delete v_dictImpl[4][v_intKey];
															if ((v_keyType == 3)) {
																v_intKey = v_value[1];
															} else {
																v_intKey = (v_value[1])[1];
															}
															v_dictImpl[4][v_intKey] = v_i;
														}
													}
												}
											}
											if (!v_bool2) {
												v_hasInterrupt = v_EX_KeyNotFound(v_ec, "dictionary does not contain the given key.");
											}
										}
										break;
									case 34:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("dictionary values method", 0, v_argCount));
										} else {
											v_valueList1 = v_dictImpl[7];
											v_len = v_valueList1.length;
											v_list1 = v_makeEmptyList(v_dictImpl[3], v_len);
											v_i = 0;
											while ((v_i < v_len)) {
												v_addToList(v_list1, v_valueList1[v_i]);
												v_i += 1;
											}
											v_output = [6, v_list1];
										}
										break;
									default:
										v_output = null;
										break;
								}
								break;
							case 9:
								// ...on a function pointer;
								v_functionPointer1 = v_value[1];
								switch (v_functionId) {
									case 1:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("argCountMax method", 0, v_argCount));
										} else {
											v_functionId = v_functionPointer1[3];
											v_functionInfo = v_metadata[10][v_functionId];
											v_output = v_buildInteger(v_globals, v_functionInfo[4]);
										}
										break;
									case 2:
										if ((v_argCount > 0)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("argCountMin method", 0, v_argCount));
										} else {
											v_functionId = v_functionPointer1[3];
											v_functionInfo = v_metadata[10][v_functionId];
											v_output = v_buildInteger(v_globals, v_functionInfo[3]);
										}
										break;
									case 12:
										v_functionInfo = v_metadata[10][v_functionPointer1[3]];
										v_output = v_buildString(v_globals, v_functionInfo[9]);
										break;
									case 15:
										if ((v_argCount == 1)) {
											v_funcArgs[1] = v_funcArgs[0];
										} else {
											if ((v_argCount == 0)) {
												v_funcArgs[1] = v_VALUE_NULL;
											} else {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "invoke requires a list of arguments.");
											}
										}
										v_funcArgs[0] = v_value;
										v_argCount = 2;
										v_primitiveMethodToCoreLibraryFallback = true;
										v_functionId = v_metadata[15][3];
										v_output = null;
										break;
									default:
										v_output = null;
										break;
								}
								break;
							case 10:
								// ...on a class definition;
								v_classValue = v_value[1];
								switch (v_functionId) {
									case 12:
										v_classInfo = v_metadata[9][v_classValue[1]];
										v_output = v_buildString(v_globals, v_classInfo[16]);
										break;
									case 16:
										if ((v_argCount != 1)) {
											v_hasInterrupt = v_EX_InvalidArgument(v_ec, v_primitiveMethodWrongArgCountError("class isA method", 1, v_argCount));
										} else {
											v_int1 = v_classValue[1];
											v_value = v_funcArgs[0];
											if ((v_value[0] != 10)) {
												v_hasInterrupt = v_EX_InvalidArgument(v_ec, "class isA method requires another class reference.");
											} else {
												v_classValue = v_value[1];
												v_int2 = v_classValue[1];
												v_output = v_VALUE_FALSE;
												if (v_isClassASubclassOf(v_vm, v_int1, v_int2)) {
													v_output = v_VALUE_TRUE;
												}
											}
										}
										break;
									default:
										v_output = null;
										break;
								}
								break;
						}
						if (!v_hasInterrupt) {
							if ((v_output == null)) {
								if (v_primitiveMethodToCoreLibraryFallback) {
									v_type = 1;
									v_bool1 = true;
								} else {
									v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "primitive method not found.");
								}
							} else {
								if (v_returnValueUsed) {
									if ((v_valueStackSize == v_valueStackCapacity)) {
										v_valueStack = v_valueStackIncreaseCapacity(v_ec);
										v_valueStackCapacity = v_valueStack.length;
									}
									v_valueStack[v_valueStackSize] = v_output;
									v_valueStackSize += 1;
								}
								v_bool1 = false;
							}
						}
					}
					if ((v_bool1 && !v_hasInterrupt)) {
						// push a new frame to the stack;
						v_stack[0] = v_pc;
						v_bool1 = false;
						switch (v_type) {
							case 1:
								// function;
								v_functionInfo = v_functionTable[v_functionId];
								v_pc = v_functionInfo[2];
								v_value = null;
								v_classId = 0;
								break;
							case 10:
								// lambda;
								v_pc = v_functionId;
								v_functionInfo = v_metadata[11][v_functionId];
								v_value = null;
								v_classId = 0;
								break;
							case 2:
								// static method;
								v_functionInfo = v_functionTable[v_functionId];
								v_pc = v_functionInfo[2];
								v_value = null;
								v_classId = 0;
								break;
							case 3:
								// non-static method;
								v_functionInfo = v_functionTable[v_functionId];
								v_pc = v_functionInfo[2];
								v_classId = 0;
								break;
							case 6:
								// constructor;
								v_vm[5] += 1;
								v_classInfo = v_classTable[v_classId];
								v_valueArray1 = PST$createNewArray(v_classInfo[8]);
								v_i = (v_valueArray1.length - 1);
								while ((v_i >= 0)) {
									switch (v_classInfo[10][v_i]) {
										case 0:
											v_valueArray1[v_i] = v_classInfo[11][v_i];
											break;
										case 1:
											break;
										case 2:
											break;
									}
									v_i -= 1;
								}
								v_objInstance1 = [v_classId, v_vm[5], v_valueArray1, null, null];
								v_value = [8, v_objInstance1];
								v_functionId = v_classInfo[7];
								v_functionInfo = v_functionTable[v_functionId];
								v_pc = v_functionInfo[2];
								v_classId = 0;
								if (v_returnValueUsed) {
									v_returnValueUsed = false;
									if ((v_valueStackSize == v_valueStackCapacity)) {
										v_valueStack = v_valueStackIncreaseCapacity(v_ec);
										v_valueStackCapacity = v_valueStack.length;
									}
									v_valueStack[v_valueStackSize] = v_value;
									v_valueStackSize += 1;
								}
								break;
							case 7:
								// base constructor;
								v_value = v_stack[6];
								v_classInfo = v_classTable[v_classId];
								v_functionId = v_classInfo[7];
								v_functionInfo = v_functionTable[v_functionId];
								v_pc = v_functionInfo[2];
								v_classId = 0;
								break;
						}
						if (((v_argCount < v_functionInfo[3]) || (v_argCount > v_functionInfo[4]))) {
							v_pc = v_stack[0];
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Incorrect number of args were passed to this function.");
						} else {
							v_int1 = v_functionInfo[7];
							v_int2 = v_stack[3];
							if ((v_localsStackCapacity <= (v_int2 + v_int1))) {
								v_increaseLocalsStackCapacity(v_ec, v_int1);
								v_localsStack = v_ec[5];
								v_localsStackSet = v_ec[6];
								v_localsStackCapacity = v_localsStack.length;
							}
							v_localsStackSetToken = (v_ec[7] + 1);
							v_ec[7] = v_localsStackSetToken;
							if ((v_localsStackSetToken > 2000000000)) {
								v_resetLocalsStackTokens(v_ec, v_stack);
								v_localsStackSetToken = 2;
							}
							v_localsStackOffset = v_int2;
							if ((v_type != 10)) {
								v_closure = null;
							}
							// invoke the function;
							v_stack = [v_pc, v_localsStackSetToken, v_localsStackOffset, (v_localsStackOffset + v_int1), v_stack, v_returnValueUsed, v_value, v_valueStackSize, 0, (v_stack[9] + 1), 0, null, v_closure, null];
							v_i = 0;
							while ((v_i < v_argCount)) {
								v_int1 = (v_localsStackOffset + v_i);
								v_localsStack[v_int1] = v_funcArgs[v_i];
								v_localsStackSet[v_int1] = v_localsStackSetToken;
								v_i += 1;
							}
							if ((v_argCount != v_functionInfo[3])) {
								v_int1 = (v_argCount - v_functionInfo[3]);
								if ((v_int1 > 0)) {
									v_pc += v_functionInfo[8][v_int1];
									v_stack[0] = v_pc;
								}
							}
							if ((v_stack[9] > 1000)) {
								v_hasInterrupt = v_EX_Fatal(v_ec, "Stack overflow.");
							}
						}
					}
				}
				break;
			case 13:
				// CAST;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				v_value2 = v_canAssignTypeToGeneric(v_vm, v_value, v_row, 0);
				if ((v_value2 == null)) {
					if (((v_value[0] == 4) && (v_row[0] == 3))) {
						if ((v_row[1] == 1)) {
							v_float1 = v_value[1];
							v_i = Math.floor(v_float1);
							if ((v_i < 0)) {
								if ((v_i > -257)) {
									v_value2 = v_globals[10][-v_i];
								} else {
									v_value2 = [3, v_i];
								}
							} else {
								if ((v_i < 2049)) {
									v_value2 = v_globals[9][v_i];
								} else {
									v_value2 = [3, v_i];
								}
							}
						}
					} else {
						if (((v_value[0] == 3) && (v_row[0] == 4))) {
							v_int1 = v_value[1];
							if ((v_int1 == 0)) {
								v_value2 = v_VALUE_FLOAT_ZERO;
							} else {
								v_value2 = [4, (0.0 + v_int1)];
							}
						}
					}
					if ((v_value2 != null)) {
						v_valueStack[(v_valueStackSize - 1)] = v_value2;
					}
				}
				if ((v_value2 == null)) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, ["Cannot convert a ", v_typeToStringFromValue(v_vm, v_value), " to a ", v_typeToString(v_vm, v_row, 0)].join(''));
				} else {
					v_valueStack[(v_valueStackSize - 1)] = v_value2;
				}
				break;
			case 14:
				// CLASS_DEFINITION;
				v_initializeClass(v_pc, v_vm, v_row, v_stringArgs[v_pc]);
				v_classTable = v_metadata[9];
				break;
			case 15:
				// CNI_INVOKE;
				v_nativeFp = v_metadata[13][v_row[0]];
				if ((v_nativeFp == null)) {
					v_hasInterrupt = v_EX_InvalidInvocation(v_ec, "CNI method could not be found.");
				} else {
					v_len = v_row[1];
					v_valueStackSize -= v_len;
					v_valueArray1 = PST$createNewArray(v_len);
					v_i = 0;
					while ((v_i < v_len)) {
						v_valueArray1[v_i] = v_valueStack[(v_valueStackSize + v_i)];
						v_i += 1;
					}
					v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc);
					v_value = v_nativeFp(v_vm, v_valueArray1);
					if (v_ec[11]) {
						v_ec[11] = false;
						if ((v_ec[12] == 1)) {
							return v_suspendInterpreter();
						}
					}
					if ((v_row[2] == 1)) {
						if ((v_valueStackSize == v_valueStackCapacity)) {
							v_valueStack = v_valueStackIncreaseCapacity(v_ec);
							v_valueStackCapacity = v_valueStack.length;
						}
						v_valueStack[v_valueStackSize] = v_value;
						v_valueStackSize += 1;
					}
				}
				break;
			case 16:
				// CNI_REGISTER;
				v_nativeFp = C$common$getFunction(v_stringArgs[v_pc]);
				v_metadata[13][v_row[0]] = v_nativeFp;
				break;
			case 17:
				// COMMAND_LINE_ARGS;
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_list1 = v_makeEmptyList(v_globals[14], 3);
				v_i = 0;
				while ((v_i < v_vm[11][0].length)) {
					v_addToList(v_list1, v_buildString(v_globals, v_vm[11][0][v_i]));
					v_i += 1;
				}
				v_valueStack[v_valueStackSize] = [6, v_list1];
				v_valueStackSize += 1;
				break;
			case 18:
				// CONTINUE;
				if ((v_row[0] == 1)) {
					v_pc += v_row[1];
				} else {
					v_intArray1 = v_esfData[v_pc];
					v_pc = (v_intArray1[1] - 1);
					v_valueStackSize = v_stack[7];
					v_stack[10] = 2;
				}
				break;
			case 19:
				// CORE_FUNCTION;
				switch (v_row[0]) {
					case 1:
						// parseInt;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_VALUE_NULL;
						if ((v_arg1[0] == 5)) {
							v_string1 = (v_arg1[1]).trim();
							if (PST$is_valid_integer(v_string1)) {
								v_output = v_buildInteger(v_globals, parseInt(v_string1));
							}
						} else {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "parseInt requires a string argument.");
						}
						break;
					case 2:
						// parseFloat;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_VALUE_NULL;
						if ((v_arg1[0] == 5)) {
							v_string1 = (v_arg1[1]).trim();
							PST$floatParseHelper(v_floatList1, v_string1);
							if ((v_floatList1[0] >= 0)) {
								v_output = v_buildFloat(v_globals, v_floatList1[1]);
							}
						} else {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "parseFloat requires a string argument.");
						}
						break;
					case 3:
						// print;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_VALUE_NULL;
						v_printToStdOut(v_vm[11][2], v_valueToString(v_vm, v_arg1));
						break;
					case 4:
						// typeof;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_buildInteger(v_globals, (v_arg1[0] - 1));
						break;
					case 5:
						// typeis;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_int1 = v_arg1[0];
						v_int2 = v_row[2];
						v_output = v_VALUE_FALSE;
						while ((v_int2 > 0)) {
							if ((v_row[(2 + v_int2)] == v_int1)) {
								v_output = v_VALUE_TRUE;
								v_int2 = 0;
							} else {
								v_int2 -= 1;
							}
						}
						break;
					case 6:
						// execId;
						v_output = v_buildInteger(v_globals, v_ec[0]);
						break;
					case 7:
						// assert;
						v_valueStackSize -= 3;
						v_arg3 = v_valueStack[(v_valueStackSize + 2)];
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						if ((v_arg1[0] != 2)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Assertion expression must be a boolean.");
						} else {
							if (v_arg1[1]) {
								v_output = v_VALUE_NULL;
							} else {
								v_string1 = v_valueToString(v_vm, v_arg2);
								if (v_arg3[1]) {
									v_string1 = "Assertion failed: " + v_string1;
								}
								v_hasInterrupt = v_EX_AssertionFailed(v_ec, v_string1);
							}
						}
						break;
					case 8:
						// chr;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = null;
						if ((v_arg1[0] == 3)) {
							v_int1 = v_arg1[1];
							if (((v_int1 >= 0) && (v_int1 < 256))) {
								v_output = v_buildCommonString(v_globals, String.fromCharCode(v_int1));
							}
						}
						if ((v_output == null)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "chr requires an integer between 0 and 255.");
						}
						break;
					case 9:
						// ord;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = null;
						if ((v_arg1[0] == 5)) {
							v_string1 = v_arg1[1];
							if ((v_string1.length == 1)) {
								v_output = v_buildInteger(v_globals, v_string1.charCodeAt(0));
							}
						}
						if ((v_output == null)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "ord requires a 1 character string.");
						}
						break;
					case 10:
						// currentTime;
						v_output = v_buildFloat(v_globals, ((Date.now ? Date.now() : new Date().getTime()) / 1000.0));
						break;
					case 11:
						// sortList;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_output = v_VALUE_NULL;
						v_list1 = v_arg1[1];
						v_list2 = v_arg2[1];
						v_sortLists(v_list2, v_list1, PST$intBuffer16);
						if ((PST$intBuffer16[0] > 0)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Invalid sort keys. Keys must be all numbers or all strings, but not mixed.");
						}
						break;
					case 12:
						// abs;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_arg1;
						if ((v_arg1[0] == 3)) {
							if ((v_arg1[1] < 0)) {
								v_output = v_buildInteger(v_globals, -v_arg1[1]);
							}
						} else {
							if ((v_arg1[0] == 4)) {
								if ((v_arg1[1] < 0)) {
									v_output = v_buildFloat(v_globals, -v_arg1[1]);
								}
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "abs requires a number as input.");
							}
						}
						break;
					case 13:
						// arcCos;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arccos requires a number as input.");
							}
						}
						if (!v_hasInterrupt) {
							if (((v_float1 < -1) || (v_float1 > 1))) {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arccos requires a number in the range of -1 to 1.");
							} else {
								v_output = v_buildFloat(v_globals, Math.acos(v_float1));
							}
						}
						break;
					case 14:
						// arcSin;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arcsin requires a number as input.");
							}
						}
						if (!v_hasInterrupt) {
							if (((v_float1 < -1) || (v_float1 > 1))) {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arcsin requires a number in the range of -1 to 1.");
							} else {
								v_output = v_buildFloat(v_globals, Math.asin(v_float1));
							}
						}
						break;
					case 15:
						// arcTan;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_bool1 = false;
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_bool1 = true;
							}
						}
						if ((v_arg2[0] == 4)) {
							v_float2 = v_arg2[1];
						} else {
							if ((v_arg2[0] == 3)) {
								v_float2 = (0.0 + v_arg2[1]);
							} else {
								v_bool1 = true;
							}
						}
						if (v_bool1) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "arctan requires numeric arguments.");
						} else {
							v_output = v_buildFloat(v_globals, Math.atan2(v_float1, v_float2));
						}
						break;
					case 16:
						// cos;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
							v_output = v_buildFloat(v_globals, Math.cos(v_float1));
						} else {
							if ((v_arg1[0] == 3)) {
								v_int1 = v_arg1[1];
								v_output = v_buildFloat(v_globals, Math.cos(v_int1));
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "cos requires a number argument.");
							}
						}
						break;
					case 17:
						// ensureRange;
						v_valueStackSize -= 3;
						v_arg3 = v_valueStack[(v_valueStackSize + 2)];
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_bool1 = false;
						if ((v_arg2[0] == 4)) {
							v_float2 = v_arg2[1];
						} else {
							if ((v_arg2[0] == 3)) {
								v_float2 = (0.0 + v_arg2[1]);
							} else {
								v_bool1 = true;
							}
						}
						if ((v_arg3[0] == 4)) {
							v_float3 = v_arg3[1];
						} else {
							if ((v_arg3[0] == 3)) {
								v_float3 = (0.0 + v_arg3[1]);
							} else {
								v_bool1 = true;
							}
						}
						if ((!v_bool1 && (v_float3 < v_float2))) {
							v_float1 = v_float3;
							v_float3 = v_float2;
							v_float2 = v_float1;
							v_value = v_arg2;
							v_arg2 = v_arg3;
							v_arg3 = v_value;
						}
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_bool1 = true;
							}
						}
						if (v_bool1) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "ensureRange requires numeric arguments.");
						} else {
							if ((v_float1 < v_float2)) {
								v_output = v_arg2;
							} else {
								if ((v_float1 > v_float3)) {
									v_output = v_arg3;
								} else {
									v_output = v_arg1;
								}
							}
						}
						break;
					case 18:
						// floor;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
							v_int1 = Math.floor(v_float1);
							if ((v_int1 < 2049)) {
								if ((v_int1 >= 0)) {
									v_output = v_INTEGER_POSITIVE_CACHE[v_int1];
								} else {
									if ((v_int1 > -257)) {
										v_output = v_INTEGER_NEGATIVE_CACHE[-v_int1];
									} else {
										v_output = [3, v_int1];
									}
								}
							} else {
								v_output = [3, v_int1];
							}
						} else {
							if ((v_arg1[0] == 3)) {
								v_output = v_arg1;
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "floor expects a numeric argument.");
							}
						}
						break;
					case 19:
						// max;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_bool1 = false;
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_bool1 = true;
							}
						}
						if ((v_arg2[0] == 4)) {
							v_float2 = v_arg2[1];
						} else {
							if ((v_arg2[0] == 3)) {
								v_float2 = (0.0 + v_arg2[1]);
							} else {
								v_bool1 = true;
							}
						}
						if (v_bool1) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "max requires numeric arguments.");
						} else {
							if ((v_float1 >= v_float2)) {
								v_output = v_arg1;
							} else {
								v_output = v_arg2;
							}
						}
						break;
					case 20:
						// min;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_bool1 = false;
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_bool1 = true;
							}
						}
						if ((v_arg2[0] == 4)) {
							v_float2 = v_arg2[1];
						} else {
							if ((v_arg2[0] == 3)) {
								v_float2 = (0.0 + v_arg2[1]);
							} else {
								v_bool1 = true;
							}
						}
						if (v_bool1) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "min requires numeric arguments.");
						} else {
							if ((v_float1 <= v_float2)) {
								v_output = v_arg1;
							} else {
								v_output = v_arg2;
							}
						}
						break;
					case 21:
						// nativeInt;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_output = v_buildInteger(v_globals, (v_arg1[1])[3][v_arg2[1]]);
						break;
					case 22:
						// nativeString;
						v_valueStackSize -= 3;
						v_arg3 = v_valueStack[(v_valueStackSize + 2)];
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_string1 = (v_arg1[1])[3][v_arg2[1]];
						if (v_arg3[1]) {
							v_output = v_buildCommonString(v_globals, v_string1);
						} else {
							v_output = v_buildString(v_globals, v_string1);
						}
						break;
					case 23:
						// sign;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 3)) {
							v_float1 = (0.0 + (v_arg1[1]));
						} else {
							if ((v_arg1[0] == 4)) {
								v_float1 = v_arg1[1];
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "sign requires a number as input.");
							}
						}
						if ((v_float1 == 0)) {
							v_output = v_VALUE_INT_ZERO;
						} else {
							if ((v_float1 > 0)) {
								v_output = v_VALUE_INT_ONE;
							} else {
								v_output = v_INTEGER_NEGATIVE_CACHE[1];
							}
						}
						break;
					case 24:
						// sin;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "sin requires a number argument.");
							}
						}
						v_output = v_buildFloat(v_globals, Math.sin(v_float1));
						break;
					case 25:
						// tan;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "tan requires a number argument.");
							}
						}
						if (!v_hasInterrupt) {
							v_float2 = Math.cos(v_float1);
							if ((v_float2 < 0)) {
								v_float2 = -v_float2;
							}
							if ((v_float2 < 0.00000000015)) {
								v_hasInterrupt = v_EX_DivisionByZero(v_ec, "Tangent is undefined.");
							} else {
								v_output = v_buildFloat(v_globals, Math.tan(v_float1));
							}
						}
						break;
					case 26:
						// log;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						if ((v_arg1[0] == 4)) {
							v_float1 = v_arg1[1];
						} else {
							if ((v_arg1[0] == 3)) {
								v_float1 = (0.0 + v_arg1[1]);
							} else {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "logarithms require a number argument.");
							}
						}
						if (!v_hasInterrupt) {
							if ((v_float1 <= 0)) {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "logarithms require positive inputs.");
							} else {
								v_output = v_buildFloat(v_globals, v_fixFuzzyFloatPrecision((Math.log(v_float1) * v_arg2[1])));
							}
						}
						break;
					case 27:
						// intQueueClear;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_VALUE_NULL;
						v_objInstance1 = v_arg1[1];
						if ((v_objInstance1[3] != null)) {
							v_objInstance1[3][1] = 0;
						}
						break;
					case 28:
						// intQueueWrite16;
						v_output = v_VALUE_NULL;
						v_int1 = v_row[2];
						v_valueStackSize -= (v_int1 + 1);
						v_value = v_valueStack[v_valueStackSize];
						v_objArray1 = (v_value[1])[3];
						v_intArray1 = v_objArray1[0];
						v_len = v_objArray1[1];
						if ((v_len >= v_intArray1.length)) {
							v_intArray2 = PST$createNewArray(((v_len * 2) + 16));
							v_j = 0;
							while ((v_j < v_len)) {
								v_intArray2[v_j] = v_intArray1[v_j];
								v_j += 1;
							}
							v_intArray1 = v_intArray2;
							v_objArray1[0] = v_intArray1;
						}
						v_objArray1[1] = (v_len + 16);
						v_i = (v_int1 - 1);
						while ((v_i >= 0)) {
							v_value = v_valueStack[((v_valueStackSize + 1) + v_i)];
							if ((v_value[0] == 3)) {
								v_intArray1[(v_len + v_i)] = v_value[1];
							} else {
								if ((v_value[0] == 4)) {
									v_float1 = (0.5 + v_value[1]);
									v_intArray1[(v_len + v_i)] = Math.floor(v_float1);
								} else {
									v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Input must be integers.");
									v_i = -1;
								}
							}
							v_i -= 1;
						}
						break;
					case 29:
						// execCounter;
						v_output = v_buildInteger(v_globals, v_ec[8]);
						break;
					case 30:
						// sleep;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_float1 = v_getFloat(v_arg1);
						if ((v_row[1] == 1)) {
							if ((v_valueStackSize == v_valueStackCapacity)) {
								v_valueStack = v_valueStackIncreaseCapacity(v_ec);
								v_valueStackCapacity = v_valueStack.length;
							}
							v_valueStack[v_valueStackSize] = v_VALUE_NULL;
							v_valueStackSize += 1;
						}
						v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc);
						v_ec[13] = [3, 0, "", v_float1, null];
						v_hasInterrupt = true;
						break;
					case 31:
						// projectId;
						v_output = v_buildCommonString(v_globals, v_metadata[17]);
						break;
					case 32:
						// isJavaScript;
						v_output = v_VALUE_TRUE;
						break;
					case 33:
						// isAndroid;
						v_output = v_VALUE_FALSE;
						break;
					case 34:
						// allocNativeData;
						v_valueStackSize -= 2;
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						v_objInstance1 = v_arg1[1];
						v_int1 = v_arg2[1];
						v_objArray1 = PST$createNewArray(v_int1);
						v_objInstance1[3] = v_objArray1;
						break;
					case 35:
						// setNativeData;
						v_valueStackSize -= 3;
						v_arg3 = v_valueStack[(v_valueStackSize + 2)];
						v_arg2 = v_valueStack[(v_valueStackSize + 1)];
						v_arg1 = v_valueStack[v_valueStackSize];
						(v_arg1[1])[3][v_arg2[1]] = v_arg3[1];
						break;
					case 36:
						// getExceptionTrace;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_intList1 = v_getNativeDataItem(v_arg1, 1);
						v_list1 = v_makeEmptyList(v_globals[14], 20);
						v_output = [6, v_list1];
						if ((v_intList1 != null)) {
							v_stringList1 = v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_intList1);
							v_i = 0;
							while ((v_i < v_stringList1.length)) {
								v_addToList(v_list1, v_buildString(v_globals, v_stringList1[v_i]));
								v_i += 1;
							}
							v_reverseList(v_list1);
						}
						break;
					case 37:
						// reflectAllClasses;
						v_output = v_Reflect_allClasses(v_vm);
						break;
					case 38:
						// reflectGetMethods;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_output = v_Reflect_getMethods(v_vm, v_ec, v_arg1);
						v_hasInterrupt = (v_ec[13] != null);
						break;
					case 39:
						// reflectGetClass;
						v_arg1 = v_valueStack[--v_valueStackSize];
						if ((v_arg1[0] != 8)) {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot get class from non-instance types.");
						} else {
							v_objInstance1 = v_arg1[1];
							v_output = [10, [false, v_objInstance1[0]]];
						}
						break;
					case 40:
						// convertFloatArgsToInts;
						v_int1 = v_stack[3];
						v_i = v_localsStackOffset;
						while ((v_i < v_int1)) {
							v_value = v_localsStack[v_i];
							if ((v_localsStackSet[v_i] != v_localsStackSetToken)) {
								v_i += v_int1;
							} else {
								if ((v_value[0] == 4)) {
									v_float1 = v_value[1];
									v_int2 = Math.floor(v_float1);
									if (((v_int2 >= 0) && (v_int2 < 2049))) {
										v_localsStack[v_i] = v_INTEGER_POSITIVE_CACHE[v_int2];
									} else {
										v_localsStack[v_i] = v_buildInteger(v_globals, v_int2);
									}
								}
							}
							v_i += 1;
						}
						break;
					case 41:
						// addShutdownHandler;
						v_arg1 = v_valueStack[--v_valueStackSize];
						v_vm[10].push(v_arg1);
						break;
				}
				if ((v_row[1] == 1)) {
					if ((v_valueStackSize == v_valueStackCapacity)) {
						v_valueStack = v_valueStackIncreaseCapacity(v_ec);
						v_valueStackCapacity = v_valueStack.length;
					}
					v_valueStack[v_valueStackSize] = v_output;
					v_valueStackSize += 1;
				}
				break;
			case 20:
				// DEBUG_SYMBOLS;
				v_applyDebugSymbolData(v_vm, v_row, v_stringArgs[v_pc], v_metadata[22]);
				break;
			case 21:
				// DEF_DICT;
				v_intIntDict1 = {};
				v_stringIntDict1 = {};
				v_valueList2 = [];
				v_valueList1 = [];
				v_len = v_row[0];
				v_type = 3;
				v_first = true;
				v_i = v_len;
				while ((v_i > 0)) {
					v_valueStackSize -= 2;
					v_value = v_valueStack[(v_valueStackSize + 1)];
					v_value2 = v_valueStack[v_valueStackSize];
					if (v_first) {
						v_type = v_value2[0];
						v_first = false;
					} else {
						if ((v_type != v_value2[0])) {
							v_hasInterrupt = v_EX_InvalidKey(v_ec, "Dictionary keys must be of the same type.");
						}
					}
					if (!v_hasInterrupt) {
						if ((v_type == 3)) {
							v_intKey = v_value2[1];
						} else {
							if ((v_type == 5)) {
								v_stringKey = v_value2[1];
							} else {
								if ((v_type == 8)) {
									v_objInstance1 = v_value2[1];
									v_intKey = v_objInstance1[1];
								} else {
									v_hasInterrupt = v_EX_InvalidKey(v_ec, "Only integers, strings, and objects can be used as dictionary keys.");
								}
							}
						}
					}
					if (!v_hasInterrupt) {
						if ((v_type == 5)) {
							v_stringIntDict1[v_stringKey] = v_valueList1.length;
						} else {
							v_intIntDict1[v_intKey] = v_valueList1.length;
						}
						v_valueList2.push(v_value2);
						v_valueList1.push(v_value);
						v_i -= 1;
					}
				}
				if (!v_hasInterrupt) {
					if ((v_type == 5)) {
						v_i = Object.keys(v_stringIntDict1).length;
					} else {
						v_i = Object.keys(v_intIntDict1).length;
					}
					if ((v_i != v_len)) {
						v_hasInterrupt = v_EX_InvalidKey(v_ec, "Key collision");
					}
				}
				if (!v_hasInterrupt) {
					v_i = v_row[1];
					v_classId = 0;
					if ((v_i > 0)) {
						v_type = v_row[2];
						if ((v_type == 8)) {
							v_classId = v_row[3];
						}
						v_int1 = v_row.length;
						v_intArray1 = PST$createNewArray((v_int1 - v_i));
						while ((v_i < v_int1)) {
							v_intArray1[(v_i - v_row[1])] = v_row[v_i];
							v_i += 1;
						}
					} else {
						v_intArray1 = null;
					}
					if ((v_valueStackSize == v_valueStackCapacity)) {
						v_valueStack = v_valueStackIncreaseCapacity(v_ec);
						v_valueStackCapacity = v_valueStack.length;
					}
					v_valueStack[v_valueStackSize] = [7, [v_len, v_type, v_classId, v_intArray1, v_intIntDict1, v_stringIntDict1, v_valueList2, v_valueList1]];
					v_valueStackSize += 1;
				}
				break;
			case 22:
				// DEF_LIST;
				v_int1 = v_row[0];
				v_list1 = v_makeEmptyList(null, v_int1);
				if ((v_row[1] != 0)) {
					v_list1[0] = PST$createNewArray((v_row.length - 1));
					v_i = 1;
					while ((v_i < v_row.length)) {
						v_list1[0][(v_i - 1)] = v_row[v_i];
						v_i += 1;
					}
				}
				v_list1[1] = v_int1;
				while ((v_int1 > 0)) {
					v_valueStackSize -= 1;
					v_list1[2].push(v_valueStack[v_valueStackSize]);
					v_int1 -= 1;
				}
				v_list1[2].reverse();
				v_value = [6, v_list1];
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_valueStack[v_valueStackSize] = v_value;
				v_valueStackSize += 1;
				break;
			case 23:
				// DEF_ORIGINAL_CODE;
				v_defOriginalCodeImpl(v_vm, v_row, v_stringArgs[v_pc]);
				break;
			case 24:
				// DEREF_CLOSURE;
				v_bool1 = true;
				v_closure = v_stack[12];
				v_i = v_row[0];
				if (((v_closure != null) && (v_closure[v_i] !== undefined))) {
					v_value = v_closure[v_i][0];
					if ((v_value != null)) {
						v_bool1 = false;
						if ((v_valueStackSize == v_valueStackCapacity)) {
							v_valueStack = v_valueStackIncreaseCapacity(v_ec);
							v_valueStackCapacity = v_valueStack.length;
						}
						v_valueStack[v_valueStackSize++] = v_value;
					}
				}
				if (v_bool1) {
					v_hasInterrupt = v_EX_UnassignedVariable(v_ec, "Variable used before it was set.");
				}
				break;
			case 25:
				// DEREF_DOT;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				v_nameId = v_row[0];
				v_int2 = v_row[1];
				switch (v_value[0]) {
					case 8:
						v_objInstance1 = v_value[1];
						v_classId = v_objInstance1[0];
						v_classInfo = v_classTable[v_classId];
						if ((v_classId == v_row[4])) {
							v_int1 = v_row[5];
						} else {
							v_intIntDict1 = v_classInfo[14];
							v_int1 = v_intIntDict1[v_int2];
							if (v_int1 === undefined) v_int1 = -1;
							v_int3 = v_classInfo[12][v_int1];
							if ((v_int3 > 1)) {
								if ((v_int3 == 2)) {
									if ((v_classId != v_row[2])) {
										v_int1 = -2;
									}
								} else {
									if (((v_int3 == 3) || (v_int3 == 5))) {
										if ((v_classInfo[3] != v_row[3])) {
											v_int1 = -3;
										}
									}
									if (((v_int3 == 4) || (v_int3 == 5))) {
										v_i = v_row[2];
										if ((v_classId == v_i)) {
										} else {
											v_classInfo = v_classTable[v_classInfo[0]];
											while (((v_classInfo[2] != -1) && (v_int1 < v_classTable[v_classInfo[2]][12].length))) {
												v_classInfo = v_classTable[v_classInfo[2]];
											}
											v_j = v_classInfo[0];
											if ((v_j != v_i)) {
												v_bool1 = false;
												while (((v_i != -1) && (v_classTable[v_i][2] != -1))) {
													v_i = v_classTable[v_i][2];
													if ((v_i == v_j)) {
														v_bool1 = true;
														v_i = -1;
													}
												}
												if (!v_bool1) {
													v_int1 = -4;
												}
											}
										}
										v_classInfo = v_classTable[v_classId];
									}
								}
							}
							v_row[4] = v_objInstance1[0];
							v_row[5] = v_int1;
						}
						if ((v_int1 > -1)) {
							v_functionId = v_classInfo[9][v_int1];
							if ((v_functionId == -1)) {
								v_output = v_objInstance1[2][v_int1];
							} else {
								v_output = [9, [2, v_value, v_objInstance1[0], v_functionId, null]];
							}
						} else {
							v_output = null;
						}
						break;
					case 5:
						if ((v_metadata[14] == v_nameId)) {
							v_output = v_buildInteger(v_globals, (v_value[1]).length);
						} else {
							v_output = null;
						}
						break;
					case 6:
						if ((v_metadata[14] == v_nameId)) {
							v_output = v_buildInteger(v_globals, (v_value[1])[1]);
						} else {
							v_output = null;
						}
						break;
					case 7:
						if ((v_metadata[14] == v_nameId)) {
							v_output = v_buildInteger(v_globals, (v_value[1])[0]);
						} else {
							v_output = null;
						}
						break;
					default:
						if ((v_value[0] == 1)) {
							v_hasInterrupt = v_EX_NullReference(v_ec, "Derferenced a field from null.");
							v_output = v_VALUE_NULL;
						} else {
							v_output = null;
						}
						break;
				}
				if ((v_output == null)) {
					v_output = v_generatePrimitiveMethodReference(v_globalNameIdToPrimitiveMethodName, v_nameId, v_value);
					if ((v_output == null)) {
						if ((v_value[0] == 1)) {
							v_hasInterrupt = v_EX_NullReference(v_ec, "Tried to dereference a field on null.");
						} else {
							if (((v_value[0] == 8) && (v_int1 < -1))) {
								v_string1 = v_identifiers[v_row[0]];
								if ((v_int1 == -2)) {
									v_string2 = "private";
								} else {
									if ((v_int1 == -3)) {
										v_string2 = "internal";
									} else {
										v_string2 = "protected";
									}
								}
								v_hasInterrupt = v_EX_UnknownField(v_ec, ["The field '", v_string1, "' is marked as ", v_string2, " and cannot be accessed from here."].join(''));
							} else {
								if ((v_value[0] == 8)) {
									v_classId = (v_value[1])[0];
									v_classInfo = v_classTable[v_classId];
									v_string1 = v_classInfo[16] + " instance";
								} else {
									v_string1 = v_getTypeFromId(v_value[0]);
								}
								v_hasInterrupt = v_EX_UnknownField(v_ec, v_string1 + " does not have that field.");
							}
						}
					}
				}
				v_valueStack[(v_valueStackSize - 1)] = v_output;
				break;
			case 26:
				// DEREF_INSTANCE_FIELD;
				v_value = v_stack[6];
				v_objInstance1 = v_value[1];
				v_value = v_objInstance1[2][v_row[0]];
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_valueStack[v_valueStackSize++] = v_value;
				break;
			case 27:
				// DEREF_STATIC_FIELD;
				v_classInfo = v_classTable[v_row[0]];
				v_staticConstructorNotInvoked = true;
				if ((v_classInfo[4] < 2)) {
					v_stack[0] = v_pc;
					v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST$intBuffer16);
					if ((PST$intBuffer16[0] == 1)) {
						return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
					}
					if ((v_stackFrame2 != null)) {
						v_staticConstructorNotInvoked = false;
						v_stack = v_stackFrame2;
						v_pc = v_stack[0];
						v_localsStackSetToken = v_stack[1];
						v_localsStackOffset = v_stack[2];
					}
				}
				if (v_staticConstructorNotInvoked) {
					if ((v_valueStackSize == v_valueStackCapacity)) {
						v_valueStack = v_valueStackIncreaseCapacity(v_ec);
						v_valueStackCapacity = v_valueStack.length;
					}
					v_valueStack[v_valueStackSize++] = v_classInfo[5][v_row[1]];
				}
				break;
			case 28:
				// DUPLICATE_STACK_TOP;
				if ((v_row[0] == 1)) {
					v_value = v_valueStack[(v_valueStackSize - 1)];
					if ((v_valueStackSize == v_valueStackCapacity)) {
						v_valueStack = v_valueStackIncreaseCapacity(v_ec);
						v_valueStackCapacity = v_valueStack.length;
					}
					v_valueStack[v_valueStackSize++] = v_value;
				} else {
					if ((v_row[0] == 2)) {
						if (((v_valueStackSize + 1) > v_valueStackCapacity)) {
							v_valueStackIncreaseCapacity(v_ec);
							v_valueStack = v_ec[4];
							v_valueStackCapacity = v_valueStack.length;
						}
						v_valueStack[v_valueStackSize] = v_valueStack[(v_valueStackSize - 2)];
						v_valueStack[(v_valueStackSize + 1)] = v_valueStack[(v_valueStackSize - 1)];
						v_valueStackSize += 2;
					} else {
						v_hasInterrupt = v_EX_Fatal(v_ec, "?");
					}
				}
				break;
			case 29:
				// EQUALS;
				v_valueStackSize -= 2;
				v_rightValue = v_valueStack[(v_valueStackSize + 1)];
				v_leftValue = v_valueStack[v_valueStackSize];
				if ((v_leftValue[0] == v_rightValue[0])) {
					switch (v_leftValue[0]) {
						case 1:
							v_bool1 = true;
							break;
						case 2:
							v_bool1 = (v_leftValue[1] == v_rightValue[1]);
							break;
						case 3:
							v_bool1 = (v_leftValue[1] == v_rightValue[1]);
							break;
						case 5:
							v_bool1 = (v_leftValue[1] == v_rightValue[1]);
							break;
						default:
							v_bool1 = (v_doEqualityComparisonAndReturnCode(v_leftValue, v_rightValue) == 1);
							break;
					}
				} else {
					v_int1 = v_doEqualityComparisonAndReturnCode(v_leftValue, v_rightValue);
					if ((v_int1 == 0)) {
						v_bool1 = false;
					} else {
						if ((v_int1 == 1)) {
							v_bool1 = true;
						} else {
							v_hasInterrupt = v_EX_UnsupportedOperation(v_ec, "== and != not defined here.");
						}
					}
				}
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				if ((v_bool1 != ((v_row[0] == 1)))) {
					v_valueStack[v_valueStackSize] = v_VALUE_TRUE;
				} else {
					v_valueStack[v_valueStackSize] = v_VALUE_FALSE;
				}
				v_valueStackSize += 1;
				break;
			case 30:
				// ESF_LOOKUP;
				v_esfData = v_generateEsfData(v_args.length, v_row);
				v_metadata[18] = v_esfData;
				break;
			case 31:
				// EXCEPTION_HANDLED_TOGGLE;
				v_ec[9] = (v_row[0] == 1);
				break;
			case 32:
				// FIELD_TYPE_INFO;
				v_initializeClassFieldTypeInfo(v_vm, v_row);
				break;
			case 33:
				// FINALIZE_INITIALIZATION;
				v_finalizeInitializationImpl(v_vm, v_stringArgs[v_pc], v_row[0]);
				v_identifiers = v_vm[4][0];
				v_literalTable = v_vm[4][3];
				v_globalNameIdToPrimitiveMethodName = v_vm[4][12];
				v_funcArgs = v_vm[8];
				break;
			case 34:
				// FINALLY_END;
				v_value = v_ec[10];
				if (((v_value == null) || v_ec[9])) {
					switch (v_stack[10]) {
						case 0:
							v_ec[10] = null;
							break;
						case 1:
							v_ec[10] = null;
							v_int1 = v_row[0];
							if ((v_int1 == 1)) {
								v_pc += v_row[1];
							} else {
								if ((v_int1 == 2)) {
									v_intArray1 = v_esfData[v_pc];
									v_pc = v_intArray1[1];
								} else {
									v_hasInterrupt = v_EX_Fatal(v_ec, "break exists without a loop");
								}
							}
							break;
						case 2:
							v_ec[10] = null;
							v_int1 = v_row[2];
							if ((v_int1 == 1)) {
								v_pc += v_row[3];
							} else {
								if ((v_int1 == 2)) {
									v_intArray1 = v_esfData[v_pc];
									v_pc = v_intArray1[1];
								} else {
									v_hasInterrupt = v_EX_Fatal(v_ec, "continue exists without a loop");
								}
							}
							break;
						case 3:
							if ((v_stack[8] != 0)) {
								v_markClassAsInitialized(v_vm, v_stack, v_stack[8]);
							}
							if (v_stack[5]) {
								v_valueStackSize = v_stack[7];
								v_value = v_stack[11];
								v_stack = v_stack[4];
								if ((v_valueStackSize == v_valueStackCapacity)) {
									v_valueStack = v_valueStackIncreaseCapacity(v_ec);
									v_valueStackCapacity = v_valueStack.length;
								}
								v_valueStack[v_valueStackSize] = v_value;
								v_valueStackSize += 1;
							} else {
								v_valueStackSize = v_stack[7];
								v_stack = v_stack[4];
							}
							v_pc = v_stack[0];
							v_localsStackOffset = v_stack[2];
							v_localsStackSetToken = v_stack[1];
							break;
					}
				} else {
					v_ec[9] = false;
					v_stack[0] = v_pc;
					v_intArray1 = v_esfData[v_pc];
					v_value = v_ec[10];
					v_objInstance1 = v_value[1];
					v_objArray1 = v_objInstance1[3];
					v_bool1 = true;
					if ((v_objArray1[0] != null)) {
						v_bool1 = v_objArray1[0];
					}
					v_intList1 = v_objArray1[1];
					while (((v_stack != null) && ((v_intArray1 == null) || v_bool1))) {
						v_stack = v_stack[4];
						if ((v_stack != null)) {
							v_pc = v_stack[0];
							v_intList1.push(v_pc);
							v_intArray1 = v_esfData[v_pc];
						}
					}
					if ((v_stack == null)) {
						return v_uncaughtExceptionResult(v_vm, v_value);
					}
					v_int1 = v_intArray1[0];
					if ((v_int1 < v_pc)) {
						v_int1 = v_intArray1[1];
					}
					v_pc = (v_int1 - 1);
					v_stack[0] = v_pc;
					v_localsStackOffset = v_stack[2];
					v_localsStackSetToken = v_stack[1];
					v_ec[1] = v_stack;
					v_stack[10] = 0;
					v_ec[2] = v_valueStackSize;
					if ((false && (v_stack[13] != null))) {
						v_hasInterrupt = true;
						v_ec[13] = [5, 0, "", 0.0, v_stack[13]];
					}
				}
				break;
			case 35:
				// FUNCTION_DEFINITION;
				v_initializeFunction(v_vm, v_row, v_pc, v_stringArgs[v_pc]);
				v_pc += v_row[7];
				v_functionTable = v_metadata[10];
				break;
			case 36:
				// INDEX;
				v_value = v_valueStack[--v_valueStackSize];
				v_root = v_valueStack[(v_valueStackSize - 1)];
				if ((v_root[0] == 6)) {
					if ((v_value[0] != 3)) {
						v_hasInterrupt = v_EX_InvalidArgument(v_ec, "List index must be an integer.");
					} else {
						v_i = v_value[1];
						v_list1 = v_root[1];
						if ((v_i < 0)) {
							v_i += v_list1[1];
						}
						if (((v_i < 0) || (v_i >= v_list1[1]))) {
							v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "List index is out of bounds");
						} else {
							v_valueStack[(v_valueStackSize - 1)] = v_list1[2][v_i];
						}
					}
				} else {
					if ((v_root[0] == 7)) {
						v_dictImpl = v_root[1];
						v_keyType = v_value[0];
						if ((v_keyType != v_dictImpl[1])) {
							if ((v_dictImpl[0] == 0)) {
								v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found. Dictionary is empty.");
							} else {
								v_hasInterrupt = v_EX_InvalidKey(v_ec, ["Incorrect key type. This dictionary contains ", v_getTypeFromId(v_dictImpl[1]), " keys. Provided key is a ", v_getTypeFromId(v_keyType), "."].join(''));
							}
						} else {
							if ((v_keyType == 3)) {
								v_intKey = v_value[1];
							} else {
								if ((v_keyType == 5)) {
									v_stringKey = v_value[1];
								} else {
									if ((v_keyType == 8)) {
										v_intKey = (v_value[1])[1];
									} else {
										if ((v_dictImpl[0] == 0)) {
											v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found. Dictionary is empty.");
										} else {
											v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found.");
										}
									}
								}
							}
							if (!v_hasInterrupt) {
								if ((v_keyType == 5)) {
									v_stringIntDict1 = v_dictImpl[5];
									v_int1 = v_stringIntDict1[v_stringKey];
									if (v_int1 === undefined) v_int1 = -1;
									if ((v_int1 == -1)) {
										v_hasInterrupt = v_EX_KeyNotFound(v_ec, ["Key not found: '", v_stringKey, "'"].join(''));
									} else {
										v_valueStack[(v_valueStackSize - 1)] = v_dictImpl[7][v_int1];
									}
								} else {
									v_intIntDict1 = v_dictImpl[4];
									v_int1 = v_intIntDict1[v_intKey];
									if (v_int1 === undefined) v_int1 = -1;
									if ((v_int1 == -1)) {
										v_hasInterrupt = v_EX_KeyNotFound(v_ec, "Key not found.");
									} else {
										v_valueStack[(v_valueStackSize - 1)] = v_dictImpl[7][v_intIntDict1[v_intKey]];
									}
								}
							}
						}
					} else {
						if ((v_root[0] == 5)) {
							v_string1 = v_root[1];
							if ((v_value[0] != 3)) {
								v_hasInterrupt = v_EX_InvalidArgument(v_ec, "String indices must be integers.");
							} else {
								v_int1 = v_value[1];
								if ((v_int1 < 0)) {
									v_int1 += v_string1.length;
								}
								if (((v_int1 < 0) || (v_int1 >= v_string1.length))) {
									v_hasInterrupt = v_EX_IndexOutOfRange(v_ec, "String index out of range.");
								} else {
									v_valueStack[(v_valueStackSize - 1)] = v_buildCommonString(v_globals, v_string1.charAt(v_int1));
								}
							}
						} else {
							v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Cannot index into this type: " + v_getTypeFromId(v_root[0]));
						}
					}
				}
				break;
			case 37:
				// IS_COMPARISON;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				v_output = v_VALUE_FALSE;
				if ((v_value[0] == 8)) {
					v_objInstance1 = v_value[1];
					if (v_isClassASubclassOf(v_vm, v_objInstance1[0], v_row[0])) {
						v_output = v_VALUE_TRUE;
					}
				}
				v_valueStack[(v_valueStackSize - 1)] = v_output;
				break;
			case 38:
				// ITERATION_STEP;
				v_int1 = (v_localsStackOffset + v_row[2]);
				v_value3 = v_localsStack[v_int1];
				v_i = v_value3[1];
				v_value = v_localsStack[(v_localsStackOffset + v_row[3])];
				if ((v_value[0] == 6)) {
					v_list1 = v_value[1];
					v_len = v_list1[1];
					v_bool1 = true;
				} else {
					v_string2 = v_value[1];
					v_len = v_string2.length;
					v_bool1 = false;
				}
				if ((v_i < v_len)) {
					if (v_bool1) {
						v_value = v_list1[2][v_i];
					} else {
						v_value = v_buildCommonString(v_globals, v_string2.charAt(v_i));
					}
					v_int3 = (v_localsStackOffset + v_row[1]);
					v_localsStackSet[v_int3] = v_localsStackSetToken;
					v_localsStack[v_int3] = v_value;
				} else {
					v_pc += v_row[0];
				}
				v_i += 1;
				if ((v_i < 2049)) {
					v_localsStack[v_int1] = v_INTEGER_POSITIVE_CACHE[v_i];
				} else {
					v_localsStack[v_int1] = [3, v_i];
				}
				break;
			case 39:
				// JUMP;
				v_pc += v_row[0];
				break;
			case 40:
				// JUMP_IF_EXCEPTION_OF_TYPE;
				v_value = v_ec[10];
				v_objInstance1 = v_value[1];
				v_int1 = v_objInstance1[0];
				v_i = (v_row.length - 1);
				while ((v_i >= 2)) {
					if (v_isClassASubclassOf(v_vm, v_int1, v_row[v_i])) {
						v_i = 0;
						v_pc += v_row[0];
						v_int2 = v_row[1];
						if ((v_int2 > -1)) {
							v_int1 = (v_localsStackOffset + v_int2);
							v_localsStack[v_int1] = v_value;
							v_localsStackSet[v_int1] = v_localsStackSetToken;
						}
					}
					v_i -= 1;
				}
				break;
			case 41:
				// JUMP_IF_FALSE;
				v_value = v_valueStack[--v_valueStackSize];
				if ((v_value[0] != 2)) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
				} else {
					if (!v_value[1]) {
						v_pc += v_row[0];
					}
				}
				break;
			case 42:
				// JUMP_IF_FALSE_NON_POP;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				if ((v_value[0] != 2)) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
				} else {
					if (v_value[1]) {
						v_valueStackSize -= 1;
					} else {
						v_pc += v_row[0];
					}
				}
				break;
			case 43:
				// JUMP_IF_TRUE;
				v_value = v_valueStack[--v_valueStackSize];
				if ((v_value[0] != 2)) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
				} else {
					if (v_value[1]) {
						v_pc += v_row[0];
					}
				}
				break;
			case 44:
				// JUMP_IF_TRUE_NO_POP;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				if ((v_value[0] != 2)) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Boolean expected.");
				} else {
					if (v_value[1]) {
						v_pc += v_row[0];
					} else {
						v_valueStackSize -= 1;
					}
				}
				break;
			case 45:
				// LAMBDA;
				if (!(v_metadata[11][v_pc] !== undefined)) {
					v_int1 = (4 + v_row[4] + 1);
					v_len = v_row[v_int1];
					v_intArray1 = PST$createNewArray(v_len);
					v_i = 0;
					while ((v_i < v_len)) {
						v_intArray1[v_i] = v_row[(v_int1 + v_i + 1)];
						v_i += 1;
					}
					v_len = v_row[4];
					v_intArray2 = PST$createNewArray(v_len);
					v_i = 0;
					while ((v_i < v_len)) {
						v_intArray2[v_i] = v_row[(5 + v_i)];
						v_i += 1;
					}
					v_metadata[11][v_pc] = [v_pc, 0, v_pc, v_row[0], v_row[1], 5, 0, v_row[2], v_intArray2, "lambda", v_intArray1];
				}
				v_closure = {};
				v_parentClosure = v_stack[12];
				if ((v_parentClosure == null)) {
					v_parentClosure = {};
					v_stack[12] = v_parentClosure;
				}
				v_functionInfo = v_metadata[11][v_pc];
				v_intArray1 = v_functionInfo[10];
				v_len = v_intArray1.length;
				v_i = 0;
				while ((v_i < v_len)) {
					v_j = v_intArray1[v_i];
					if ((v_parentClosure[v_j] !== undefined)) {
						v_closure[v_j] = v_parentClosure[v_j];
					} else {
						v_closure[v_j] = [null];
						v_parentClosure[v_j] = v_closure[v_j];
					}
					v_i += 1;
				}
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_valueStack[v_valueStackSize] = [9, [5, null, 0, v_pc, v_closure]];
				v_valueStackSize += 1;
				v_pc += v_row[3];
				break;
			case 46:
				// LIB_DECLARATION;
				v_prepareToSuspend(v_ec, v_stack, v_valueStackSize, v_pc);
				v_ec[13] = [4, v_row[0], v_stringArgs[v_pc], 0.0, null];
				v_hasInterrupt = true;
				break;
			case 47:
				// LIST_SLICE;
				if ((v_row[2] == 1)) {
					v_valueStackSize -= 1;
					v_arg3 = v_valueStack[v_valueStackSize];
				} else {
					v_arg3 = null;
				}
				if ((v_row[1] == 1)) {
					v_valueStackSize -= 1;
					v_arg2 = v_valueStack[v_valueStackSize];
				} else {
					v_arg2 = null;
				}
				if ((v_row[0] == 1)) {
					v_valueStackSize -= 1;
					v_arg1 = v_valueStack[v_valueStackSize];
				} else {
					v_arg1 = null;
				}
				v_value = v_valueStack[(v_valueStackSize - 1)];
				v_value = v_performListSlice(v_globals, v_ec, v_value, v_arg1, v_arg2, v_arg3);
				v_hasInterrupt = (v_ec[13] != null);
				if (!v_hasInterrupt) {
					v_valueStack[(v_valueStackSize - 1)] = v_value;
				}
				break;
			case 48:
				// LITERAL;
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_valueStack[v_valueStackSize++] = v_literalTable[v_row[0]];
				break;
			case 49:
				// LITERAL_STREAM;
				v_int1 = v_row.length;
				if (((v_valueStackSize + v_int1) > v_valueStackCapacity)) {
					while (((v_valueStackSize + v_int1) > v_valueStackCapacity)) {
						v_valueStackIncreaseCapacity(v_ec);
						v_valueStack = v_ec[4];
						v_valueStackCapacity = v_valueStack.length;
					}
				}
				v_i = v_int1;
				while ((--v_i >= 0)) {
					v_valueStack[v_valueStackSize++] = v_literalTable[v_row[v_i]];
				}
				break;
			case 50:
				// LOCAL;
				v_int1 = (v_localsStackOffset + v_row[0]);
				if ((v_localsStackSet[v_int1] == v_localsStackSetToken)) {
					if ((v_valueStackSize == v_valueStackCapacity)) {
						v_valueStack = v_valueStackIncreaseCapacity(v_ec);
						v_valueStackCapacity = v_valueStack.length;
					}
					v_valueStack[v_valueStackSize++] = v_localsStack[v_int1];
				} else {
					v_hasInterrupt = v_EX_UnassignedVariable(v_ec, "Variable used before it was set.");
				}
				break;
			case 51:
				// LOC_TABLE;
				v_initLocTable(v_vm, v_row);
				break;
			case 52:
				// NEGATIVE_SIGN;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				v_type = v_value[0];
				if ((v_type == 3)) {
					v_valueStack[(v_valueStackSize - 1)] = v_buildInteger(v_globals, -v_value[1]);
				} else {
					if ((v_type == 4)) {
						v_valueStack[(v_valueStackSize - 1)] = v_buildFloat(v_globals, -v_value[1]);
					} else {
						v_hasInterrupt = v_EX_InvalidArgument(v_ec, ["Negative sign can only be applied to numbers. Found ", v_getTypeFromId(v_type), " instead."].join(''));
					}
				}
				break;
			case 53:
				// POP;
				v_valueStackSize -= 1;
				break;
			case 54:
				// POP_IF_NULL_OR_JUMP;
				v_value = v_valueStack[(v_valueStackSize - 1)];
				if ((v_value[0] == 1)) {
					v_valueStackSize -= 1;
				} else {
					v_pc += v_row[0];
				}
				break;
			case 55:
				// PUSH_FUNC_REF;
				v_value = null;
				switch (v_row[1]) {
					case 0:
						v_value = [9, [1, null, 0, v_row[0], null]];
						break;
					case 1:
						v_value = [9, [2, v_stack[6], v_row[2], v_row[0], null]];
						break;
					case 2:
						v_classId = v_row[2];
						v_classInfo = v_classTable[v_classId];
						v_staticConstructorNotInvoked = true;
						if ((v_classInfo[4] < 2)) {
							v_stack[0] = v_pc;
							v_stackFrame2 = v_maybeInvokeStaticConstructor(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, PST$intBuffer16);
							if ((PST$intBuffer16[0] == 1)) {
								return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Static initialization loop detected. The class this field is a member of is not done being initialized.");
							}
							if ((v_stackFrame2 != null)) {
								v_staticConstructorNotInvoked = false;
								v_stack = v_stackFrame2;
								v_pc = v_stack[0];
								v_localsStackSetToken = v_stack[1];
								v_localsStackOffset = v_stack[2];
							}
						}
						if (v_staticConstructorNotInvoked) {
							v_value = [9, [3, null, v_classId, v_row[0], null]];
						} else {
							v_value = null;
						}
						break;
				}
				if ((v_value != null)) {
					if ((v_valueStackSize == v_valueStackCapacity)) {
						v_valueStack = v_valueStackIncreaseCapacity(v_ec);
						v_valueStackCapacity = v_valueStack.length;
					}
					v_valueStack[v_valueStackSize] = v_value;
					v_valueStackSize += 1;
				}
				break;
			case 56:
				// RETURN;
				if ((v_esfData[v_pc] != null)) {
					v_intArray1 = v_esfData[v_pc];
					v_pc = (v_intArray1[1] - 1);
					if ((v_row[0] == 0)) {
						v_stack[11] = v_VALUE_NULL;
					} else {
						v_stack[11] = v_valueStack[(v_valueStackSize - 1)];
					}
					v_valueStackSize = v_stack[7];
					v_stack[10] = 3;
				} else {
					if ((v_stack[4] == null)) {
						return v_interpreterFinished(v_vm, v_ec);
					}
					if ((v_stack[8] != 0)) {
						v_markClassAsInitialized(v_vm, v_stack, v_stack[8]);
					}
					if (v_stack[5]) {
						if ((v_row[0] == 0)) {
							v_valueStackSize = v_stack[7];
							v_stack = v_stack[4];
							if ((v_valueStackSize == v_valueStackCapacity)) {
								v_valueStack = v_valueStackIncreaseCapacity(v_ec);
								v_valueStackCapacity = v_valueStack.length;
							}
							v_valueStack[v_valueStackSize] = v_VALUE_NULL;
						} else {
							v_value = v_valueStack[(v_valueStackSize - 1)];
							v_valueStackSize = v_stack[7];
							v_stack = v_stack[4];
							v_valueStack[v_valueStackSize] = v_value;
						}
						v_valueStackSize += 1;
					} else {
						v_valueStackSize = v_stack[7];
						v_stack = v_stack[4];
					}
					v_pc = v_stack[0];
					v_localsStackOffset = v_stack[2];
					v_localsStackSetToken = v_stack[1];
					if ((false && (v_stack[13] != null))) {
						v_hasInterrupt = true;
						v_ec[13] = [5, 0, "", 0.0, v_stack[13]];
					}
				}
				break;
			case 57:
				// STACK_INSERTION_FOR_INCREMENT;
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_valueStack[v_valueStackSize] = v_valueStack[(v_valueStackSize - 1)];
				v_valueStack[(v_valueStackSize - 1)] = v_valueStack[(v_valueStackSize - 2)];
				v_valueStack[(v_valueStackSize - 2)] = v_valueStack[(v_valueStackSize - 3)];
				v_valueStack[(v_valueStackSize - 3)] = v_valueStack[v_valueStackSize];
				v_valueStackSize += 1;
				break;
			case 58:
				// STACK_SWAP_POP;
				v_valueStackSize -= 1;
				v_valueStack[(v_valueStackSize - 1)] = v_valueStack[v_valueStackSize];
				break;
			case 59:
				// SWITCH_INT;
				v_value = v_valueStack[--v_valueStackSize];
				if ((v_value[0] == 3)) {
					v_intKey = v_value[1];
					v_integerSwitch = v_integerSwitchesByPc[v_pc];
					if ((v_integerSwitch == null)) {
						v_integerSwitch = v_initializeIntSwitchStatement(v_vm, v_pc, v_row);
					}
					v_i = v_integerSwitch[v_intKey];
					if (v_i === undefined) v_i = -1;
					if ((v_i == -1)) {
						v_pc += v_row[0];
					} else {
						v_pc += v_i;
					}
				} else {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Switch statement expects an integer.");
				}
				break;
			case 60:
				// SWITCH_STRING;
				v_value = v_valueStack[--v_valueStackSize];
				if ((v_value[0] == 5)) {
					v_stringKey = v_value[1];
					v_stringSwitch = v_stringSwitchesByPc[v_pc];
					if ((v_stringSwitch == null)) {
						v_stringSwitch = v_initializeStringSwitchStatement(v_vm, v_pc, v_row);
					}
					v_i = v_stringSwitch[v_stringKey];
					if (v_i === undefined) v_i = -1;
					if ((v_i == -1)) {
						v_pc += v_row[0];
					} else {
						v_pc += v_i;
					}
				} else {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Switch statement expects a string.");
				}
				break;
			case 61:
				// THIS;
				if ((v_valueStackSize == v_valueStackCapacity)) {
					v_valueStack = v_valueStackIncreaseCapacity(v_ec);
					v_valueStackCapacity = v_valueStack.length;
				}
				v_valueStack[v_valueStackSize] = v_stack[6];
				v_valueStackSize += 1;
				break;
			case 62:
				// THROW;
				v_valueStackSize -= 1;
				v_value = v_valueStack[v_valueStackSize];
				v_bool2 = (v_value[0] == 8);
				if (v_bool2) {
					v_objInstance1 = v_value[1];
					if (!v_isClassASubclassOf(v_vm, v_objInstance1[0], v_magicNumbers[0])) {
						v_bool2 = false;
					}
				}
				if (v_bool2) {
					v_objArray1 = v_objInstance1[3];
					v_intList1 = [];
					v_objArray1[1] = v_intList1;
					if (!v_isPcFromCore(v_vm, v_pc)) {
						v_intList1.push(v_pc);
					}
					v_ec[10] = v_value;
					v_ec[9] = false;
					v_stack[0] = v_pc;
					v_intArray1 = v_esfData[v_pc];
					v_value = v_ec[10];
					v_objInstance1 = v_value[1];
					v_objArray1 = v_objInstance1[3];
					v_bool1 = true;
					if ((v_objArray1[0] != null)) {
						v_bool1 = v_objArray1[0];
					}
					v_intList1 = v_objArray1[1];
					while (((v_stack != null) && ((v_intArray1 == null) || v_bool1))) {
						v_stack = v_stack[4];
						if ((v_stack != null)) {
							v_pc = v_stack[0];
							v_intList1.push(v_pc);
							v_intArray1 = v_esfData[v_pc];
						}
					}
					if ((v_stack == null)) {
						return v_uncaughtExceptionResult(v_vm, v_value);
					}
					v_int1 = v_intArray1[0];
					if ((v_int1 < v_pc)) {
						v_int1 = v_intArray1[1];
					}
					v_pc = (v_int1 - 1);
					v_stack[0] = v_pc;
					v_localsStackOffset = v_stack[2];
					v_localsStackSetToken = v_stack[1];
					v_ec[1] = v_stack;
					v_stack[10] = 0;
					v_ec[2] = v_valueStackSize;
					if ((false && (v_stack[13] != null))) {
						v_hasInterrupt = true;
						v_ec[13] = [5, 0, "", 0.0, v_stack[13]];
					}
				} else {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, "Thrown value is not an exception.");
				}
				break;
			case 63:
				// TOKEN_DATA;
				v_tokenDataImpl(v_vm, v_row);
				break;
			case 64:
				// USER_CODE_START;
				v_metadata[16] = v_row[0];
				break;
			case 65:
				// VERIFY_TYPE_IS_ITERABLE;
				v_value = v_valueStack[--v_valueStackSize];
				v_i = (v_localsStackOffset + v_row[0]);
				v_localsStack[v_i] = v_value;
				v_localsStackSet[v_i] = v_localsStackSetToken;
				v_int1 = v_value[0];
				if (((v_int1 != 6) && (v_int1 != 5))) {
					v_hasInterrupt = v_EX_InvalidArgument(v_ec, ["Expected an iterable type, such as a list or string. Found ", v_getTypeFromId(v_int1), " instead."].join(''));
				}
				v_i = (v_localsStackOffset + v_row[1]);
				v_localsStack[v_i] = v_VALUE_INT_ZERO;
				v_localsStackSet[v_i] = v_localsStackSetToken;
				break;
			default:
				// THIS SHOULD NEVER HAPPEN;
				return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, 0, "Bad op code: " + ('' + v_ops[v_pc]));
		}
		if (v_hasInterrupt) {
			var v_interrupt = v_ec[13];
			v_ec[13] = null;
			if ((v_interrupt[0] == 1)) {
				return v_generateException(v_vm, v_stack, v_pc, v_valueStackSize, v_ec, v_interrupt[1], v_interrupt[2]);
			}
			if ((v_interrupt[0] == 3)) {
				return [5, "", v_interrupt[3], 0, false, ""];
			}
			if ((v_interrupt[0] == 4)) {
				return [6, "", 0.0, 0, false, v_interrupt[2]];
			}
		}
		++v_pc;
	}
};

var v_isClassASubclassOf = function(v_vm, v_subClassId, v_parentClassId) {
	if ((v_subClassId == v_parentClassId)) {
		return true;
	}
	var v_classTable = v_vm[4][9];
	var v_classIdWalker = v_subClassId;
	while ((v_classIdWalker != -1)) {
		if ((v_classIdWalker == v_parentClassId)) {
			return true;
		}
		var v_classInfo = v_classTable[v_classIdWalker];
		v_classIdWalker = v_classInfo[2];
	}
	return false;
};

var v_isPcFromCore = function(v_vm, v_pc) {
	if ((v_vm[3] == null)) {
		return false;
	}
	var v_tokens = v_vm[3][0][v_pc];
	if ((v_tokens == null)) {
		return false;
	}
	var v_token = v_tokens[0];
	var v_filename = v_tokenHelperGetFileLine(v_vm, v_token[2], 0);
	return "[Core]" == v_filename;
};

var v_isStringEqual = function(v_a, v_b) {
	if ((v_a == v_b)) {
		return true;
	}
	return false;
};

var v_isVmResultRootExecContext = function(v_result) {
	return v_result[4];
};

var v_makeEmptyList = function(v_type, v_capacity) {
	return [v_type, 0, []];
};

var v_markClassAsInitialized = function(v_vm, v_stack, v_classId) {
	var v_classInfo = v_vm[4][9][v_stack[8]];
	v_classInfo[4] = 2;
	v_vm[7].pop();
	return 0;
};

var v_maybeInvokeStaticConstructor = function(v_vm, v_ec, v_stack, v_classInfo, v_valueStackSize, v_intOutParam) {
	PST$intBuffer16[0] = 0;
	var v_classId = v_classInfo[0];
	if ((v_classInfo[4] == 1)) {
		var v_classIdsBeingInitialized = v_vm[7];
		if ((v_classIdsBeingInitialized[(v_classIdsBeingInitialized.length - 1)] != v_classId)) {
			PST$intBuffer16[0] = 1;
		}
		return null;
	}
	v_classInfo[4] = 1;
	v_vm[7].push(v_classId);
	var v_functionInfo = v_vm[4][10][v_classInfo[6]];
	v_stack[0] -= 1;
	var v_newFrameLocalsSize = v_functionInfo[7];
	var v_currentFrameLocalsEnd = v_stack[3];
	if ((v_ec[5].length <= (v_currentFrameLocalsEnd + v_newFrameLocalsSize))) {
		v_increaseLocalsStackCapacity(v_ec, v_newFrameLocalsSize);
	}
	if ((v_ec[7] > 2000000000)) {
		v_resetLocalsStackTokens(v_ec, v_stack);
	}
	v_ec[7] += 1;
	return [v_functionInfo[2], v_ec[7], v_currentFrameLocalsEnd, (v_currentFrameLocalsEnd + v_newFrameLocalsSize), v_stack, false, null, v_valueStackSize, v_classId, (v_stack[9] + 1), 0, null, null, null];
};

var v_multiplyString = function(v_globals, v_strValue, v_str, v_n) {
	if ((v_n <= 2)) {
		if ((v_n == 1)) {
			return v_strValue;
		}
		if ((v_n == 2)) {
			return v_buildString(v_globals, v_str + v_str);
		}
		return v_globals[8];
	}
	var v_builder = [];
	while ((v_n > 0)) {
		v_n -= 1;
		v_builder.push(v_str);
	}
	v_str = v_builder.join("");
	return v_buildString(v_globals, v_str);
};

var v_nextPowerOf2 = function(v_value) {
	if ((((v_value - 1) & v_value) == 0)) {
		return v_value;
	}
	var v_output = 1;
	while ((v_output < v_value)) {
		v_output *= 2;
	}
	return v_output;
};

var v_noop = function() {
	return 0;
};

var v_performListSlice = function(v_globals, v_ec, v_value, v_arg1, v_arg2, v_arg3) {
	var v_begin = 0;
	var v_end = 0;
	var v_step = 0;
	var v_length = 0;
	var v_i = 0;
	var v_isForward = false;
	var v_isString = false;
	var v_originalString = "";
	var v_originalList = null;
	var v_outputList = null;
	var v_outputString = null;
	var v_status = 0;
	if ((v_arg3 != null)) {
		if ((v_arg3[0] == 3)) {
			v_step = v_arg3[1];
			if ((v_step == 0)) {
				v_status = 2;
			}
		} else {
			v_status = 3;
			v_step = 1;
		}
	} else {
		v_step = 1;
	}
	v_isForward = (v_step > 0);
	if ((v_arg2 != null)) {
		if ((v_arg2[0] == 3)) {
			v_end = v_arg2[1];
		} else {
			v_status = 3;
		}
	}
	if ((v_arg1 != null)) {
		if ((v_arg1[0] == 3)) {
			v_begin = v_arg1[1];
		} else {
			v_status = 3;
		}
	}
	if ((v_value[0] == 5)) {
		v_isString = true;
		v_originalString = v_value[1];
		v_length = v_originalString.length;
	} else {
		if ((v_value[0] == 6)) {
			v_isString = false;
			v_originalList = v_value[1];
			v_length = v_originalList[1];
		} else {
			v_EX_InvalidArgument(v_ec, ["Cannot apply slicing to ", v_getTypeFromId(v_value[0]), ". Must be string or list."].join(''));
			return v_globals[0];
		}
	}
	if ((v_status >= 2)) {
		var v_msg = null;
		if (v_isString) {
			v_msg = "String";
		} else {
			v_msg = "List";
		}
		if ((v_status == 3)) {
			v_msg += " slice indexes must be integers. Found ";
			if (((v_arg1 != null) && (v_arg1[0] != 3))) {
				v_EX_InvalidArgument(v_ec, [v_msg, v_getTypeFromId(v_arg1[0]), " for begin index."].join(''));
				return v_globals[0];
			}
			if (((v_arg2 != null) && (v_arg2[0] != 3))) {
				v_EX_InvalidArgument(v_ec, [v_msg, v_getTypeFromId(v_arg2[0]), " for end index."].join(''));
				return v_globals[0];
			}
			if (((v_arg3 != null) && (v_arg3[0] != 3))) {
				v_EX_InvalidArgument(v_ec, [v_msg, v_getTypeFromId(v_arg3[0]), " for step amount."].join(''));
				return v_globals[0];
			}
			v_EX_InvalidArgument(v_ec, "Invalid slice arguments.");
			return v_globals[0];
		} else {
			v_EX_InvalidArgument(v_ec, v_msg + " slice step cannot be 0.");
			return v_globals[0];
		}
	}
	v_status = v_canonicalizeListSliceArgs(PST$intBuffer16, v_arg1, v_arg2, v_begin, v_end, v_step, v_length, v_isForward);
	if ((v_status == 1)) {
		v_begin = PST$intBuffer16[0];
		v_end = PST$intBuffer16[1];
		if (v_isString) {
			v_outputString = [];
			if (v_isForward) {
				if ((v_step == 1)) {
					return v_buildString(v_globals, v_originalString.substring(v_begin, v_begin + (v_end - v_begin)));
				} else {
					while ((v_begin < v_end)) {
						v_outputString.push(v_originalString.charAt(v_begin));
						v_begin += v_step;
					}
				}
			} else {
				while ((v_begin > v_end)) {
					v_outputString.push(v_originalString.charAt(v_begin));
					v_begin += v_step;
				}
			}
			v_value = v_buildString(v_globals, v_outputString.join(""));
		} else {
			v_outputList = v_makeEmptyList(v_originalList[0], 10);
			if (v_isForward) {
				while ((v_begin < v_end)) {
					v_addToList(v_outputList, v_originalList[2][v_begin]);
					v_begin += v_step;
				}
			} else {
				while ((v_begin > v_end)) {
					v_addToList(v_outputList, v_originalList[2][v_begin]);
					v_begin += v_step;
				}
			}
			v_value = [6, v_outputList];
		}
	} else {
		if ((v_status == 0)) {
			if (v_isString) {
				v_value = v_globals[8];
			} else {
				v_value = [6, v_makeEmptyList(v_originalList[0], 0)];
			}
		} else {
			if ((v_status == 2)) {
				if (!v_isString) {
					v_outputList = v_makeEmptyList(v_originalList[0], v_length);
					v_i = 0;
					while ((v_i < v_length)) {
						v_addToList(v_outputList, v_originalList[2][v_i]);
						v_i += 1;
					}
					v_value = [6, v_outputList];
				}
			} else {
				var v_msg = null;
				if (v_isString) {
					v_msg = "String";
				} else {
					v_msg = "List";
				}
				if ((v_status == 3)) {
					v_msg += " slice begin index is out of range.";
				} else {
					if (v_isForward) {
						v_msg += " slice begin index must occur before the end index when step is positive.";
					} else {
						v_msg += " slice begin index must occur after the end index when the step is negative.";
					}
				}
				v_EX_IndexOutOfRange(v_ec, v_msg);
				return v_globals[0];
			}
		}
	}
	return v_value;
};

var v_prepareToSuspend = function(v_ec, v_stack, v_valueStackSize, v_currentPc) {
	v_ec[1] = v_stack;
	v_ec[2] = v_valueStackSize;
	v_stack[0] = (v_currentPc + 1);
	return 0;
};

var v_primitiveMethodsInitializeLookup = function(v_nameLookups) {
	var v_length = Object.keys(v_nameLookups).length;
	var v_lookup = PST$createNewArray(v_length);
	var v_i = 0;
	while ((v_i < v_length)) {
		v_lookup[v_i] = -1;
		v_i += 1;
	}
	if ((v_nameLookups["add"] !== undefined)) {
		v_lookup[v_nameLookups["add"]] = 0;
	}
	if ((v_nameLookups["argCountMax"] !== undefined)) {
		v_lookup[v_nameLookups["argCountMax"]] = 1;
	}
	if ((v_nameLookups["argCountMin"] !== undefined)) {
		v_lookup[v_nameLookups["argCountMin"]] = 2;
	}
	if ((v_nameLookups["choice"] !== undefined)) {
		v_lookup[v_nameLookups["choice"]] = 3;
	}
	if ((v_nameLookups["clear"] !== undefined)) {
		v_lookup[v_nameLookups["clear"]] = 4;
	}
	if ((v_nameLookups["clone"] !== undefined)) {
		v_lookup[v_nameLookups["clone"]] = 5;
	}
	if ((v_nameLookups["concat"] !== undefined)) {
		v_lookup[v_nameLookups["concat"]] = 6;
	}
	if ((v_nameLookups["contains"] !== undefined)) {
		v_lookup[v_nameLookups["contains"]] = 7;
	}
	if ((v_nameLookups["createInstance"] !== undefined)) {
		v_lookup[v_nameLookups["createInstance"]] = 8;
	}
	if ((v_nameLookups["endsWith"] !== undefined)) {
		v_lookup[v_nameLookups["endsWith"]] = 9;
	}
	if ((v_nameLookups["filter"] !== undefined)) {
		v_lookup[v_nameLookups["filter"]] = 10;
	}
	if ((v_nameLookups["get"] !== undefined)) {
		v_lookup[v_nameLookups["get"]] = 11;
	}
	if ((v_nameLookups["getName"] !== undefined)) {
		v_lookup[v_nameLookups["getName"]] = 12;
	}
	if ((v_nameLookups["indexOf"] !== undefined)) {
		v_lookup[v_nameLookups["indexOf"]] = 13;
	}
	if ((v_nameLookups["insert"] !== undefined)) {
		v_lookup[v_nameLookups["insert"]] = 14;
	}
	if ((v_nameLookups["invoke"] !== undefined)) {
		v_lookup[v_nameLookups["invoke"]] = 15;
	}
	if ((v_nameLookups["isA"] !== undefined)) {
		v_lookup[v_nameLookups["isA"]] = 16;
	}
	if ((v_nameLookups["join"] !== undefined)) {
		v_lookup[v_nameLookups["join"]] = 17;
	}
	if ((v_nameLookups["keys"] !== undefined)) {
		v_lookup[v_nameLookups["keys"]] = 18;
	}
	if ((v_nameLookups["lower"] !== undefined)) {
		v_lookup[v_nameLookups["lower"]] = 19;
	}
	if ((v_nameLookups["ltrim"] !== undefined)) {
		v_lookup[v_nameLookups["ltrim"]] = 20;
	}
	if ((v_nameLookups["map"] !== undefined)) {
		v_lookup[v_nameLookups["map"]] = 21;
	}
	if ((v_nameLookups["merge"] !== undefined)) {
		v_lookup[v_nameLookups["merge"]] = 22;
	}
	if ((v_nameLookups["pop"] !== undefined)) {
		v_lookup[v_nameLookups["pop"]] = 23;
	}
	if ((v_nameLookups["remove"] !== undefined)) {
		v_lookup[v_nameLookups["remove"]] = 24;
	}
	if ((v_nameLookups["replace"] !== undefined)) {
		v_lookup[v_nameLookups["replace"]] = 25;
	}
	if ((v_nameLookups["reverse"] !== undefined)) {
		v_lookup[v_nameLookups["reverse"]] = 26;
	}
	if ((v_nameLookups["rtrim"] !== undefined)) {
		v_lookup[v_nameLookups["rtrim"]] = 27;
	}
	if ((v_nameLookups["shuffle"] !== undefined)) {
		v_lookup[v_nameLookups["shuffle"]] = 28;
	}
	if ((v_nameLookups["sort"] !== undefined)) {
		v_lookup[v_nameLookups["sort"]] = 29;
	}
	if ((v_nameLookups["split"] !== undefined)) {
		v_lookup[v_nameLookups["split"]] = 30;
	}
	if ((v_nameLookups["startsWith"] !== undefined)) {
		v_lookup[v_nameLookups["startsWith"]] = 31;
	}
	if ((v_nameLookups["trim"] !== undefined)) {
		v_lookup[v_nameLookups["trim"]] = 32;
	}
	if ((v_nameLookups["upper"] !== undefined)) {
		v_lookup[v_nameLookups["upper"]] = 33;
	}
	if ((v_nameLookups["values"] !== undefined)) {
		v_lookup[v_nameLookups["values"]] = 34;
	}
	return v_lookup;
};

var v_primitiveMethodWrongArgCountError = function(v_name, v_expected, v_actual) {
	var v_output = "";
	if ((v_expected == 0)) {
		v_output = v_name + " does not accept any arguments.";
	} else {
		if ((v_expected == 1)) {
			v_output = v_name + " accepts exactly 1 argument.";
		} else {
			v_output = [v_name, " requires ", ('' + v_expected), " arguments."].join('');
		}
	}
	return [v_output, " Found: ", ('' + v_actual)].join('');
};

var v_printToStdOut = function(v_prefix, v_line) {
	if ((v_prefix == null)) {
		C$common$print(v_line);
	} else {
		var v_canonical = v_line.split("\r\n").join("\n").split("\r").join("\n");
		var v_lines = v_canonical.split("\n");
		var v_i = 0;
		while ((v_i < v_lines.length)) {
			C$common$print([v_prefix, ": ", v_lines[v_i]].join(''));
			v_i += 1;
		}
	}
	return 0;
};

var v_qsortHelper = function(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, v_endIndex) {
	if (((v_endIndex - v_startIndex) <= 0)) {
		return 0;
	}
	if (((v_endIndex - v_startIndex) == 1)) {
		if (v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_startIndex, v_endIndex)) {
			v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, v_endIndex);
		}
		return 0;
	}
	var v_mid = ((v_endIndex + v_startIndex) >> 1);
	v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_mid, v_startIndex);
	var v_upperPointer = (v_endIndex + 1);
	var v_lowerPointer = (v_startIndex + 1);
	while ((v_upperPointer > v_lowerPointer)) {
		if (v_sortHelperIsRevOrder(v_keyStringList, v_keyNumList, v_isString, v_startIndex, v_lowerPointer)) {
			v_lowerPointer += 1;
		} else {
			v_upperPointer -= 1;
			v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_lowerPointer, v_upperPointer);
		}
	}
	var v_midIndex = (v_lowerPointer - 1);
	v_sortHelperSwap(v_keyStringList, v_keyNumList, v_indices, v_isString, v_midIndex, v_startIndex);
	v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, v_startIndex, (v_midIndex - 1));
	v_qsortHelper(v_keyStringList, v_keyNumList, v_indices, v_isString, (v_midIndex + 1), v_endIndex);
	return 0;
};

var v_queryValue = function(v_vm, v_execId, v_stackFrameOffset, v_steps) {
	if ((v_execId == -1)) {
		v_execId = v_vm[1];
	}
	var v_ec = v_vm[0][v_execId];
	var v_stackFrame = v_ec[1];
	while ((v_stackFrameOffset > 0)) {
		v_stackFrameOffset -= 1;
		v_stackFrame = v_stackFrame[4];
	}
	var v_current = null;
	var v_i = 0;
	var v_j = 0;
	var v_len = v_steps.length;
	v_i = 0;
	while ((v_i < v_steps.length)) {
		if (((v_current == null) && (v_i > 0))) {
			return null;
		}
		var v_step = v_steps[v_i];
		if (v_isStringEqual(".", v_step)) {
			return null;
		} else {
			if (v_isStringEqual("this", v_step)) {
				v_current = v_stackFrame[6];
			} else {
				if (v_isStringEqual("class", v_step)) {
					return null;
				} else {
					if (v_isStringEqual("local", v_step)) {
						v_i += 1;
						v_step = v_steps[v_i];
						var v_localNamesByFuncPc = v_vm[3][5];
						var v_localNames = null;
						if (((v_localNamesByFuncPc == null) || (Object.keys(v_localNamesByFuncPc).length == 0))) {
							return null;
						}
						v_j = v_stackFrame[0];
						while ((v_j >= 0)) {
							if ((v_localNamesByFuncPc[v_j] !== undefined)) {
								v_localNames = v_localNamesByFuncPc[v_j];
								v_j = -1;
							}
							v_j -= 1;
						}
						if ((v_localNames == null)) {
							return null;
						}
						var v_localId = -1;
						if ((v_localNames != null)) {
							v_j = 0;
							while ((v_j < v_localNames.length)) {
								if (v_isStringEqual(v_localNames[v_j], v_step)) {
									v_localId = v_j;
									v_j = v_localNames.length;
								}
								v_j += 1;
							}
						}
						if ((v_localId == -1)) {
							return null;
						}
						var v_localOffset = (v_localId + v_stackFrame[2]);
						if ((v_ec[6][v_localOffset] != v_stackFrame[1])) {
							return null;
						}
						v_current = v_ec[5][v_localOffset];
					} else {
						if (v_isStringEqual("index", v_step)) {
							return null;
						} else {
							if (v_isStringEqual("key-int", v_step)) {
								return null;
							} else {
								if (v_isStringEqual("key-str", v_step)) {
									return null;
								} else {
									if (v_isStringEqual("key-obj", v_step)) {
										return null;
									} else {
										return null;
									}
								}
							}
						}
					}
				}
			}
		}
		v_i += 1;
	}
	return v_current;
};

var v_read_integer = function(v_pindex, v_raw, v_length, v_alphaNums) {
	var v_num = 0;
	var v_c = v_raw.charAt(v_pindex[0]);
	v_pindex[0] = (v_pindex[0] + 1);
	if ((v_c == "%")) {
		var v_value = v_read_till(v_pindex, v_raw, v_length, "%");
		v_num = parseInt(v_value);
	} else {
		if ((v_c == "@")) {
			v_num = v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
			v_num *= 62;
			v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
		} else {
			if ((v_c == "#")) {
				v_num = v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
				v_num *= 62;
				v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
				v_num *= 62;
				v_num += v_read_integer(v_pindex, v_raw, v_length, v_alphaNums);
			} else {
				if ((v_c == "^")) {
					v_num = (-1 * v_read_integer(v_pindex, v_raw, v_length, v_alphaNums));
				} else {
					// TODO: string.IndexOfChar(c);
					v_num = v_alphaNums.indexOf(v_c);
					if ((v_num == -1)) {
					}
				}
			}
		}
	}
	return v_num;
};

var v_read_string = function(v_pindex, v_raw, v_length, v_alphaNums) {
	var v_b64 = v_read_till(v_pindex, v_raw, v_length, "%");
	return decodeURIComponent(Array.prototype.map.call(atob(v_b64), function(c) { return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2); }).join(''));
};

var v_read_till = function(v_index, v_raw, v_length, v_end) {
	var v_output = [];
	var v_ctn = true;
	var v_c = " ";
	while (v_ctn) {
		v_c = v_raw.charAt(v_index[0]);
		if ((v_c == v_end)) {
			v_ctn = false;
		} else {
			v_output.push(v_c);
		}
		v_index[0] = (v_index[0] + 1);
	}
	return v_output.join('');
};

var v_reallocIntArray = function(v_original, v_requiredCapacity) {
	var v_oldSize = v_original.length;
	var v_size = v_oldSize;
	while ((v_size < v_requiredCapacity)) {
		v_size *= 2;
	}
	var v_output = PST$createNewArray(v_size);
	var v_i = 0;
	while ((v_i < v_oldSize)) {
		v_output[v_i] = v_original[v_i];
		v_i += 1;
	}
	return v_output;
};

var v_Reflect_allClasses = function(v_vm) {
	var v_generics = PST$createNewArray(1);
	v_generics[0] = 10;
	var v_output = v_makeEmptyList(v_generics, 20);
	var v_classTable = v_vm[4][9];
	var v_i = 1;
	while ((v_i < v_classTable.length)) {
		var v_classInfo = v_classTable[v_i];
		if ((v_classInfo == null)) {
			v_i = v_classTable.length;
		} else {
			v_addToList(v_output, [10, [false, v_classInfo[0]]]);
		}
		v_i += 1;
	}
	return [6, v_output];
};

var v_Reflect_getMethods = function(v_vm, v_ec, v_methodSource) {
	var v_output = v_makeEmptyList(null, 8);
	if ((v_methodSource[0] == 8)) {
		var v_objInstance1 = v_methodSource[1];
		var v_classInfo = v_vm[4][9][v_objInstance1[0]];
		var v_i = 0;
		while ((v_i < v_classInfo[9].length)) {
			var v_functionId = v_classInfo[9][v_i];
			if ((v_functionId != -1)) {
				v_addToList(v_output, [9, [2, v_methodSource, v_objInstance1[0], v_functionId, null]]);
			}
			v_i += 1;
		}
	} else {
		var v_classValue = v_methodSource[1];
		var v_classInfo = v_vm[4][9][v_classValue[1]];
		v_EX_UnsupportedOperation(v_ec, "static method reflection not implemented yet.");
	}
	return [6, v_output];
};

var v_resetLocalsStackTokens = function(v_ec, v_stack) {
	var v_localsStack = v_ec[5];
	var v_localsStackSet = v_ec[6];
	var v_i = v_stack[3];
	while ((v_i < v_localsStackSet.length)) {
		v_localsStackSet[v_i] = 0;
		v_localsStack[v_i] = null;
		v_i += 1;
	}
	var v_stackWalker = v_stack;
	while ((v_stackWalker != null)) {
		var v_token = v_stackWalker[1];
		v_stackWalker[1] = 1;
		v_i = v_stackWalker[2];
		while ((v_i < v_stackWalker[3])) {
			if ((v_localsStackSet[v_i] == v_token)) {
				v_localsStackSet[v_i] = 1;
			} else {
				v_localsStackSet[v_i] = 0;
				v_localsStack[v_i] = null;
			}
			v_i += 1;
		}
		v_stackWalker = v_stackWalker[4];
	}
	v_ec[7] = 1;
	return -1;
};

var v_resolvePrimitiveMethodName2 = function(v_lookup, v_type, v_globalNameId) {
	var v_output = v_lookup[v_globalNameId];
	if ((v_output != -1)) {
		switch ((v_type + (11 * v_output))) {
			case 82:
				return v_output;
			case 104:
				return v_output;
			case 148:
				return v_output;
			case 214:
				return v_output;
			case 225:
				return v_output;
			case 280:
				return v_output;
			case 291:
				return v_output;
			case 302:
				return v_output;
			case 335:
				return v_output;
			case 346:
				return v_output;
			case 357:
				return v_output;
			case 368:
				return v_output;
			case 6:
				return v_output;
			case 39:
				return v_output;
			case 50:
				return v_output;
			case 61:
				return v_output;
			case 72:
				return v_output;
			case 83:
				return v_output;
			case 116:
				return v_output;
			case 160:
				return v_output;
			case 193:
				return v_output;
			case 237:
				return v_output;
			case 259:
				return v_output;
			case 270:
				return v_output;
			case 292:
				return v_output;
			case 314:
				return v_output;
			case 325:
				return v_output;
			case 51:
				return v_output;
			case 62:
				return v_output;
			case 84:
				return v_output;
			case 128:
				return v_output;
			case 205:
				return v_output;
			case 249:
				return v_output;
			case 271:
				return v_output;
			case 381:
				return v_output;
			case 20:
				return v_output;
			case 31:
				return v_output;
			case 141:
				return v_output;
			case 174:
				return v_output;
			case 98:
				return v_output;
			case 142:
				return v_output;
			case 186:
				return v_output;
			default:
				return -1;
		}
	}
	return -1;
};

var v_resource_manager_getResourceOfType = function(v_vm, v_userPath, v_type) {
	var v_db = v_vm[9];
	var v_lookup = v_db[1];
	if ((v_lookup[v_userPath] !== undefined)) {
		var v_output = v_makeEmptyList(null, 2);
		var v_file = v_lookup[v_userPath];
		if (v_file[3] == v_type) {
			v_addToList(v_output, v_vm[12][1]);
			v_addToList(v_output, v_buildString(v_vm[12], v_file[1]));
		} else {
			v_addToList(v_output, v_vm[12][2]);
		}
		return [6, v_output];
	}
	return v_vm[12][0];
};

var v_resource_manager_populate_directory_lookup = function(v_dirs, v_path) {
	var v_parts = v_path.split("/");
	var v_pathBuilder = "";
	var v_file = "";
	var v_i = 0;
	while ((v_i < v_parts.length)) {
		v_file = v_parts[v_i];
		var v_files = null;
		if (!(v_dirs[v_pathBuilder] !== undefined)) {
			v_files = [];
			v_dirs[v_pathBuilder] = v_files;
		} else {
			v_files = v_dirs[v_pathBuilder];
		}
		v_files.push(v_file);
		if ((v_i > 0)) {
			v_pathBuilder = [v_pathBuilder, "/", v_file].join('');
		} else {
			v_pathBuilder = v_file;
		}
		v_i += 1;
	}
	return 0;
};

var v_resourceManagerInitialize = function(v_globals, v_manifest) {
	var v_filesPerDirectoryBuilder = {};
	var v_fileInfo = {};
	var v_dataList = [];
	var v_items = v_manifest.split("\n");
	var v_resourceInfo = null;
	var v_type = "";
	var v_userPath = "";
	var v_internalPath = "";
	var v_argument = "";
	var v_isText = false;
	var v_intType = 0;
	var v_i = 0;
	while ((v_i < v_items.length)) {
		var v_itemData = v_items[v_i].split(",");
		if ((v_itemData.length >= 3)) {
			v_type = v_itemData[0];
			v_isText = "TXT" == v_type;
			if (v_isText) {
				v_intType = 1;
			} else {
				if (("IMGSH" == v_type || "IMG" == v_type)) {
					v_intType = 2;
				} else {
					if ("SND" == v_type) {
						v_intType = 3;
					} else {
						if ("TTF" == v_type) {
							v_intType = 4;
						} else {
							v_intType = 5;
						}
					}
				}
			}
			v_userPath = v_stringDecode(v_itemData[1]);
			v_internalPath = v_itemData[2];
			v_argument = "";
			if ((v_itemData.length > 3)) {
				v_argument = v_stringDecode(v_itemData[3]);
			}
			v_resourceInfo = [v_userPath, v_internalPath, v_isText, v_type, v_argument];
			v_fileInfo[v_userPath] = v_resourceInfo;
			v_resource_manager_populate_directory_lookup(v_filesPerDirectoryBuilder, v_userPath);
			v_dataList.push(v_buildString(v_globals, v_userPath));
			v_dataList.push(v_buildInteger(v_globals, v_intType));
			if ((v_internalPath != null)) {
				v_dataList.push(v_buildString(v_globals, v_internalPath));
			} else {
				v_dataList.push(v_globals[0]);
			}
		}
		v_i += 1;
	}
	var v_dirs = PST$dictionaryKeys(v_filesPerDirectoryBuilder);
	var v_filesPerDirectorySorted = {};
	v_i = 0;
	while ((v_i < v_dirs.length)) {
		var v_dir = v_dirs[v_i];
		var v_unsortedDirs = v_filesPerDirectoryBuilder[v_dir];
		var v_dirsSorted = PST$multiplyList(v_unsortedDirs, 1);
		v_dirsSorted = PST$sortedCopyOfArray(v_dirsSorted);
		v_filesPerDirectorySorted[v_dir] = v_dirsSorted;
		v_i += 1;
	}
	return [v_filesPerDirectorySorted, v_fileInfo, v_dataList];
};

var v_reverseList = function(v_list) {
	v_list[2].reverse();
};

var v_runInterpreter = function(v_vm, v_executionContextId) {
	var v_result = v_interpret(v_vm, v_executionContextId);
	v_result[3] = v_executionContextId;
	var v_status = v_result[0];
	if ((v_status == 1)) {
		if ((v_vm[0][v_executionContextId] !== undefined)) {
			delete v_vm[0][v_executionContextId];
		}
		v_runShutdownHandlers(v_vm);
	} else {
		if ((v_status == 3)) {
			v_printToStdOut(v_vm[11][3], v_result[1]);
			v_runShutdownHandlers(v_vm);
		}
	}
	if ((v_executionContextId == 0)) {
		v_result[4] = true;
	}
	return v_result;
};

var v_runInterpreterWithFunctionPointer = function(v_vm, v_fpValue, v_args) {
	var v_newId = (v_vm[1] + 1);
	v_vm[1] = v_newId;
	var v_argList = [];
	var v_i = 0;
	while ((v_i < v_args.length)) {
		v_argList.push(v_args[v_i]);
		v_i += 1;
	}
	var v_locals = PST$createNewArray(0);
	var v_localsSet = PST$createNewArray(0);
	var v_valueStack = PST$createNewArray(100);
	v_valueStack[0] = v_fpValue;
	v_valueStack[1] = [6, v_argList];
	var v_stack = [(v_vm[2][0].length - 2), 1, 0, 0, null, false, null, 0, 0, 1, 0, null, null, null];
	var v_executionContext = [v_newId, v_stack, 2, 100, v_valueStack, v_locals, v_localsSet, 1, 0, false, null, false, 0, null];
	v_vm[0][v_newId] = v_executionContext;
	return v_runInterpreter(v_vm, v_newId);
};

var v_runShutdownHandlers = function(v_vm) {
	while ((v_vm[10].length > 0)) {
		var v_handler = v_vm[10][0];
		v_vm[10].splice(0, 1);
		v_runInterpreterWithFunctionPointer(v_vm, v_handler, PST$createNewArray(0));
	}
	return 0;
};

var v_setItemInList = function(v_list, v_i, v_v) {
	v_list[2][v_i] = v_v;
};

var v_sortHelperIsRevOrder = function(v_keyStringList, v_keyNumList, v_isString, v_indexLeft, v_indexRight) {
	if (v_isString) {
		return (v_keyStringList[v_indexLeft].localeCompare(v_keyStringList[v_indexRight]) > 0);
	}
	return (v_keyNumList[v_indexLeft] > v_keyNumList[v_indexRight]);
};

var v_sortHelperSwap = function(v_keyStringList, v_keyNumList, v_indices, v_isString, v_index1, v_index2) {
	if ((v_index1 == v_index2)) {
		return 0;
	}
	var v_t = v_indices[v_index1];
	v_indices[v_index1] = v_indices[v_index2];
	v_indices[v_index2] = v_t;
	if (v_isString) {
		var v_s = v_keyStringList[v_index1];
		v_keyStringList[v_index1] = v_keyStringList[v_index2];
		v_keyStringList[v_index2] = v_s;
	} else {
		var v_n = v_keyNumList[v_index1];
		v_keyNumList[v_index1] = v_keyNumList[v_index2];
		v_keyNumList[v_index2] = v_n;
	}
	return 0;
};

var v_sortLists = function(v_keyList, v_parallelList, v_intOutParam) {
	PST$intBuffer16[0] = 0;
	var v_length = v_keyList[1];
	if ((v_length < 2)) {
		return 0;
	}
	var v_i = 0;
	var v_item = null;
	v_item = v_keyList[2][0];
	var v_isString = (v_item[0] == 5);
	var v_stringKeys = null;
	var v_numKeys = null;
	if (v_isString) {
		v_stringKeys = PST$createNewArray(v_length);
	} else {
		v_numKeys = PST$createNewArray(v_length);
	}
	var v_indices = PST$createNewArray(v_length);
	var v_originalOrder = PST$createNewArray(v_length);
	v_i = 0;
	while ((v_i < v_length)) {
		v_indices[v_i] = v_i;
		v_originalOrder[v_i] = v_parallelList[2][v_i];
		v_item = v_keyList[2][v_i];
		switch (v_item[0]) {
			case 3:
				if (v_isString) {
					PST$intBuffer16[0] = 1;
					return 0;
				}
				v_numKeys[v_i] = v_item[1];
				break;
			case 4:
				if (v_isString) {
					PST$intBuffer16[0] = 1;
					return 0;
				}
				v_numKeys[v_i] = v_item[1];
				break;
			case 5:
				if (!v_isString) {
					PST$intBuffer16[0] = 1;
					return 0;
				}
				v_stringKeys[v_i] = v_item[1];
				break;
			default:
				PST$intBuffer16[0] = 1;
				return 0;
		}
		v_i += 1;
	}
	v_qsortHelper(v_stringKeys, v_numKeys, v_indices, v_isString, 0, (v_length - 1));
	v_i = 0;
	while ((v_i < v_length)) {
		v_parallelList[2][v_i] = v_originalOrder[v_indices[v_i]];
		v_i += 1;
	}
	return 0;
};

var v_stackItemIsLibrary = function(v_stackInfo) {
	if ((v_stackInfo.charAt(0) != "[")) {
		return false;
	}
	var v_cIndex = v_stackInfo.indexOf(":");
	return ((v_cIndex > 0) && (v_cIndex < v_stackInfo.indexOf("]")));
};

var v_startVm = function(v_vm) {
	return v_runInterpreter(v_vm, v_vm[1]);
};

var v_stringDecode = function(v_encoded) {
	if (!(v_encoded.indexOf("%") != -1)) {
		var v_length = v_encoded.length;
		var v_per = "%";
		var v_builder = [];
		var v_i = 0;
		while ((v_i < v_length)) {
			var v_c = v_encoded.charAt(v_i);
			if (((v_c == v_per) && ((v_i + 2) < v_length))) {
				v_builder.push(v_stringFromHex(["", v_encoded.charAt((v_i + 1)), v_encoded.charAt((v_i + 2))].join('')));
			} else {
				v_builder.push("" + v_c);
			}
			v_i += 1;
		}
		return v_builder.join("");
	}
	return v_encoded;
};

var v_stringFromHex = function(v_encoded) {
	v_encoded = v_encoded.toUpperCase();
	var v_hex = "0123456789ABCDEF";
	var v_output = [];
	var v_length = v_encoded.length;
	var v_a = 0;
	var v_b = 0;
	var v_c = null;
	var v_i = 0;
	while (((v_i + 1) < v_length)) {
		v_c = "" + v_encoded.charAt(v_i);
		v_a = v_hex.indexOf(v_c);
		if ((v_a == -1)) {
			return null;
		}
		v_c = "" + v_encoded.charAt((v_i + 1));
		v_b = v_hex.indexOf(v_c);
		if ((v_b == -1)) {
			return null;
		}
		v_a = ((v_a * 16) + v_b);
		v_output.push(String.fromCharCode(v_a));
		v_i += 2;
	}
	return v_output.join("");
};

var v_suspendInterpreter = function() {
	return [2, null, 0.0, 0, false, ""];
};

var v_tokenDataImpl = function(v_vm, v_row) {
	var v_tokensByPc = v_vm[3][0];
	var v_pc = (v_row[0] + v_vm[4][16]);
	var v_line = v_row[1];
	var v_col = v_row[2];
	var v_file = v_row[3];
	var v_tokens = v_tokensByPc[v_pc];
	if ((v_tokens == null)) {
		v_tokens = [];
		v_tokensByPc[v_pc] = v_tokens;
	}
	v_tokens.push([v_line, v_col, v_file]);
	return 0;
};

var v_tokenHelperConvertPcsToStackTraceStrings = function(v_vm, v_pcs) {
	var v_tokens = v_generateTokenListFromPcs(v_vm, v_pcs);
	var v_files = v_vm[3][1];
	var v_output = [];
	var v_i = 0;
	while ((v_i < v_tokens.length)) {
		var v_token = v_tokens[v_i];
		if ((v_token == null)) {
			v_output.push("[No stack information]");
		} else {
			var v_line = v_token[0];
			var v_col = v_token[1];
			var v_fileData = v_files[v_token[2]];
			var v_lines = v_fileData.split("\n");
			var v_filename = v_lines[0];
			var v_linevalue = v_lines[(v_line + 1)];
			v_output.push([v_filename, ", Line: ", ('' + (v_line + 1)), ", Col: ", ('' + (v_col + 1))].join(''));
		}
		v_i += 1;
	}
	return v_output;
};

var v_tokenHelperGetFileLine = function(v_vm, v_fileId, v_lineNum) {
	var v_sourceCode = v_vm[3][1][v_fileId];
	if ((v_sourceCode == null)) {
		return null;
	}
	return v_sourceCode.split("\n")[v_lineNum];
};

var v_tokenHelperGetFormattedPointerToToken = function(v_vm, v_token) {
	var v_line = v_tokenHelperGetFileLine(v_vm, v_token[2], (v_token[0] + 1));
	if ((v_line == null)) {
		return null;
	}
	var v_columnIndex = v_token[1];
	var v_lineLength = v_line.length;
	v_line = PST$stringTrimOneSide(v_line, true);
	v_line = v_line.split("\t").join(" ");
	var v_offset = (v_lineLength - v_line.length);
	v_columnIndex -= v_offset;
	var v_line2 = "";
	while ((v_columnIndex > 0)) {
		v_columnIndex -= 1;
		v_line2 = v_line2 + " ";
	}
	v_line2 = v_line2 + "^";
	return [v_line, "\n", v_line2].join('');
};

var v_tokenHelplerIsFilePathLibrary = function(v_vm, v_fileId, v_allFiles) {
	var v_filename = v_tokenHelperGetFileLine(v_vm, v_fileId, 0);
	return !PST$stringEndsWith(v_filename.toLowerCase(), ".cry");
};

var v_typeInfoToString = function(v_vm, v_typeInfo, v_i) {
	var v_output = [];
	v_typeToStringBuilder(v_vm, v_output, v_typeInfo, v_i);
	return v_output.join("");
};

var v_typeToString = function(v_vm, v_typeInfo, v_i) {
	var v_sb = [];
	v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i);
	return v_sb.join("");
};

var v_typeToStringBuilder = function(v_vm, v_sb, v_typeInfo, v_i) {
	switch (v_typeInfo[v_i]) {
		case -1:
			v_sb.push("void");
			return (v_i + 1);
		case 0:
			v_sb.push("object");
			return (v_i + 1);
		case 1:
			v_sb.push("object");
			return (v_i + 1);
		case 3:
			v_sb.push("int");
			return (v_i + 1);
		case 4:
			v_sb.push("float");
			return (v_i + 1);
		case 2:
			v_sb.push("bool");
			return (v_i + 1);
		case 5:
			v_sb.push("string");
			return (v_i + 1);
		case 6:
			v_sb.push("List<");
			v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, (v_i + 1));
			v_sb.push(">");
			return v_i;
		case 7:
			v_sb.push("Dictionary<");
			v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, (v_i + 1));
			v_sb.push(", ");
			v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i);
			v_sb.push(">");
			return v_i;
		case 8:
			var v_classId = v_typeInfo[(v_i + 1)];
			if ((v_classId == 0)) {
				v_sb.push("object");
			} else {
				var v_classInfo = v_vm[4][9][v_classId];
				v_sb.push(v_classInfo[16]);
			}
			return (v_i + 2);
		case 10:
			v_sb.push("Class");
			return (v_i + 1);
		case 9:
			var v_n = v_typeInfo[(v_i + 1)];
			var v_optCount = v_typeInfo[(v_i + 2)];
			v_i += 2;
			v_sb.push("function(");
			var v_ret = [];
			v_i = v_typeToStringBuilder(v_vm, v_ret, v_typeInfo, v_i);
			var v_j = 1;
			while ((v_j < v_n)) {
				if ((v_j > 1)) {
					v_sb.push(", ");
				}
				v_i = v_typeToStringBuilder(v_vm, v_sb, v_typeInfo, v_i);
				v_j += 1;
			}
			if ((v_n == 1)) {
				v_sb.push("void");
			}
			v_sb.push(" => ");
			var v_optStart = (v_n - v_optCount - 1);
			v_j = 0;
			while ((v_j < v_ret.length)) {
				if ((v_j >= v_optStart)) {
					v_sb.push("(opt) ");
				}
				v_sb.push(v_ret[v_j]);
				v_j += 1;
			}
			v_sb.push(")");
			return v_i;
		default:
			v_sb.push("UNKNOWN");
			return (v_i + 1);
	}
};

var v_typeToStringFromValue = function(v_vm, v_value) {
	var v_sb = null;
	switch (v_value[0]) {
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
			var v_classId = (v_value[1])[0];
			var v_classInfo = v_vm[4][9][v_classId];
			return v_classInfo[16];
		case 6:
			v_sb = [];
			v_sb.push("List<");
			var v_list = v_value[1];
			if ((v_list[0] == null)) {
				v_sb.push("object");
			} else {
				v_typeToStringBuilder(v_vm, v_sb, v_list[0], 0);
			}
			v_sb.push(">");
			return v_sb.join("");
		case 7:
			var v_dict = v_value[1];
			v_sb = [];
			v_sb.push("Dictionary<");
			switch (v_dict[1]) {
				case 3:
					v_sb.push("int");
					break;
				case 5:
					v_sb.push("string");
					break;
				case 8:
					v_sb.push("object");
					break;
				default:
					v_sb.push("???");
					break;
			}
			v_sb.push(", ");
			if ((v_dict[3] == null)) {
				v_sb.push("object");
			} else {
				v_typeToStringBuilder(v_vm, v_sb, v_dict[3], 0);
			}
			v_sb.push(">");
			return v_sb.join("");
		case 9:
			return "Function";
		default:
			return "Unknown";
	}
};

var v_uncaughtExceptionResult = function(v_vm, v_exception) {
	return [3, v_unrollExceptionOutput(v_vm, v_exception), 0.0, 0, false, ""];
};

var v_unrollExceptionOutput = function(v_vm, v_exceptionInstance) {
	var v_objInstance = v_exceptionInstance[1];
	var v_classInfo = v_vm[4][9][v_objInstance[0]];
	var v_pcs = v_objInstance[3][1];
	var v_codeFormattedPointer = "";
	var v_exceptionName = v_classInfo[16];
	var v_message = v_valueToString(v_vm, v_objInstance[2][1]);
	var v_trace = v_tokenHelperConvertPcsToStackTraceStrings(v_vm, v_pcs);
	v_trace.pop();
	v_trace.push("Stack Trace:");
	v_trace.reverse();
	v_pcs.reverse();
	var v_showLibStack = v_vm[11][1];
	if ((!v_showLibStack && !v_stackItemIsLibrary(v_trace[0]))) {
		while (v_stackItemIsLibrary(v_trace[(v_trace.length - 1)])) {
			v_trace.pop();
			v_pcs.pop();
		}
	}
	var v_tokensAtPc = v_vm[3][0][v_pcs[(v_pcs.length - 1)]];
	if ((v_tokensAtPc != null)) {
		v_codeFormattedPointer = "\n\n" + v_tokenHelperGetFormattedPointerToToken(v_vm, v_tokensAtPc[0]);
	}
	var v_stackTrace = v_trace.join("\n");
	return [v_stackTrace, v_codeFormattedPointer, "\n", v_exceptionName, ": ", v_message].join('');
};

var v_valueConcatLists = function(v_a, v_b) {
	return [null, (v_a[1] + v_b[1]), v_a[2].concat(v_b[2])];
};

var v_valueMultiplyList = function(v_a, v_n) {
	var v_len = (v_a[1] * v_n);
	var v_output = v_makeEmptyList(v_a[0], v_len);
	if ((v_len == 0)) {
		return v_output;
	}
	var v_aLen = v_a[1];
	var v_i = 0;
	var v_value = null;
	if ((v_aLen == 1)) {
		v_value = v_a[2][0];
		v_i = 0;
		while ((v_i < v_n)) {
			v_output[2].push(v_value);
			v_i += 1;
		}
	} else {
		var v_j = 0;
		v_i = 0;
		while ((v_i < v_n)) {
			v_j = 0;
			while ((v_j < v_aLen)) {
				v_output[2].push(v_a[2][v_j]);
				v_j += 1;
			}
			v_i += 1;
		}
	}
	v_output[1] = v_len;
	return v_output;
};

var v_valueStackIncreaseCapacity = function(v_ec) {
	var v_stack = v_ec[4];
	var v_oldCapacity = v_stack.length;
	var v_newCapacity = (v_oldCapacity * 2);
	var v_newStack = PST$createNewArray(v_newCapacity);
	var v_i = (v_oldCapacity - 1);
	while ((v_i >= 0)) {
		v_newStack[v_i] = v_stack[v_i];
		v_i -= 1;
	}
	v_ec[4] = v_newStack;
	return v_newStack;
};

var v_valueToString = function(v_vm, v_wrappedValue) {
	var v_type = v_wrappedValue[0];
	if ((v_type == 1)) {
		return "null";
	}
	if ((v_type == 2)) {
		if (v_wrappedValue[1]) {
			return "true";
		}
		return "false";
	}
	if ((v_type == 4)) {
		var v_floatStr = '' + v_wrappedValue[1];
		if (!(v_floatStr.indexOf(".") != -1)) {
			v_floatStr += ".0";
		}
		return v_floatStr;
	}
	if ((v_type == 3)) {
		return ('' + v_wrappedValue[1]);
	}
	if ((v_type == 5)) {
		return v_wrappedValue[1];
	}
	if ((v_type == 6)) {
		var v_internalList = v_wrappedValue[1];
		var v_output = "[";
		var v_i = 0;
		while ((v_i < v_internalList[1])) {
			if ((v_i > 0)) {
				v_output += ", ";
			}
			v_output += v_valueToString(v_vm, v_internalList[2][v_i]);
			v_i += 1;
		}
		v_output += "]";
		return v_output;
	}
	if ((v_type == 8)) {
		var v_objInstance = v_wrappedValue[1];
		var v_classId = v_objInstance[0];
		var v_ptr = v_objInstance[1];
		var v_classInfo = v_vm[4][9][v_classId];
		var v_nameId = v_classInfo[1];
		var v_className = v_vm[4][0][v_nameId];
		return ["Instance<", v_className, "#", ('' + v_ptr), ">"].join('');
	}
	if ((v_type == 7)) {
		var v_dict = v_wrappedValue[1];
		if ((v_dict[0] == 0)) {
			return "{}";
		}
		var v_output = "{";
		var v_keyList = v_dict[6];
		var v_valueList = v_dict[7];
		var v_i = 0;
		while ((v_i < v_dict[0])) {
			if ((v_i > 0)) {
				v_output += ", ";
			}
			v_output += [v_valueToString(v_vm, v_dict[6][v_i]), ": ", v_valueToString(v_vm, v_dict[7][v_i])].join('');
			v_i += 1;
		}
		v_output += " }";
		return v_output;
	}
	return "<unknown>";
};

var v_vm_getCurrentExecutionContextId = function(v_vm) {
	return v_vm[1];
};

var v_vm_suspend = function(v_vm, v_status) {
	return v_vm_suspend_for_context(v_getExecutionContext(v_vm, -1), 1);
};

var v_vm_suspend_for_context = function(v_ec, v_status) {
	v_ec[11] = true;
	v_ec[12] = v_status;
	return 0;
};

var v_vm_suspend_with_status = function(v_vm, v_status) {
	return v_vm_suspend_for_context(v_getExecutionContext(v_vm, -1), v_status);
};

var v_vmEnvSetCommandLineArgs = function(v_vm, v_args) {
	v_vm[11][0] = v_args;
};