
LIB$http$makeHttpRequest = function(requestNativeData, method, url, contentMode, content, requestHeaders, outputIsBinary, executionContextId, vm, cb) {
	var xhr = new XMLHttpRequest({ mozSystem: true });

	xhr.open(method, url, true);
	
	xhr.onload = function () {
		if (xhr.readyState == 4) {
			var rawHeaders = xhr.getAllResponseHeaders().trim().split('\r\n');
			var responseHeaders = [];
			for (var i = 0; i < rawHeaders.length; ++i) {
				var parts = rawHeaders[i].split(':', 2);
				responseHeaders.push(parts[0].trim());
				responseHeaders.push(parts.length > 1 ? parts[1].trim() : '');
			}
			var data = outputIsBinary ? xhr.response : xhr.responseText;
			requestNativeData[0] = {
				'sc': xhr.status,
				'sm': xhr.statusText,
				'ib': outputIsBinary, 
				'cb': outputIsBinary ? data : null,
				'cs': outputIsBinary ? null : data,
				'h': responseHeaders,
			};
			requestNativeData[2] = true;
			if (executionContextId !== null) {
				runInterpreter(vm, executionContextId);
			}
			
			if (cb != null) {
				runInterpreterWithFunctionPointer(vm, cb, []);
			}
		}
	};
	
	xhr.ontimeout = function () {
		requestNativeData[0] = null;
		requestNativeData[2] = true;
		if (executionContextId !== null) {
			runInterpreter(vm, executionContextId);
		}
	};

	for (var i = 0; i < requestHeaders.length; i += 2) {
		var name = requestHeaders[i];
		switch (name.toLowerCase()) {
			case 'user-agent': break;
			default: xhr.setRequestHeader(name, requestHeaders[i + 1]); break;
		}
	}

	xhr.send(content);
};

LIB$http$readResponseData = function (response, intOut, stringOut, responseNativeData, headersOut) {
	intOut[0] = response['sc'];
	intOut[1] = response['ib'] ? 1 : 0;
	stringOut[0] = response['sm'];
	stringOut[1] = response['cs'];
	responseNativeData[0] = response['cb'];
	
	var headers = response['h'];
	for (var i = 0; i < headers.length; i += 2) {
		headersOut.push(headers[i]);
		headersOut.push(headers[i + 1]);
	}
};

LIB$http$getResponseBytes = function (bytes, integers, output) {
	var length = bytes.length;
	var i = 0;
	while (i < length) {
		output.push(integers[bytes[i++]]);
	}
};

LIB$http$sendRequestAsync = function(requestNativeData, method, url, headers, contentMode, content, outputIsBinary, vm, cb, mutexIgnore) {
	LIB$http$makeHttpRequest(requestNativeData, method, url, contentMode, content, headers, outputIsBinary, null, vm, cb);
};

LIB$http$sendRequestSync = function (
	requestNativeData,
	method,
	url,
	headers,
	contentMode, // 0 - null, 1 - text, 2 - binary
	content,
	outputIsBinary,
	executionContextId,
	vm) {
	LIB$http$makeHttpRequest(requestNativeData, method, url, contentMode, content, headers, outputIsBinary, executionContextId, vm, null);
	return true;
};

LIB$http$pollRequest = function (requestNativeData) {
	return requestNativeData[2];
};
