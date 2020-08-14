PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
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

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_datetime_getNativeTimezone = function(value) {
	var tzObj = value[1];
	if ((tzObj[3] == null)) {
		return null;
	}
	return tzObj[3][0];
};

var lib_datetime_getUtcOffsetAt = function(vm, args) {
	var nativeTz = lib_datetime_getNativeTimezone(args[0]);
	var unixTime = args[1][1];
	var offsetSeconds = LIB$datetime$getUtcOffsetAt(nativeTz, unixTime);
	return buildInteger(vm[13], offsetSeconds);
};

var lib_datetime_initTimeZone = function(vm, args) {
	var timezone = args[0][1];
	timezone[3] = PST$createNewArray(1);
	var nativeTzRef = null;
	var readableName = null;
	var offsetFromUtc = 0;
	var isDstObserved = 0;
	var fingerprint = null;
	if ((args[1][0] == 1)) {
		var strOut = PST$stringBuffer16;
		var intOut = PST$intBuffer16;
		nativeTzRef = LIB$datetime$getDataForLocalTimeZone(strOut, intOut);
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

var lib_datetime_initTimeZoneList = function(vm, args) {
	var obj = args[0][1];
	obj[3] = PST$createNewArray(1);
	var timezones = LIB$datetime$initializeTimeZoneList();
	obj[3][0] = timezones;
	var length = timezones.length;
	return buildInteger(vm[13], length);
};

var lib_datetime_isDstOccurringAt = function(vm, args) {
	var nativeTz = lib_datetime_getNativeTimezone(args[0]);
	var unixtime = args[1][1];
	return buildBoolean(vm[13], LIB$datetime$isDstOccurringAt(nativeTz, unixtime));
};

var lib_datetime_parseDate = function(vm, args) {
	var year = args[0][1];
	var month = args[1][1];
	var day = args[2][1];
	var hour = args[3][1];
	var minute = args[4][1];
	var microseconds = args[5][1];
	var nullableTimeZone = lib_datetime_getNativeTimezone(args[6]);
	if (((year >= 1970) && (year < 2100) && (month >= 1) && (month <= 12) && (day >= 1) && (day <= 31) && (hour >= 0) && (hour < 24) && (minute >= 0) && (minute < 60) && (microseconds >= 0) && (microseconds < 60000000))) {
		var intOut = PST$intBuffer16;
		LIB$datetime$parseDate(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds);
		if ((intOut[0] == 1)) {
			var unixFloat = (intOut[1] + (intOut[2] / 1000000.0));
			return buildFloat(vm[13], unixFloat);
		}
	}
	return vm[14];
};

var lib_datetime_unixToStructured = function(vm, args) {
	var unixTime = args[0][1];
	var nullableTimeZone = lib_datetime_getNativeTimezone(args[1]);
	var output = [];
	var intOut = PST$intBuffer16;
	var success = LIB$datetime$unixToStructured(intOut, nullableTimeZone, unixTime);
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
