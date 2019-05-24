var lib_random_random_bool = function(vm, args) {
	if ((Math.random() < 0.5)) {
		return vm[15];
	}
	return vm[16];
};

var lib_random_random_float = function(vm, args) {
	return [4, Math.random()];
};

var lib_random_random_int = function(vm, args) {
	if (((args[0][0] != 3) || (args[1][0] != 3))) {
		return vm[14];
	}
	var lower = args[0][1];
	var upper = args[1][1];
	if ((lower >= upper)) {
		return vm[14];
	}
	var value = Math.floor(((Math.random() * (upper - lower))));
	return buildInteger(vm[13], (lower + value));
};
