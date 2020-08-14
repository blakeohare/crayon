PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_srandom_getBoolean = function(vm, args) {
	var intPtr = args[0][1];
	var value = 0;
	value = (((intPtr[2][0][1] * 20077) + 12345) & 65535);
	intPtr[2][0] = buildInteger(vm[13], value);
	if (((value & 1) == 0)) {
		return vm[16];
	}
	return vm[15];
};

var lib_srandom_getFloat = function(vm, args) {
	var intPtr = args[0][1];
	var value1 = 0;
	value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535);
	var value2 = (((value1 * 20077) + 12345) & 65535);
	var value3 = (((value2 * 20077) + 12345) & 65535);
	intPtr[2][0] = buildInteger(vm[13], value3);
	value1 = ((value1 >> 8) & 255);
	value2 = ((value2 >> 8) & 255);
	value3 = ((value3 >> 8) & 255);
	return buildFloat(vm[13], (((value1 << 16) | (value2 << 8) | value3) / 16777216.0));
};

var lib_srandom_getInteger = function(vm, args) {
	var intPtr = args[0][1];
	var value1 = 0;
	value1 = (((intPtr[2][0][1] * 20077) + 12345) & 65535);
	var value2 = (((value1 * 20077) + 12345) & 65535);
	var value3 = (((value2 * 20077) + 12345) & 65535);
	var value4 = (((value3 * 20077) + 12345) & 65535);
	intPtr[2][0] = buildInteger(vm[13], value4);
	value1 = ((value1 >> 8) & 255);
	value2 = ((value2 >> 8) & 255);
	value3 = ((value3 >> 8) & 255);
	value4 = ((value4 >> 8) & 127);
	return buildInteger(vm[13], ((value4 << 24) | (value3 << 16) | (value2 << 8) | value1));
};
