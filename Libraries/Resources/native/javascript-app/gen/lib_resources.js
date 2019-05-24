var lib_resources_getResourceData = function(vm, args) {
	var output = buildList(vm[9][2]);
	vm[9][2] = null;
	return output;
};

var lib_resources_readText = function(vm, args) {
	var string1 = C$common$readResourceText(args[0][1]);
	return buildString(vm[13], string1);
};
