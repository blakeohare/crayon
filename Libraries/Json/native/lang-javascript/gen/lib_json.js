PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_json_parse = function(vm, args) {
	var raw = args[0][1];
	if ((raw.length > 0)) {
		var output = LIB$json$parseJson(vm[13], raw);
		if ((output != null)) {
			return output;
		}
	}
	return vm[14];
};
