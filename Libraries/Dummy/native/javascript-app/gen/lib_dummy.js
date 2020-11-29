PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_dummy_lol = function(vm) {
	return 42;
};
