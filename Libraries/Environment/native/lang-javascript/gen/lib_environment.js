PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_environment_get = function(vm, args) {
	var value = null;
	if ((value == null)) {
		return vm[14];
	}
	return buildString(vm[13], value);
};
