PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

var lib_imagewebresources_bytesToImage = function(vm, args) {
	var objInstance1 = args[0][1];
	var object1 = objInstance1[3][0];
	var list1 = args[1][1];
	var value = getItemFromList(list1, 0);
	var objArray1 = PST$createNewArray(3);
	objInstance1 = value[1];
	objInstance1[3] = objArray1;
	if (C$common$alwaysTrue()) {
		var width = buildInteger(vm[13], objArray1[1]);
		var height = buildInteger(vm[13], objArray1[2]);
		list1[2][1] = width;
		list1[2][2] = height;
		return vm[15];
	}
	return vm[16];
};

var lib_imagewebresources_jsDownload = function(vm, args) {
	var url = args[0][1];
	var objInstance1 = args[1][1];
	var objArray1 = PST$createNewArray(5);
	objInstance1[3] = objArray1;
	objArray1[0] = false;
	LIB$imagewebresources$download(url, objArray1);
	return vm[14];
};

var lib_imagewebresources_jsGetImage = function(vm, args) {
	var objInstance1 = args[0][1];
	var objArray1 = objInstance1[3];
	var list1 = args[1][1];
	if (objArray1[1]) {
		var object1 = objArray1[2];
		var value = getItemFromList(list1, 0);
		objInstance1 = value[1];
		var objArray2 = PST$createNewArray(3);
		objInstance1[3] = objArray2;
		var width = objArray1[3];
		var height = objArray1[4];
		objArray2[0] = object1;
		objArray2[1] = width;
		objArray2[2] = height;
		clearList(list1);
		addToList(list1, value);
		addToList(list1, buildInteger(vm[13], width));
		addToList(list1, buildInteger(vm[13], height));
		return vm[15];
	}
	return vm[16];
};

var lib_imagewebresources_jsPoll = function(vm, args) {
	var objInstance1 = args[0][1];
	if (objInstance1[3][0]) {
		return vm[15];
	}
	return vm[16];
};
