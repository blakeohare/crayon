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

var lib_xml_ampUnescape = function(value, entityLookup) {
	var ampParts = value.split("&");
	var i = 1;
	while ((i < ampParts.length)) {
		var component = ampParts[i];
		var semicolon = component.indexOf(";");
		if ((semicolon != -1)) {
			var entityCode = component.substring(0, 0 + semicolon);
			var entityValue = lib_xml_getEntity(entityCode, entityLookup);
			if ((entityValue == null)) {
				entityValue = "&";
			} else {
				component = component.substring((semicolon + 1), (semicolon + 1) + (component.length - semicolon));
			}
			ampParts[i] = entityValue + component;
		}
		i += 1;
	}
	return ampParts.join("");
};

var lib_xml_error = function(xml, index, msg) {
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

var lib_xml_getEntity = function(code, entityLookup) {
	if ((entityLookup[code] !== undefined)) {
		return entityLookup[code];
	}
	return null;
};

var lib_xml_isNext = function(xml, indexPtr, value) {
	return PST$checksubstring(xml, indexPtr[0], value);
};

var lib_xml_parse = function(vm, args) {
	var xml = args[0][1];
	var list1 = args[1][1];
	var objInstance1 = args[2][1];
	var objArray1 = objInstance1[3];
	if ((objArray1 == null)) {
		objArray1 = PST$createNewArray(2);
		objInstance1[3] = objArray1;
		objArray1[0] = {};
		objArray1[1] = {};
	}
	var output = [];
	var errMsg = lib_xml_parseImpl(vm, xml, PST$intBuffer16, output, objArray1[0], objArray1[1]);
	if ((errMsg != null)) {
		return buildString(vm[13], errMsg);
	}
	var list2 = buildList(output)[1];
	list1[1] = list2[1];
	list1[2] = list2[2];
	return vm[14];
};

var lib_xml_parseElement = function(vm, xml, indexPtr, output, entityLookup, stringEnders) {
	var length = xml.length;
	var attributeKeys = [];
	var attributeValues = [];
	var children = [];
	var element = [];
	var error = null;
	if (!lib_xml_popIfPresent(xml, indexPtr, "<")) {
		return lib_xml_error(xml, indexPtr[0], "Expected: '<'");
	}
	var name = lib_xml_popName(xml, indexPtr);
	lib_xml_skipWhitespace(xml, indexPtr);
	var hasClosingTag = true;
	while (true) {
		if ((indexPtr[0] >= length)) {
			return lib_xml_error(xml, length, "Unexpected EOF");
		}
		if (lib_xml_popIfPresent(xml, indexPtr, ">")) {
			break;
		}
		if (lib_xml_popIfPresent(xml, indexPtr, "/>")) {
			hasClosingTag = false;
			break;
		}
		var key = lib_xml_popName(xml, indexPtr);
		if ((key.length == 0)) {
			return lib_xml_error(xml, indexPtr[0], "Expected attribute name.");
		}
		attributeKeys.push(buildString(vm[13], key));
		lib_xml_skipWhitespace(xml, indexPtr);
		if (!lib_xml_popIfPresent(xml, indexPtr, "=")) {
			return lib_xml_error(xml, indexPtr[0], "Expected: '='");
		}
		lib_xml_skipWhitespace(xml, indexPtr);
		error = lib_xml_popString(vm, xml, indexPtr, attributeValues, entityLookup, stringEnders);
		if ((error != null)) {
			return error;
		}
		lib_xml_skipWhitespace(xml, indexPtr);
	}
	if (hasClosingTag) {
		var close = ["</", name, ">"].join('');
		while (!lib_xml_popIfPresent(xml, indexPtr, close)) {
			if (lib_xml_isNext(xml, indexPtr, "</")) {
				error = lib_xml_error(xml, (indexPtr[0] - 2), "Unexpected close tag.");
			} else {
				if (lib_xml_isNext(xml, indexPtr, "<!--")) {
					error = lib_xml_skipComment(xml, indexPtr);
				} else {
					if (lib_xml_isNext(xml, indexPtr, "<")) {
						error = lib_xml_parseElement(vm, xml, indexPtr, children, entityLookup, stringEnders);
					} else {
						error = lib_xml_parseText(vm, xml, indexPtr, children, entityLookup);
					}
				}
			}
			if (((error == null) && (indexPtr[0] >= length))) {
				error = lib_xml_error(xml, length, "Unexpected EOF. Unclosed tag.");
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

var lib_xml_parseImpl = function(vm, input, indexPtr, output, entityLookup, stringEnders) {
	if ((Object.keys(entityLookup).length == 0)) {
		entityLookup["amp"] = "&";
		entityLookup["lt"] = "<";
		entityLookup["gt"] = ">";
		entityLookup["quot"] = "\"";
		entityLookup["apos"] = "'";
	}
	if ((Object.keys(stringEnders).length == 0)) {
		stringEnders[(" ").charCodeAt(0)] = 1;
		stringEnders[("\"").charCodeAt(0)] = 1;
		stringEnders[("'").charCodeAt(0)] = 1;
		stringEnders[("<").charCodeAt(0)] = 1;
		stringEnders[(">").charCodeAt(0)] = 1;
		stringEnders[("\t").charCodeAt(0)] = 1;
		stringEnders[("\r").charCodeAt(0)] = 1;
		stringEnders[("\n").charCodeAt(0)] = 1;
		stringEnders[("/").charCodeAt(0)] = 1;
	}
	indexPtr[0] = 0;
	lib_xml_skipWhitespace(input, indexPtr);
	if (lib_xml_popIfPresent(input, indexPtr, "<?xml")) {
		var newBegin = input.indexOf("?>");
		if ((newBegin == -1)) {
			return lib_xml_error(input, (indexPtr[0] - 5), "XML Declaration is not closed.");
		}
		indexPtr[0] = (newBegin + 2);
	}
	var error = lib_xml_skipStuff(input, indexPtr);
	if ((error != null)) {
		return error;
	}
	error = lib_xml_parseElement(vm, input, indexPtr, output, entityLookup, stringEnders);
	if ((error != null)) {
		return error;
	}
	lib_xml_skipStuff(input, indexPtr);
	if ((indexPtr[0] != input.length)) {
		return lib_xml_error(input, indexPtr[0], "Unexpected text.");
	}
	return null;
};

var lib_xml_parseText = function(vm, xml, indexPtr, output, entityLookup) {
	var length = xml.length;
	var start = indexPtr[0];
	var i = start;
	var ampFound = false;
	var c = " ";
	while ((i < length)) {
		c = xml.charAt(i);
		if ((c == "<")) {
			break;
		} else {
			if ((c == "&")) {
				ampFound = true;
			}
		}
		i += 1;
	}
	if ((i > start)) {
		indexPtr[0] = i;
		var textValue = xml.substring(start, start + (i - start));
		if (ampFound) {
			textValue = lib_xml_ampUnescape(textValue, entityLookup);
		}
		var textElement = [];
		textElement.push(vm[16]);
		textElement.push(buildString(vm[13], textValue));
		output.push(buildList(textElement));
	}
	return null;
};

var lib_xml_popIfPresent = function(xml, indexPtr, s) {
	if (PST$checksubstring(xml, indexPtr[0], s)) {
		indexPtr[0] = (indexPtr[0] + s.length);
		return true;
	}
	return false;
};

var lib_xml_popName = function(xml, indexPtr) {
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

var lib_xml_popString = function(vm, xml, indexPtr, attributeValueOut, entityLookup, stringEnders) {
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
			} else {
				if ((c == ("&").charCodeAt(0))) {
					ampFound = true;
				}
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
			} else {
				if ((c == ("&").charCodeAt(0))) {
					ampFound = true;
				}
			}
			i += 1;
		}
	}
	indexPtr[0] = i;
	var output = xml.substring(start, start + (end - start));
	if (ampFound) {
		output = lib_xml_ampUnescape(output, entityLookup);
	}
	attributeValueOut.push(buildString(vm[13], output));
	return null;
};

var lib_xml_skipComment = function(xml, indexPtr) {
	if (lib_xml_popIfPresent(xml, indexPtr, "<!--")) {
		var i = xml.indexOf("-->", indexPtr[0]);
		if ((i == -1)) {
			return lib_xml_error(xml, (indexPtr[0] - 4), "Unclosed comment.");
		}
		indexPtr[0] = (i + 3);
	}
	return null;
};

var lib_xml_skipStuff = function(xml, indexPtr) {
	var index = (indexPtr[0] - 1);
	while ((index < indexPtr[0])) {
		index = indexPtr[0];
		lib_xml_skipWhitespace(xml, indexPtr);
		var error = lib_xml_skipComment(xml, indexPtr);
		if ((error != null)) {
			return error;
		}
	}
	return null;
};

var lib_xml_skipWhitespace = function(xml, indexPtr) {
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
