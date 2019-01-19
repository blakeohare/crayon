
function platformSpecificHandleEvent(id, args) {
	window.alert("Alert: " + id + " - " + args);
}

var NoriHelper = {};

NoriHelper.ShowFrame = function(crayonFrameValue, title, width, height, data, execId) {
	var ch = document.getElementById('crayon_host');
	
	while (ch.lastChild) {
		ch.removeChild(ch.lastChild);
	}
	
	var nh = document.createElement('div');
	nh.style.width = '100%';
	nh.style.height = '100%';
	nh.crayon_execId = execId;
	nh.crayon_frameValue = crayonFrameValue;
	
	flushData(data);
	
	return nh;
};

NoriHelper.CloseFrame = function(frameHandle) {
	while (frameHandle.lastChild) {
		frameHandle.removeChild(frameHandle.lastChild);
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
