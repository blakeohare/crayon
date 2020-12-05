
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

let getWindowSize = () => {
	return [window.innerWidth, window.innerHeight];
};

function shimInit(uiData, options) {
	let shownCb = () => { window.sendMessage('shown', true); };
	let eventBatchSender = data => { window.sendMessage('eventBatch', data); };
	let keepAspectRatio = options && options.keepAspectRatio;
	let targetAspectRatio = window.innerWidth / Math.max(1, window.innerHeight);
	let noriRoot;
	let root = document.getElementById('html_render_host');
	if (keepAspectRatio) {
		let currentSize = [window.innerWidth, window.innerHeight];
		getWindowSize = () => currentSize.slice(0);
		let inner = document.createElement('div');
		let outStyle = root.style;
		let inStyle = inner.style;
		outStyle.position = 'relative';
		outStyle.backgroundColor = '#000000';
		outStyle.overflow = 'hidden';
		inStyle.position = 'absolute';
		inStyle.overflow = 'hidden';
		root.append(inner);
		noriRoot = inner;
		let setSize = () => {
			let width = window.innerWidth;
			let height = window.innerHeight;
			let aspectRatio = width / Math.max(1, height);
			let innerWidth;
			let innerHeight;
			let left = 0;
			let top = 0;
			if (targetAspectRatio > aspectRatio) {
				innerWidth = width;
				innerHeight = Math.max(1, Math.floor(width / targetAspectRatio));
				top = Math.floor((height - innerHeight) / 2); 
			} else {
				innerHeight = height;
				innerWidth = Math.max(1, Math.floor(height * targetAspectRatio));
				left = Math.floor((width - innerWidth) / 2);
			}
			outStyle.width = width + 'px';
			outStyle.height = height + 'px';
			inStyle.width = innerWidth + 'px';
			inStyle.height = innerHeight + 'px';
			inStyle.left = left + 'px';
			inStyle.top = top + 'px';
			currentSize = [innerWidth, innerHeight];
		};
		window.addEventListener('resize', () => { 
			setSize(); 
			NoriLayout.doLayoutPass();
		});
		setSize();
	} else {
		noriRoot = root;
	}
	noriInit(noriRoot, uiData, shownCb, eventBatchSender);
}
