LIB$nori$makeWindow = function(title, width, height) {
	var host = document.getElementById('crayon_host');
	host.style.width = width + 'px';
	host.style.height = height + 'px';
	host.style.backgroundColor = '#cccccc';
	host.style.position = 'relative';
	document.title = title;
	return [host];
};

var t = '0123456789abcdef';
var LIB$nori$HEX_LOOKUP = [];
for (var i = 0; i < 256; ++i) {
	LIB$nori$HEX_LOOKUP.push(t.charAt(Math.floor(i / 16)) + t.charAt(i % 16));
}
LIB$nori$toColor = function(r, g, b) {
	return '#' + LIB$nori$HEX_LOOKUP[r] + LIB$nori$HEX_LOOKUP[g] + LIB$nori$HEX_LOOKUP[b];
};

LIB$nori$addChildToParent = function(child, parent) {
	parent.appendChild(child);
};

LIB$nori$closeWindow = function(window) {
	throw 'not implemented - closeWindow';
};

LIB$nori$ensureParentLinkOrder = function(parent, children) {
	var validUntil = 0;
	var index = 0;
	var actualChildren = parent.childNodes;
	for (var i = 0; i < actualChildren.length; ++i) {
		var child = parent.childNodes[i];
		if (child == children[i]) {
			validUntil = i + 1;
		} else {
			break;
		}
	}
	if (validUntil == children.length) {
		return;
	}
	throw 'parent link order not valid -- TODO: implement this';
};

LIB$nori$instantiateElement = function(type, properties) {
	var left = 0;
	var top = 0;
	var width = 0;
	var height = 0;
	var red = 0;
	var green = 0;
	var blue = 0;
	var alpha = 255;
	
	if (properties[0] !== null) {
		left = properties[0];
		top = properties[1];
		width = properties[2];
		height = properties[3];
	}
	if (properties[16] !== null) {
		var colors = properties[16];
		red = colors[0][1];
		green = colors[1][1];
		blue = colors[2][1];
		// TODO: alpha
	}
	
	var obj = null;
	switch (type) {
		case 1:
			obj = document.createElement("div");
			// TODO: alpha
			obj.style.backgroundColor = LIB$nori$toColor(red, green, blue);
			break;
		case 3:
			obj = document.createElement("div");
			break;
		default:
			throw 'not implemented';
	}
	
	var s = obj.style;
	s.position = 'absolute';
	s.left = left + 'px';
	s.top = top + 'px';
	s.width = width + 'px';
	s.height = height + 'px';
	return obj;
};

LIB$nori$instantiateWindow = function(properties) {
	var title = properties[0];
	var width = properties[1][0][1];
	var height = properties[1][0][1];
	return LIB$nori$makeWindow(title, width, height);
};

LIB$nori$invalidateElementProperty = function(type, element, key, value) {

	throw 'not implemented - invalidateElementProperty';
};

LIB$nori$invalidateWindowProperty = function(window, key, value) {

	throw 'not implemented - invalidateWindowProperty';
};

LIB$nori$showWindow = function(window, properties, rootElement) {
	window[0].appendChild(rootElement);
};

LIB$nori$updateLayout = function(element, typeId, x, y, width, height) {
	var s = element.style;
	s.left = x + 'px';
	s.top = y + 'px';
	s.width = width + 'px';
	s.height = height + 'px';
};
