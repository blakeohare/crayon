window.registerMessageListener = null;
window.sendMessage = null;
(() => {
	let queue = [];
	let bridgeInstance = null;
	window.chrome.webview.hostObjects.u3bridge.then(async bridge => {
		bridgeInstance = bridge;
		await bridgeInstance.sendToCSharp('bridgeReady', '{}');

		if (queue.length > 0) {
			for (let queuedItem of queue) {
				await window.sendMessage(queuedItem.type, queuedItem.payloadStr);
			}
			queue = null;
		}
	}).catch(e => {
		console.log(e);
	});

	window.sendMessage = async (msgType, msgPayload) => {
		let msgPayloadStr = JSON.stringify(msgPayload);
		if (bridgeInstance !== null) {
			return bridgeInstance.sendToCSharp(msgType, msgPayloadStr);
		} else {
			queue.push({ type: msgType, payloadStr: msgPayloadStr });
		}
		return Promise.of(false);
	};

	let listener = null;
	window.registerMessageListener = (cb) => { listener = cb; };

	window.csharpToJavaScript = str => {
		listener(JSON.parse(str));
	};

})();
