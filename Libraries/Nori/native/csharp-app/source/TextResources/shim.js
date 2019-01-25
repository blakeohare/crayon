
function platformSpecificHandleEvent(id, eventName, args) {
	window.external.SendEventToCSharp(id, eventName, args);
}

function winFormsGetWindowSize() {
	var width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
	var height = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
	return [width, height];
}

function shimInit(uiData) {
	var noriRoot = document.getElementById('nori_root');
	setFrameRoot(noriRoot);
	var sz = winFormsGetWindowSize();
	setFrameSize(sz[0], sz[1]);
	window.onresize = function() {
		var newSize = winFormsGetWindowSize();
		setFrameSize(newSize[0], newSize[1]);
		platformSpecificHandleEvent(-1, 'frame.onresize', newSize[0] + ',' + newSize[1]);
		flushUpdates('NO,0');
	};
	winFormsNoriHandleNewUiData(uiData);
}

function winFormsNoriHandleNewUiData(data) {
	flushUpdates(data);
}

