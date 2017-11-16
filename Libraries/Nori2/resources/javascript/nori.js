LIB$nori$makeWindow = function(title, width, height) {
	var host = document.getElementById('crayon_host');
	host.style.width = width + 'px';
	host.style.height = height + 'px';
	host.style.backgroundColor = '#cccccc';
	host.style.position = 'relative';
	document.title = title;
	return [host, -1];
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
	var left = properties[0];
	var top = properties[1];
	var width = properties[2];
	var height = properties[3];
	var red = properties[4];
	var green = properties[5];
	var blue = properties[6];
	var alpha = properties[7];
	
	var obj = null;
	var s;
	switch (type) {
		case 1:
			obj = document.createElement("div");
			// TODO: alpha
			obj.style.backgroundColor = LIB$nori$toColor(red, green, blue);
			break;
		case 3:
			obj = document.createElement("div");
			break;
		case 4:
			obj = document.createElement("div");
			var btn = document.createElement("button");
			btn.innerHTML = properties[8]; // misc_string_0
			obj.appendChild(btn);
			s = btn.style;
			s.width = '100%';
			s.height = '100%';
			break;
		default:
			throw 'not implemented';
	}
	
	s = obj.style;
	s.position = 'absolute';
	s.left = left + 'px';
	s.top = top + 'px';
	s.width = width + 'px';
	s.height = height + 'px';
	return obj;
};

LIB$nori$instantiateWindow = function(properties) {
	var title = properties[0];
	var width = properties[1];
	var height = properties[2];
	return LIB$nori$makeWindow(title, width, height);
};

LIB$nori$invalidateElementProperty = function(type, element, key, value) {

	throw 'not implemented - invalidateElementProperty';
};

LIB$nori$invalidateWindowProperty = function(window, key, value) {

	throw 'not implemented - invalidateWindowProperty';
};

LIB$nori$showWindow = function(window, properties, rootElement, execId) {
	window[0].appendChild(rootElement);
	window[1] = execId;
};

LIB$nori$updateLayout = function(element, typeId, x, y, width, height) {
	var s = element.style;
	s.left = x + 'px';
	s.top = y + 'px';
	s.width = width + 'px';
	s.height = height + 'px';
};
