PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_processutil_isSupported = function(vm, args) {
	var t = C$common$alwaysFalse();
	return buildBoolean(vm[13], t);
};

var lib_processutil_launchProcess = function(vm, args) {
	var bridge = args[0][1];
	bridge[3] = PST$createNewArray(5);
	bridge[3][0] = true;
	bridge[3][1] = 0;
	bridge[3][2] = [];
	bridge[3][3] = [];
	bridge[3][4] = null;
	var execName = args[1][1];
	var argsRaw = args[2][1];
	var isAsync = args[3][1];
	var cb = args[4];
	var dispatcherQueue = args[5][1];
	var argStrings = [];
	var i = 0;
	while ((i < argsRaw[1])) {
		var a = getItemFromList(argsRaw, i);
		argStrings.push(a[1]);
		i += 1;
	}
	C$common$alwaysFalse();
	return vm[14];
};

var lib_processutil_readBridge = function(vm, args) {
	var bridge = args[0][1];
	var outputList = args[1][1];
	var type = args[2][1];
	var mtx = bridge[3][4];
	if ((type == 1)) {
		var outputInt = C$common$alwaysFalse();
		addToList(outputList, buildInteger(vm[13], outputInt));
	} else {
		var output = [];
		C$common$alwaysFalse();
		var i = 0;
		while ((i < output.length)) {
			addToList(outputList, buildString(vm[13], output[i]));
			i += 1;
		}
	}
	return vm[14];
};
