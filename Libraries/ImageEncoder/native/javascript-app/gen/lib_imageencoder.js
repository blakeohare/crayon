PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_imageencoder_encodeToBytes = function(vm, args) {
	var platformBitmap = getNativeDataItem(args[0], 0);
	var imageFormat = args[1][1];
	var byteOutputList = [];
	var statusCode = LIB$imageencoder$encodeImage(platformBitmap, imageFormat, byteOutputList, vm[13][9]);
	var length = byteOutputList.length;
	var finalOutputList = args[2][1];
	var i = 0;
	while ((i < length)) {
		addToList(finalOutputList, byteOutputList[i]);
		i += 1;
	}
	return buildInteger(vm[13], statusCode);
};
