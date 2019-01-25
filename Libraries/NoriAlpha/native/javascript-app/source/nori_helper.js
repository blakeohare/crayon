
function platformSpecificHandleEvent(id, args) {
	window.alert("Alert: " + id + " - " + args);
}

var NoriHelper = {};

NoriHelper.getCrayonHost = function() {
	return document.getElementById('crayon_host');
};

NoriHelper.ShowFrame = function(crayonFrameValue, title, width, height, data, execId) {
	var ch = NoriHelper.getCrayonHost();
	
	while (ch.lastChild) {
		ch.removeChild(ch.lastChild);
	}
	
	ch.style.height = '100%';
	ch.style.backgroundColor = '#ff0000';
	
	width = window.innerWidth;
	height = window.innerHeight;
	
	// for browsers, width and height are ignored.
	ch.style.width = width + 'px';
	ch.style.height = height + 'px';
	
	var body = ch.parentElement;
	body.style.margin = '0px';
	
	var nh = document.createElement('div');
	nh.style.width = '100%';
	nh.style.height = '100%';
	nh.style.position = 'relative';
	nh.crayon_execId = execId;
	nh.crayon_frameValue = crayonFrameValue;
	
	// TODO: wrap all contents of nori.js into a single object instance.
	ch.appendChild(nh);
	setFrameRoot(nh);
	setFrameSize(width, height);
	
	// update flushing must occur AFTER the elements are added to the document.
	// Some of the layout stuff (such as text size calculation) requires the elements
	// to be in the document.
	flushUpdates(data);
	
	return nh;
};

NoriHelper.CloseFrame = function(frameHandle) {
	var ch = NoriHelper.getCrayonHost();
	
	while (ch.lastChild) {
		ch.removeChild(ch.lastChild);
	}
};

NoriHelper.FlushUpdatesToFrame = function(frameHandle, uiData) {
	var ch = document.getElementById('crayon_host');
	if (ch.firstChild !== frameHandle) throw "Multiple frames not supported";
	// TODO: open iframes to determine proper frame. For now, assume one frame.
	flushData(uiData);
};

NoriHelper.HEX = '0123456789ABCDEF';
NoriHelper.EscapeStringHex = function(original) {
	var output = [];
	var h = NoriHelper.HEX;
	var len = original.length;
	var c;
	for (var i = 0; i < len; ++i) {
		c = original.charCodeAt(i);
		output.push(h.charAt((c >> 4) & 255));
		output.push(h.charAt(c & 15));
	}
	var t= output.join('');
	return t;
};

NoriHelper.EventWatcher = function(vm, execContextIdForResume, eventCallback) {
	// There is nothing to do here, yet.
};
