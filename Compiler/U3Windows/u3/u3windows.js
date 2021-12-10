window.registerMessageListener = null;
window.sendMessage = null;
(() => {
	let queue = [];
	let bridgeInstance = null;
	console.log("U3WINDOWS");
	console.log(window.chrome.webview.hostObjects);
	console.log(window.chrome.webview.hostObjects.u3bridge);
	window.chrome.webview.hostObjects.u3bridge.then(async bridge => {
		console.log("E");
		bridgeInstance = bridge;
		console.log("A");
		await bridgeInstance.sendToCSharp('bridgeReady', '{}');
		console.log("B");

		if (queue.length > 0) {
			for (let queuedItem of queue) {
				console.log("C");
				await window.sendMessage(queuedItem.type, queuedItem.payloadStr);
			}
			queue = null;
		}
	}).catch(e => {
		console.log(e);
	});
	console.log("FOO");

	window.sendMessage = async (msgType, msgPayload) => {
		console.log("D " + msgType);
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
		console.log("csharpToJavaScript was called");
		listener(JSON.parse(str));
	};

})();
