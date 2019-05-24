var lib_web_launch_browser = function(vm, args) {
	var url = args[0][1];
	window.open(url);
	return vm[14];
};
