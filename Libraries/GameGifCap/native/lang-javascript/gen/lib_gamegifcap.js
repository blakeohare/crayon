var lib_gamegifcap_createGifContext = function(vm, args) {
	var ms = args[1][1];
	var oi = args[0][1];
	oi[3][0] = {};
	return vm[14];
};

var lib_gamegifcap_isSupported = function(vm, args) {
	if (C$common$alwaysFalse()) {
		return vm[15];
	}
	return vm[16];
};

var lib_gamegifcap_saveToDisk = function(vm, args) {
	var oi = args[0][1];
	var ctx = oi[3][0];
	var path = args[1][1];
	var sc = 0;
	return buildInteger(vm[13], sc);
};

var lib_gamegifcap_screenCap = function(vm, args) {
	var oiCtx = args[0][1];
	var oiGw = args[1][1];
	var sc = 0;
	return buildInteger(vm[13], sc);
};

var lib_gamegifcap_setRecordSize = function(vm, args) {
	var oi = args[0][1];
	var w = args[1][1];
	var h = args[2][1];
	C$common$alwaysFalse();
	return vm[14];
};
