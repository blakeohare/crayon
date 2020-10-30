
// The events are sent back in batches intead of individually since something like
// a mouse move handler can easily bombard the VM with callbacks.
let msgQueue = [];
let msgQueueLastFlush = 0;
let enqueuedFlushes = [];
const msgQueueDelay = Math.floor(1000 / 30);
function platformSpecificHandleEvent(id, eventName, args) {
	msgQueue.push('E', id, eventName, args);
	flushMessageQueue();
}

function flushMessageQueue() {
	let now = Date.now();
	if (now - msgQueueLastFlush < msgQueueDelay) {
		let nextSlot = msgQueueLastFlush + msgQueueDelay + 1;
		enqueuedFlushes.push(window.setTimeout(() => { flushMessageQueue() }, nextSlot - now));
	} else {
		if (msgQueue.length > 0) {
			window.sendMessage(msgQueue.join(' '));
			msgQueue = [];
			msgQueueLastFlush = now;
			for (let ef of enqueuedFlushes) {
				window.clearTimeout(ef);
			}
		}
	}
}

function getWindowSize() {
	var width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
	var height = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
	return [width, height];
}

/*
function getImageInHoldingArea(id) {
	var img = winFormsImages[id];
	delete winFormsImages[id];
	return img;
}
*/

//var winFormsImages = {};
function winFormsPrepImageData(id, width, height, dataUri) {
	throw new Error("Image stuff needs to be revisited");
	/*
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
	*/
}

function shimInit(uiData) {
	var noriRoot = document.getElementById('html_render_host');
	setFrameRoot(noriRoot);
	var sz = getWindowSize();
	setFrameSize(sz[0], sz[1]);
	window.onresize = function() {
		var newSize = getWindowSize();
		setFrameSize(newSize[0], newSize[1]);
		platformSpecificHandleEvent(-1, 'frame.onresize', newSize[0] + ',' + newSize[1]);
		flushUpdates(['NO', 0]);
	};
	flushUpdates(uiData);
}

