
// The events are sent back in batches intead of individually since something like
// a mouse move handler can easily bombard the VM with callbacks.
let msgQueue = [];
let msgQueueLastFlush = 0;
let enqueuedFlushes = [];
const msgQueueDelay = Math.floor(1000 / 30);
function platformSpecificHandleEvent(id, eventName, args) {
	msgQueue.push({ type: 'E', id, eventName, arg: args });
	flushMessageQueue();
}

function flushMessageQueue() {
	let now = Date.now();
	if (now - msgQueueLastFlush < msgQueueDelay) {
		let nextSlot = msgQueueLastFlush + msgQueueDelay + 1;
		enqueuedFlushes.push(window.setTimeout(() => { flushMessageQueue() }, nextSlot - now));
	} else {
		if (msgQueue.length > 0) {
			window.sendMessage('events', msgQueue);
			msgQueue = [];
			msgQueueLastFlush = now;
			for (let ef of enqueuedFlushes) {
				window.clearTimeout(ef);
			}
		}
	}
}

function getWindowSize() {
	let width = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
	let height = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
	return [width, height];
}

function shimInit(uiData) {
	let noriRoot = document.getElementById('html_render_host');
	setFrameRoot(noriRoot);
	let sz = getWindowSize();
	setFrameSize(sz[0], sz[1]);
	window.onresize = function() {
		let newSize = getWindowSize();
		setFrameSize(newSize[0], newSize[1]);
		platformSpecificHandleEvent(-1, 'frame.onresize', newSize[0] + ',' + newSize[1]);
		flushUpdates(['NO', 0]);
	};
	let fontUpdates = [];
	let otherUpdates = [];
	for (let i = 0; i < uiData.length; ++i) {
		if (uiData[i] === 'FR') {
			for (let j = 0; j < 4; ++j) {
				fontUpdates.push(uiData[i + j]);
			}
			i += 3;
		} else {
			otherUpdates = uiData.slice(i);
			break;
		}
	}
	uiData = otherUpdates;
	
	let finishInit = () => {
		flushUpdates(uiData);
		window.sendMessage('shown', true);
	};

	if (fontUpdates.length === 0) {
		finishInit();
	} else {
		flushUpdates(fontUpdates);
		waitForFonts().then(finishInit);
	}
}
