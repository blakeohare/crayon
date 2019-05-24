PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_nori_closeFrame = function(vm, args) {
	var frameObj = args[0][1];
	var nativeFrameHandle = frameObj[3][0];
	NoriHelper.CloseFrame(nativeFrameHandle);
	return vm[14];
};

var lib_nori_flushUpdatesToFrame = function(vm, args) {
	var frameObj = args[0][1];
	var nativeFrameHandle = frameObj[3][0];
	var data = args[1][1];
	NoriHelper.FlushUpdatesToFrame(nativeFrameHandle, data);
	return vm[14];
};

var lib_nori_prepImageResource = function(vm, args) {
	var frameWrapped = args[0][1];
	var frame = frameWrapped[3][0];
	var obj = args[2][1];
	var nativeImageData = obj[3][0];
	var id = args[1][1];
	var x = args[3][1];
	var y = args[4][1];
	var width = args[5][1];
	var height = args[6][1];
	NoriHelper.SendImageToRenderer(frame, id, nativeImageData, x, y, width, height);
	return vm[14];
};

var lib_nori_runEventWatcher = function(vm, args) {
	var frameObj = args[0][1];
	var execContextIdForResume = args[1][1];
	var eventCallback = args[2];
	var postShowCallback = args[3];
	var ec = getExecutionContext(vm, execContextIdForResume);
	vm_suspend_for_context(ec, 1);
	NoriHelper.EventWatcher(vm, execContextIdForResume, args[0], eventCallback, postShowCallback);
	return vm[14];
};

var lib_nori_showFrame = function(vm, args) {
	var frameObj = args[0][1];
	var title = args[1][1];
	var width = args[2][1];
	var height = args[3][1];
	var data = args[4][1];
	var execId = args[5][1];
	frameObj[3] = PST$createNewArray(1);
	frameObj[3][0] = NoriHelper.ShowFrame(args[0], title, width, height, data, execId);
	return vm[14];
};
