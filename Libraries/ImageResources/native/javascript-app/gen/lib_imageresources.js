PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_imageresources_blit = function(vm, args) {
	var object1 = null;
	var objInstance1 = args[0][1];
	var objInstance2 = args[1][1];
	LIB$imageresources$imageResourceBlitImage(objInstance1[3][0], objInstance2[3][0], args[2][1], args[3][1], args[4][1], args[5][1], args[6][1], args[7][1]);
	return vm[14];
};

var lib_imageresources_checkLoaderIsDone = function(vm, args) {
	var objInstance1 = args[0][1];
	var objInstance2 = args[1][1];
	var status = LIB$imageresources$checkLoaderIsDone(objInstance1[3], objInstance2[3]);
	return buildInteger(vm[13], status);
};

var lib_imageresources_flushImageChanges = function(vm, args) {
	return vm[14];
};

var lib_imageresources_getManifestString = function(vm, args) {
	return buildString(vm[13], LIB$imageresources$getImageResourceManifest());
};

var lib_imageresources_loadAsynchronous = function(vm, args) {
	var objInstance1 = args[0][1];
	var filename = args[1][1];
	var objInstance2 = args[2][1];
	var objArray1 = PST$createNewArray(3);
	objInstance1[3] = objArray1;
	var objArray2 = PST$createNewArray(4);
	objArray2[2] = 0;
	objInstance2[3] = objArray2;
	LIB$imageresources$imageLoad(filename, objArray1, objArray2);
	return vm[14];
};

var lib_imageresources_nativeImageDataInit = function(vm, args) {
	var objInstance1 = args[0][1];
	var nd = PST$createNewArray(4);
	var width = args[1][1];
	var height = args[2][1];
	nd[0] = LIB$imageresources$generateNativeBitmapOfSize(width, height);
	nd[1] = width;
	nd[2] = height;
	nd[3] = null;
	objInstance1[3] = nd;
	return vm[14];
};
