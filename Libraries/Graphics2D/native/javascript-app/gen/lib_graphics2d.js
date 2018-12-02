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

var lib_graphics2d_addImageRenderEvent = function(vm, args) {
	var i = 0;
	// get the drawing queue data;
	var drawQueueData = (args[0][1])[3];
	// expand the draw event queue;
	var eventQueue = drawQueueData[0];
	var queueLength = drawQueueData[1];
	if ((queueLength >= eventQueue.length)) {
		eventQueue = lib_graphics2d_expandEventQueueCapacity(eventQueue);
		drawQueueData[0] = eventQueue;
	}
	drawQueueData[1] = (queueLength + 16);
	// expand (or create) the image native data queue;
	var imageNativeDataQueue = drawQueueData[2];
	var imageNativeDataQueueSize = 0;
	if ((imageNativeDataQueue == null)) {
		imageNativeDataQueue = PST$createNewArray(16);
	} else {
		imageNativeDataQueueSize = drawQueueData[3];
	}
	if ((imageNativeDataQueueSize >= imageNativeDataQueue.length)) {
		var objArrayArray2 = PST$createNewArray(((imageNativeDataQueueSize * 2) + 16));
		i = 0;
		while ((i < imageNativeDataQueueSize)) {
			objArrayArray2[i] = imageNativeDataQueue[i];
			i += 1;
		}
		imageNativeDataQueue = objArrayArray2;
		drawQueueData[2] = imageNativeDataQueue;
	}
	drawQueueData[3] = (imageNativeDataQueueSize + 1);
	// Add the image to the image native data queue;
	var imageNativeData = (args[1][1])[3];
	imageNativeDataQueue[imageNativeDataQueueSize] = imageNativeData;
	var isValid = true;
	var isNoop = false;
	// mark event as an Image event (6);
	eventQueue[queueLength] = 6;
	// get/set the draw options mask;
	var flag = args[2][1];
	eventQueue[(queueLength | 1)] = flag;
	// rotation;
	if (((flag & 4) != 0)) {
		var rotationValue = args[11];
		var theta = 0.0;
		if ((rotationValue[0] == 4)) {
			theta = rotationValue[1];
		} else {
			if ((rotationValue[0] == 3)) {
				theta += rotationValue[1];
			} else {
				isValid = false;
			}
		}
		eventQueue[(queueLength | 10)] = Math.floor((canonicalizeAngle(theta) * 1048576));
	}
	// alpha;
	if (((flag & 8) != 0)) {
		var alphaValue = args[12];
		var alpha = 0;
		if ((alphaValue[0] == 3)) {
			alpha = alphaValue[1];
		} else {
			if ((alphaValue[0] == 4)) {
				alpha = Math.floor((0.5 + alphaValue[1]));
			} else {
				isValid = false;
			}
		}
		if ((i > 254)) {
			eventQueue[(queueLength | 1)] = (flag - 8);
		} else {
			if ((i < 0)) {
				isNoop = true;
			} else {
				eventQueue[(queueLength | 11)] = alpha;
			}
		}
	}
	// Copy values to event queue;
	var value = null;
	i = 3;
	while ((i < 11)) {
		value = args[i];
		if ((value[0] == 3)) {
			eventQueue[(queueLength + i - 1)] = value[1];
		} else {
			if ((value[0] == 4)) {
				eventQueue[(queueLength + i - 1)] = Math.floor((0.5 + value[1]));
			} else {
				isValid = false;
			}
		}
		i += 1;
	}
	// slicing;
	if (((flag & 1) != 0)) {
		var actualWidth = imageNativeData[5];
		var sourceX = eventQueue[(queueLength | 2)];
		var sourceWidth = eventQueue[(queueLength | 4)];
		if (((sourceX < 0) || ((sourceX + sourceWidth) > actualWidth) || (sourceWidth < 0))) {
			isValid = false;
		} else {
			if ((sourceWidth == 0)) {
				isNoop = true;
			}
		}
		var actualHeight = imageNativeData[6];
		var sourceY = eventQueue[(queueLength | 3)];
		var sourceHeight = eventQueue[(queueLength | 5)];
		if (((sourceY < 0) || ((sourceY + sourceHeight) > actualHeight) || (sourceHeight < 0))) {
			isValid = false;
		} else {
			if ((sourceHeight == 0)) {
				isNoop = true;
			}
		}
	}
	// stretching;
	if (((flag & 2) != 0)) {
		if ((eventQueue[(queueLength | 6)] <= 0)) {
			if ((eventQueue[(queueLength | 6)] < 0)) {
				isValid = false;
			} else {
				isNoop = true;
			}
		}
		if ((eventQueue[(queueLength | 7)] <= 0)) {
			if ((eventQueue[(queueLength | 7)] < 0)) {
				isValid = false;
			} else {
				isNoop = true;
			}
		}
	}
	// Revert the operation if it is null or a no-op;
	if ((isNoop || !isValid)) {
		drawQueueData[1] = queueLength;
		drawQueueData[3] = imageNativeDataQueueSize;
	}
	if ((isValid || isNoop)) {
		return vm[15];
	}
	return vm[16];
};

var lib_graphics2d_expandEventQueueCapacity = function(a) {
	var _len = a.length;
	var output = PST$createNewArray(((_len * 2) + 16));
	var i = 0;
	while ((i < _len)) {
		output[i] = a[i];
		i += 1;
	}
	return output;
};

var lib_graphics2d_flip = function(vm, args) {
	var bool1 = false;
	var bool2 = false;
	var i = 0;
	var objArray1 = null;
	var objArray2 = null;
	var object1 = null;
	var objInstance1 = null;
	var objInstance2 = null;
	var arg1 = args[0];
	var arg2 = args[1];
	var arg3 = args[2];
	var arg4 = args[3];
	var arg5 = args[4];
	var arg6 = args[5];
	objInstance1 = arg1[1];
	objInstance2 = arg2[1];
	objArray1 = objInstance1[3];
	objArray2 = PST$createNewArray(7);
	objInstance2[3] = objArray2;
	bool1 = arg3[1];
	bool2 = arg4[1];
	i = 6;
	while ((i >= 0)) {
		objArray2[i] = objArray1[i];
		i -= 1;
	}
	objInstance1 = arg6[1];
	objArray1 = objInstance1[3];
	objInstance2 = arg2[1];
	objInstance2[3][0] = objArray1;
	object1 = objArray1[3];
	object1 = C$drawing$flipImage(object1, bool1, bool2);
	objArray1[3] = object1;
	return arg2;
};

var lib_graphics2d_initializeTexture = function(vm, args) {
	var arg1 = args[0];
	var arg2 = args[1];
	var arg3 = args[2];
	var arg4 = args[3];
	var arg5 = args[4];
	var objInstance1 = arg1[1];
	var objArray1 = PST$createNewArray(7);
	objInstance1[3] = objArray1;
	objInstance1 = arg2[1];
	objArray1[0] = objInstance1[3];
	var list1 = arg3[1];
	var value = getItemFromList(list1, 0);
	var float1 = value[1];
	value = getItemFromList(list1, 2);
	var float2 = value[1];
	objArray1[1] = float1;
	objArray1[3] = float2;
	value = getItemFromList(list1, 1);
	float1 = value[1];
	value = getItemFromList(list1, 3);
	float2 = value[1];
	objArray1[2] = float1;
	objArray1[4] = float2;
	objArray1[5] = arg4[1];
	objArray1[6] = arg5[1];
	return vm[14];
};

var lib_graphics2d_initializeTextureResource = function(vm, args) {
	var textureResourceInstance = args[0][1];
	var textureResourceNativeData = PST$createNewArray(6);
	textureResourceInstance[3] = textureResourceNativeData;
	var nativeImageDataInstance = args[2][1];
	var nativeImageDataNativeData = nativeImageDataInstance[3];
	if (args[1][1]) {
		textureResourceNativeData[0] = false;
		textureResourceNativeData[1] = false;
		textureResourceNativeData[2] = -1;
		textureResourceNativeData[3] = nativeImageDataNativeData[0];
		textureResourceNativeData[4] = nativeImageDataNativeData[1];
		textureResourceNativeData[5] = nativeImageDataNativeData[2];
	} else {
		textureResourceNativeData[0] = false;
		textureResourceNativeData[1] = true;
		textureResourceNativeData[2] = -1;
		textureResourceNativeData[3] = nativeImageDataNativeData[3];
		textureResourceNativeData[4] = nativeImageDataNativeData[4];
		textureResourceNativeData[5] = nativeImageDataNativeData[5];
	}
	return vm[14];
};

var lib_graphics2d_isOpenGlBased = function(vm, args) {
	return vm[16];
};

var lib_graphics2d_isPlatformUsingTextureAtlas = function(vm, args) {
	return vm[16];
};

var lib_graphics2d_lineToQuad = function(vm, args) {
	var float1 = 0.0;
	var float2 = 0.0;
	var float3 = 0.0;
	var i = 0;
	var j = 0;
	var int1 = 0;
	var int2 = 0;
	var int3 = 0;
	var int4 = 0;
	var int5 = 0;
	var objInstance1 = args[0][1];
	var objArray1 = objInstance1[3];
	var intArray1 = objArray1[0];
	var _len = (objArray1[1] - 16);
	int1 = intArray1[(_len + 1)];
	int2 = intArray1[(_len + 2)];
	int3 = intArray1[(_len + 3)];
	int4 = intArray1[(_len + 4)];
	int5 = intArray1[(_len + 5)];
	float1 = ((0.0 + int4) - int2);
	float2 = ((0.0 + int3) - int1);
	float3 = (float1 / float2);
	float1 = (int5 / 2.0);
	if ((float1 < 0.5)) {
		float1 = 1.0;
	}
	float2 = (float1 / (Math.pow(((float3 * float3) + 1), 0.5)));
	float1 = (-float2 * float3);
	i = Math.floor(((int1 + float1) + 0.5));
	j = Math.floor(((int1 - float1) + 0.5));
	if ((i == j)) {
		j += 1;
	}
	intArray1[(_len + 1)] = i;
	intArray1[(_len + 3)] = j;
	i = Math.floor(((int2 + float2) + 0.5));
	j = Math.floor(((int2 - float2) + 0.5));
	if ((i == j)) {
		j += 1;
	}
	intArray1[(_len + 2)] = i;
	intArray1[(_len + 4)] = j;
	i = Math.floor(((int3 - float1) + 0.5));
	j = Math.floor(((int3 + float1) + 0.5));
	if ((i == j)) {
		i += 1;
	}
	intArray1[(_len + 5)] = i;
	intArray1[(_len + 7)] = j;
	i = Math.floor(((int4 - float2) + 0.5));
	j = Math.floor(((int4 + float2) + 0.5));
	if ((i == j)) {
		i += 1;
	}
	intArray1[(_len + 6)] = i;
	intArray1[(_len + 8)] = j;
	return vm[14];
};

var lib_graphics2d_renderQueueAction = function(vm, args) {
	var command = args[2][1];
	var objInstance1 = args[0][1];
	var objArray1 = objInstance1[3];
	if ((objArray1 == null)) {
		objArray1 = PST$createNewArray(5);
		objInstance1[3] = objArray1;
	}
	var intArray1 = objArray1[0];
	if ((intArray1 == null)) {
		intArray1 = PST$createNewArray(0);
		objArray1[0] = intArray1;
		objArray1[1] = 0;
		objArray1[2] = PST$createNewArray(64);
		objArray1[3] = 0;
		objArray1[4] = [];
	}
	var intList1 = objArray1[4];
	if ((command == 1)) {
		var charList = args[1];
		if ((charList[0] == 6)) {
			var value = null;
			var list1 = charList[1];
			var _len = list1.length;
			var i = 0;
			while ((i < _len)) {
				value = list1[i];
				intList1.push(value[1]);
				i += 1;
			}
		}
		var renderArgs = PST$createNewArray(4);
		renderArgs[0] = intArray1;
		renderArgs[1] = objArray1[1];
		renderArgs[2] = objArray1[2];
		renderArgs[3] = intList1;
		var callbackId = getNamedCallbackId(vm, "Game", "set-render-data");
		invokeNamedCallback(vm, callbackId, renderArgs);
	} else {
		if ((command == 2)) {
			objArray1[1] = 0;
			objArray1[3] = 0;
			PST$clearList((intList1));
		}
	}
	return vm[14];
};

var lib_graphics2d_renderQueueValidateArgs = function(vm, args) {
	var o = args[0][1];
	var drawQueueRawData = o[3];
	var drawEvents = drawQueueRawData[0];
	var length = drawQueueRawData[1];
	var r = 0;
	var g = 0;
	var b = 0;
	var a = 0;
	var i = 0;
	while ((i < length)) {
		switch (drawEvents[i]) {
			case 1:
				r = drawEvents[(i | 5)];
				g = drawEvents[(i | 6)];
				b = drawEvents[(i | 7)];
				a = drawEvents[(i | 8)];
				if ((r > 255)) {
					drawEvents[(i | 5)] = 255;
				} else {
					if ((r < 0)) {
						drawEvents[(i | 5)] = 0;
					}
				}
				if ((g > 255)) {
					drawEvents[(i | 6)] = 255;
				} else {
					if ((g < 0)) {
						drawEvents[(i | 6)] = 0;
					}
				}
				if ((b > 255)) {
					drawEvents[(i | 7)] = 255;
				} else {
					if ((b < 0)) {
						drawEvents[(i | 7)] = 0;
					}
				}
				if ((a > 255)) {
					drawEvents[(i | 8)] = 255;
				} else {
					if ((a < 0)) {
						drawEvents[(i | 8)] = 0;
					}
				}
				break;
			case 2:
				r = drawEvents[(i | 5)];
				g = drawEvents[(i | 6)];
				b = drawEvents[(i | 7)];
				a = drawEvents[(i | 8)];
				if ((r > 255)) {
					drawEvents[(i | 5)] = 255;
				} else {
					if ((r < 0)) {
						drawEvents[(i | 5)] = 0;
					}
				}
				if ((g > 255)) {
					drawEvents[(i | 6)] = 255;
				} else {
					if ((g < 0)) {
						drawEvents[(i | 6)] = 0;
					}
				}
				if ((b > 255)) {
					drawEvents[(i | 7)] = 255;
				} else {
					if ((b < 0)) {
						drawEvents[(i | 7)] = 0;
					}
				}
				if ((a > 255)) {
					drawEvents[(i | 8)] = 255;
				} else {
					if ((a < 0)) {
						drawEvents[(i | 8)] = 0;
					}
				}
				break;
			case 3:
				r = drawEvents[(i | 6)];
				g = drawEvents[(i | 7)];
				b = drawEvents[(i | 8)];
				a = drawEvents[(i | 9)];
				if ((r > 255)) {
					drawEvents[(i | 6)] = 255;
				} else {
					if ((r < 0)) {
						drawEvents[(i | 6)] = 0;
					}
				}
				if ((g > 255)) {
					drawEvents[(i | 7)] = 255;
				} else {
					if ((g < 0)) {
						drawEvents[(i | 7)] = 0;
					}
				}
				if ((b > 255)) {
					drawEvents[(i | 8)] = 255;
				} else {
					if ((b < 0)) {
						drawEvents[(i | 8)] = 0;
					}
				}
				if ((a > 255)) {
					drawEvents[(i | 9)] = 255;
				} else {
					if ((a < 0)) {
						drawEvents[(i | 9)] = 0;
					}
				}
				break;
			case 4:
				r = drawEvents[(i | 7)];
				g = drawEvents[(i | 8)];
				b = drawEvents[(i | 9)];
				a = drawEvents[(i | 10)];
				if ((r > 255)) {
					drawEvents[(i | 7)] = 255;
				} else {
					if ((r < 0)) {
						drawEvents[(i | 7)] = 0;
					}
				}
				if ((g > 255)) {
					drawEvents[(i | 8)] = 255;
				} else {
					if ((g < 0)) {
						drawEvents[(i | 8)] = 0;
					}
				}
				if ((b > 255)) {
					drawEvents[(i | 9)] = 255;
				} else {
					if ((b < 0)) {
						drawEvents[(i | 9)] = 0;
					}
				}
				if ((a > 255)) {
					drawEvents[(i | 10)] = 255;
				} else {
					if ((a < 0)) {
						drawEvents[(i | 10)] = 0;
					}
				}
				break;
			case 5:
				r = drawEvents[(i | 9)];
				g = drawEvents[(i | 10)];
				b = drawEvents[(i | 11)];
				a = drawEvents[(i | 12)];
				if ((r > 255)) {
					drawEvents[(i | 9)] = 255;
				} else {
					if ((r < 0)) {
						drawEvents[(i | 9)] = 0;
					}
				}
				if ((g > 255)) {
					drawEvents[(i | 10)] = 255;
				} else {
					if ((g < 0)) {
						drawEvents[(i | 10)] = 0;
					}
				}
				if ((b > 255)) {
					drawEvents[(i | 11)] = 255;
				} else {
					if ((b < 0)) {
						drawEvents[(i | 11)] = 0;
					}
				}
				if ((a > 255)) {
					drawEvents[(i | 12)] = 255;
				} else {
					if ((a < 0)) {
						drawEvents[(i | 12)] = 0;
					}
				}
				break;
			case 8:
				r = drawEvents[(i | 10)];
				g = drawEvents[(i | 11)];
				b = drawEvents[(i | 12)];
				a = drawEvents[(i | 13)];
				if ((r > 255)) {
					drawEvents[(i | 10)] = 255;
				} else {
					if ((r < 0)) {
						drawEvents[(i | 10)] = 0;
					}
				}
				if ((g > 255)) {
					drawEvents[(i | 11)] = 255;
				} else {
					if ((g < 0)) {
						drawEvents[(i | 11)] = 0;
					}
				}
				if ((b > 255)) {
					drawEvents[(i | 12)] = 255;
				} else {
					if ((b < 0)) {
						drawEvents[(i | 12)] = 0;
					}
				}
				if ((a > 255)) {
					drawEvents[(i | 13)] = 255;
				} else {
					if ((a < 0)) {
						drawEvents[(i | 13)] = 0;
					}
				}
				break;
		}
		i += 16;
	}
	return vm[14];
};

var lib_graphics2d_scale = function(vm, args) {
	var objArray1 = null;
	var objArray2 = null;
	var objInstance1 = null;
	var objInstance2 = null;
	var arg2 = args[1];
	var arg3 = args[2];
	var arg4 = args[3];
	var arg5 = args[4];
	var arg6 = args[5];
	var int1 = arg3[1];
	var int2 = arg4[1];
	objInstance1 = arg5[1];
	var object1 = objInstance1[3][3];
	objInstance1 = arg6[1];
	objArray1 = PST$createNewArray(6);
	objInstance1[3] = objArray1;
	objArray1[0] = false;
	objArray1[1] = true;
	objArray1[2] = 0;
	objArray1[3] = C$drawing$scaleImage(object1, int1, int2);
	objArray1[4] = int1;
	objArray1[5] = int2;
	objInstance2 = arg2[1];
	objArray1 = PST$createNewArray(7);
	objInstance2[3] = objArray1;
	objInstance2 = args[0][1];
	objArray2 = objInstance2[3];
	var i = 4;
	while ((i >= 1)) {
		objArray1[i] = objArray2[i];
		i -= 1;
	}
	objArray1[5] = int1;
	objArray1[6] = int2;
	objInstance1 = arg6[1];
	objArray1[0] = objInstance1[3];
	return args[0];
};
