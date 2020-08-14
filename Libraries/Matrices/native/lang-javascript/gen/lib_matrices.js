PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_matrices_addMatrix = function(vm, args) {
	var obj = args[0][1];
	var nd1 = obj[3];
	if (!args[2][1]) {
		nd1[5] = "Input must be a matrix";
		return vm[14];
	}
	var left = nd1[0];
	var width = nd1[1];
	var height = nd1[2];
	obj = args[1][1];
	var nd2 = obj[3];
	var right = nd2[0];
	if (((nd2[1] != width) || (nd2[2] != height))) {
		nd1[5] = "Matrices must be the same size.";
		return vm[14];
	}
	var output = left;
	var isInline = (args[3][0] == 1);
	if (isInline) {
		nd1[4] = true;
	} else if (!args[4][1]) {
		nd1[5] = "Output value must be a matrix";
		return vm[14];
	} else {
		obj = args[3][1];
		var nd3 = obj[3];
		output = nd3[0];
		if (((nd3[1] != width) || (nd3[2] != height))) {
			nd1[5] = "Output matrix must have the same size as the inputs.";
			return vm[14];
		}
		nd3[4] = true;
	}
	var length = (width * height);
	var i = 0;
	while ((i < length)) {
		output[i] = (left[i] + right[i]);
		i += 1;
	}
	return args[0];
};

var lib_matrices_copyFrom = function(vm, args) {
	var obj = args[0][1];
	var nd1 = obj[3];
	obj = args[1][1];
	var nd2 = obj[3];
	if (!args[2][1]) {
		nd1[5] = "value was not a matrix";
		return vm[14];
	}
	if (((nd1[1] != nd2[1]) || (nd1[2] != nd2[2]))) {
		nd1[5] = "Matrices were not the same size.";
		return vm[14];
	}
	var target = nd1[0];
	var source = nd2[0];
	var _len = target.length;
	var i = 0;
	while ((i < _len)) {
		target[i] = source[i];
		i += 1;
	}
	return args[0];
};

var lib_matrices_getError = function(vm, args) {
	var obj = args[0][1];
	return buildString(vm[13], obj[3][5]);
};

var lib_matrices_getValue = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	if (((args[1][0] != 3) || (args[2][0] != 3))) {
		nd[5] = "Invalid coordinates";
		return vm[14];
	}
	var x = args[1][1];
	var y = args[2][1];
	var width = nd[1];
	var height = nd[2];
	if (((x < 0) || (x >= width) || (y < 0) || (y >= height))) {
		nd[5] = "Coordinates out of range.";
		return vm[14];
	}
	var valueArray = nd[3];
	if (!nd[4]) {
		var data = nd[0];
		var length = (width * height);
		var i = 0;
		while ((i < length)) {
			valueArray[i] = buildFloat(vm[13], data[i]);
			i += 1;
		}
	}
	return valueArray[((width * y) + x)];
};

var lib_matrices_initMatrix = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	if (((args[1][0] != 3) || (args[2][0] != 3))) {
		nd[5] = "Width and height must be integers.";
		return vm[15];
	}
	var width = args[1][1];
	var height = args[2][1];
	var size = (width * height);
	var data = PST$createNewArray(size);
	nd[0] = data;
	nd[1] = width;
	nd[2] = height;
	nd[3] = PST$createNewArray(size);
	nd[4] = false;
	nd[5] = "";
	nd[6] = PST$createNewArray(size);
	var i = 0;
	while ((i < size)) {
		data[i] = 0.0;
		i += 1;
	}
	return vm[16];
};

var lib_matrices_multiplyMatrix = function(vm, args) {
	var obj = args[0][1];
	var nd1 = obj[3];
	if (!args[2][1]) {
		nd1[5] = "argument must be a matrix";
		return vm[14];
	}
	obj = args[1][1];
	var nd2 = obj[3];
	var isInline = false;
	if ((args[3][0] == 1)) {
		isInline = true;
	} else if (!args[4][1]) {
		nd1[5] = "output matrix was unrecognized type.";
		return vm[14];
	}
	var m1width = nd1[1];
	var m1height = nd1[2];
	var m2width = nd2[1];
	var m2height = nd2[2];
	var m3width = m2width;
	var m3height = m1height;
	if ((m1width != m2height)) {
		nd1[5] = "Matrix size mismatch";
		return vm[14];
	}
	var m1data = nd1[0];
	var m2data = nd2[0];
	var nd3 = null;
	if (isInline) {
		nd3 = nd1;
		if ((m2width != m2height)) {
			nd1[5] = "You can only multiply a matrix inline with a square matrix.";
			return vm[14];
		}
	} else {
		obj = args[3][1];
		nd3 = obj[3];
		if (((nd3[1] != m3width) || (nd3[2] != m3height))) {
			nd1[5] = "Output matrix is incorrect size.";
			return vm[14];
		}
	}
	nd3[4] = true;
	var m3data = nd3[6];
	var x = 0;
	var y = 0;
	var i = 0;
	var m1index = 0;
	var m2index = 0;
	var m3index = 0;
	var value = 0.0;
	y = 0;
	while ((y < m3height)) {
		x = 0;
		while ((x < m3width)) {
			value = 0.0;
			m1index = (y * m1height);
			m2index = x;
			i = 0;
			while ((i < m1width)) {
				value += (m1data[m1index] * m2data[m2index]);
				m1index += 1;
				m2index += m2width;
				i += 1;
			}
			m3data[m3index] = value;
			m3index += 1;
			x += 1;
		}
		y += 1;
	}
	var t = nd3[0];
	nd3[0] = nd3[6];
	nd3[6] = t;
	return args[0];
};

var lib_matrices_multiplyScalar = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	var isInline = (args[2][0] == 1);
	var m1data = nd[0];
	var m2data = m1data;
	if (isInline) {
		nd[4] = true;
	} else if (!args[3][1]) {
		nd[5] = "output must be a matrix instance";
		return vm[14];
	} else {
		obj = args[2][1];
		var nd2 = obj[3];
		if (((nd[1] != nd2[1]) || (nd[2] != nd2[2]))) {
			nd[5] = "output matrix must be the same size.";
			return vm[14];
		}
		m2data = nd2[0];
		nd2[4] = true;
	}
	var scalar = 0.0;
	if ((args[1][0] == 4)) {
		scalar = args[1][1];
	} else if ((args[1][0] == 3)) {
		scalar = (0.0 + args[1][1]);
	} else {
		nd[5] = "scalar must be a number";
		return vm[14];
	}
	var i = 0;
	var length = m1data.length;
	i = 0;
	while ((i < length)) {
		m2data[i] = (m1data[i] * scalar);
		i += 1;
	}
	return args[0];
};

var lib_matrices_setValue = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	if (((args[1][0] != 3) || (args[2][0] != 3))) {
		nd[5] = "Invalid coordinates";
		return vm[14];
	}
	var x = args[1][1];
	var y = args[2][1];
	var width = nd[1];
	var height = nd[2];
	if (((x < 0) || (x >= width) || (y < 0) || (y >= height))) {
		nd[5] = "Coordinates out of range.";
		return vm[14];
	}
	var value = 0.0;
	if ((args[3][0] == 4)) {
		value = args[3][1];
	} else if ((args[3][0] == 3)) {
		value = (0.0 + args[3][1]);
	} else {
		nd[5] = "Value must be a number.";
		return vm[14];
	}
	var index = ((y * width) + x);
	var data = nd[0];
	var valueArray = nd[3];
	data[index] = value;
	valueArray[index] = buildFloat(vm[13], value);
	return args[0];
};

var lib_matrices_toVector = function(vm, args) {
	var obj = args[0][1];
	var nd = obj[3];
	var data = nd[0];
	var width = nd[1];
	var height = nd[2];
	var length = (width * height);
	if ((args[1][0] != 6)) {
		nd[5] = "Output argument must be a list";
		return vm[14];
	}
	var output = args[1][1];
	while ((output[1] < length)) {
		addToList(output, vm[14]);
	}
	var value = 0.0;
	var toList = null;
	var i = 0;
	while ((i < length)) {
		value = data[i];
		if ((value == 0)) {
			toList = vm[13][6];
		} else if ((value == 1)) {
			toList = vm[13][7];
		} else {
			toList = [4, data[i]];
		}
		output[2][i] = toList;
		i += 1;
	}
	return args[1];
};
