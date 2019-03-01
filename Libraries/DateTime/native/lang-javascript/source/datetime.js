LIB$datetime$utcTimeZone = {
	name: "utc",
	isLocal: false,
	observesDst: false
};

LIB$datetime$extractTimeZone = function(nullableTz) {
	if (nullableTz === null) return LIB$datetime$utcTimeZone;
	return nullableTz;
};

LIB$datetime$getDataForLocalTimeZone = function(strOut, intOut) {
	var name = Intl.DateTimeFormat().resolvedOptions().timeZone;
	strOut[0] = name;
	strOut[1] = name;
	intOut[0] = -1; // TODO: remove this
	var now = new Date();
	var janSeconds = new Date(now.getFullYear(), 1, 1, 0, 0, 0).getTime() / 1000;
	var junSeconds = new Date(now.getFullYear(), 6, 1, 0, 0, 0).getTime() / 1000;
	var janToJunHours = (junSeconds - janSeconds) / 3600;
	var dstOccurs = (janToJunHours % 24) != 0;
	intOut[1] = dstOccurs ? 1 : 0;
	return {
		name: name,
		isLocal: true,
		observesDst: dstOccurs
	};
};

LIB$datetime$unixToStructured = function(intOut, nullableTimeZone, unixTime) {
	var unixTimeInt = Math.floor(unixTime);
	var micros = Math.floor(1000000 * (unixTime - unixTimeInt));
	var millis = Math.floor(micros / 1000);
	var tz = LIB$datetime$extractTimeZone(nullableTimeZone);
	var d = new Date(Math.floor(unixTime * 1000));
	intOut[6] = millis;
	intOut[7] = micros;
	if (tz.isLocal) {
		intOut[0] = d.getFullYear();
		intOut[1] = d.getMonth() + 1;
		intOut[2] = d.getDate();
		intOut[3] = d.getHours();
		intOut[4] = d.getMinutes();
		intOut[5] = d.getSeconds();
		intOut[8] = d.getDay() + 1;
	} else {
		var p = d.toISOString().split('Z')[0].split('T');
		var pd = p[0].split('-');
		var pt = p[1].split('.')[0].split(':');
		
		intOut[0] = parseInt(pd[0]);
		intOut[1] = parseInt(pd[1]);
		intOut[2] = parseInt(pd[2]);
		intOut[3] = parseInt(pt[0]);
		intOut[4] = parseInt(pt[1]);
		intOut[5] = parseInt(pt[2]);
		intOut[8] = new Date(intOut[0], intOut[1] - 1, intOut[2], 12, 0, 0).getDay() + 1;
	}
	return true;
};

LIB$datetime$pad = function(n, length) {
	n = n + '';
	while (n.length < length) {
		n = '0' + n;
	}
	return n;
};
LIB$datetime$parseDate = function(intOut, nullableTimeZone, year, month, day, hour, minute, microseconds) {
	var tz = LIB$datetime$extractTimeZone(nullableTimeZone);
	intOut[2] = microseconds % 1000000;
	intOut[0] = 1;
	var seconds = Math.floor((microseconds - intOut[2]) / 10000000);
	if (tz.isLocal) {
		var d = new Date(year, month, day, hour, minute, seconds);
		intOut[1] = Math.floor(d.getTime() / 1000);
	} else {
		intOut[1] = new Date([
			year, "-",
			LIB$datetime$pad(month, 2), "-",
			LIB$datetime$pad(day, 2), "T",
			LIB$datetime$pad(hour, 2), ":",
			LIB$datetime$pad(minute, 2), ":",
			LIB$datetime$pad(seconds, 2), "Z"].join('')).getTime();
	}
};