PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_cryptocommon_addBytes = function(vm, args) {
	var obj = args[0][1];
	var fromByteList = args[1][1];
	var toByteList = obj[3][0];
	var length = fromByteList[1];
	var i = 0;
	while ((i < length)) {
		toByteList.push(fromByteList[2][i][1]);
		i += 1;
	}
	return vm[16];
};

var lib_cryptocommon_initHash = function(vm, args) {
	var obj = args[0][1];
	obj[3] = PST$createNewArray(1);
	obj[3][0] = [];
	return vm[14];
};
