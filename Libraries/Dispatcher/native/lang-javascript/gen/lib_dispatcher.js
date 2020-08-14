PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_dispatcher_flushNativeQueue = function(vm, args) {
	var nd = (args[0][1])[3];
	var output = [];
	LIB$dispatcher$flushNativeQueue(nd, output);
	if ((output.length == 0)) {
		return vm[14];
	}
	return buildList(output);
};

var lib_dispatcher_initNativeQueue = function(vm, args) {
	var obj = args[0][1];
	var nd = PST$createNewArray(2);
	nd[0] = null;
	nd[1] = [];
	obj[3] = nd;
	return vm[14];
};
