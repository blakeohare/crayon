
function platformSpecificHandleEvent(id, eventName, args) {
	window.external.SendEventToCSharp(id, eventName, args);
}

function winFormsGetWindowSize() {
	var width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
	var height = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
	return [width, height];
}

function getImageInHoldingArea(id) {
	var img = winFormsImages[id];
	delete winFormsImages[id];
	return img;
}

var winFormsImages = {};
function winFormsPrepImageData(id, width, height, dataUri) {
	var canvas = document.createElement('canvas');
	canvas.NORI_canvas_needs_loading = true;
	var img = new Image();
	img.src = dataUri;
	canvas.width = width;
	canvas.height = height;
	canvas.NORI_canvas_load_callback_here = null;
	img.onload = function () {
		var ctx = canvas.getContext('2d');
		ctx.drawImage(img, 0, 0);
		canvas.NORI_canvas_needs_loading = false;
		if (canvas.NORI_canvas_load_callback_here !== null) {
			canvas.NORI_canvas_load_callback_here();
		}
	};
	winFormsImages[id] = canvas;
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

