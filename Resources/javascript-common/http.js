
C$http = 1;
C$http$makeHttpRequest = function (requestObj, method, url, body, userAgent, contentType, contentLength, headerNames, headerValues) {
	var requestSender = null;
	if (window.ActiveXObject) {
		requestSender = new ActiveXObject("Microsoft.XMLHTTP");
	} else if (window.XMLHttpRequest) {
		requestSender = new XMLHttpRequest({ mozSystem: true });
	}

	if (requestSender != null) {
		requestSender.open(method, url, true);
		requestSender.onreadystatechange = function () {
			if (requestSender.readyState == 4) {
				var code = requestSender.status;
				var status = requestSender.statusText;
				var content = requestSender.responseText;
				v_handleHttpResponse(requestObj, code, status, content, {});
			}
		};

		var headersSet = {};
		for (var i = 0; i < headerNames.length; ++i) {
			var canonicalName = headerNames[i].toUpperCase();
			requestSender.setRequestHeader(headerNames[i], headerValues[i]);
			headersSet[canonicalName] = true;
		}

		if (body != null) {
			requestSender.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			requestSender.send(body);
		} else {
			requestSender.send(null);
		}
	}
};
