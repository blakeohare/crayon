var lib_userdata_getProjectSandboxDirectory = function(vm, args) {
	var output = vm[14];
	var arg1 = args[0];
	var string1 = arg1[1];
	var string2 = '/' + string1;
	output = buildString(vm[13], string2);
	return output;
};
