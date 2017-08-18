lib_http_vars = {}

def lib_http_get_methods():
	if len(lib_http_vars) == 0:
		isOld = 3 / 2 == 1
		lib_http_vars['isOld'] = isOld
		if isOld:
			import urllib
			import urllib2
			import urlparse
			lib_http_vars['urllib'] = urllib
			lib_http_vars['urllib2'] = urllib2
			lib_http_vars['urlparse'] = urlparse
		else:
			import urllib
			import urllib.parse
			import urllib.request
			lib_http_vars['urllib'] = urllib
			lib_http_vars['urllibParse'] = urllib.parse
			lib_http_vars['urllibRequest'] = urllib.request
		import threading
		lib_http_vars['threading'] = threading
	return lib_http_vars


def lib_http_readResponseData(httpRequest, intOut, stringOut, responseNativeData, headersOut):
	intOut[0] = httpRequest['statusCode']
	intOut[1] = 1 if httpRequest['isBinary'] else 0
	stringOut[0] = httpRequest['statusMessage']
	stringOut[1] = httpRequest['contentString']
	responseNativeData[0] = httpRequest['contentBytes']
	for h in httpRequest['headers']:
		headersOut.append(h)

def lib_http_getResponseBytes(byteArray, integersCache, output):
	for b in byteArray:
		output.append(integersCache[b])

def lib_http_sendRequestAsync(requestNativeData, method, url, headers, contentMode, content, outputIsBinary):
	vars = lib_http_get_methods()
	requestNativeData[1] = vars['threading'].Lock()
	requestNativeData[2] = False
	
	thread = vars['threading'].Thread(
		target = lib_http_sendRequestAsyncWorker,
		args = (requestNativeData, method, url, headers, contentMode, content, outputIsBinary))
	thread.daemon = True
	thread.start()

def lib_http_sendRequestAsyncWorker(requestNativeData, method, url, headers, contentMode, content, outputIsBinary):
	response = lib_http_sendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary)
	requestNativeData[1].acquire()
	requestNativeData[0] = response
	requestNativeData[2] = True
	requestNativeData[1].release()

def lib_http_sendRequestSync(requestNativeData, method, url, headers, contentMode, content, outputIsBinary):
	response = lib_http_sendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary)
	requestNativeData[0] = response
	requestNativeData[2] = True
	return False

def lib_http_sendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary):
	vars = lib_http_get_methods()
	if vars['isOld']:
		opener = vars['urllib2'].build_opener(vars['urllib2'].HTTPHandler)
		if contentMode == 0:
			request = vars['urllib2'].Request(url)
		elif contentMode == 1:
			request = vars['urllib2'].Request(url, data=content)
		else:
			raise Error("TODO: binary content")
	else:
		opener = vars['urllibRequest'].build_opener(vars['urllibRequest'].HTTPHandler)
		if contentMode == 0:
			request = vars['urllibRequest'].Request(url)
		elif contentMode == 1:
			request = vars['urllibRequest'].Request(url, data=content)
		elif contentMode == 2:
			raise Error("TODO: binary content")
	for i in range(0, len(headers), 2):
		headerName = headers[i]
		headerValue = headers[i + 1]
		request.add_header(headerName, headerValue)
	
	request.get_method = lambda:method
	output = opener.open(request)
	contentBytes = None
	contentString = None
	if outputIsBinary:
		contentBytes = []
		byte = output.read(1)
		while byte != '':
			contentBytes.append(ord(byte))
			byte = output.read(1)
	else:
		contentString = output.read()
		
	headers = []
	for header_key in output.headers.keys():
		headers.append(header_key)
		headers.append(output.headers[header_key])
	response_message = output.msg
	response_code = output.code
	
	return {
		'statusCode': output.code,
		'statusMessage': output.msg,
		'isBinary': outputIsBinary,
		'contentString': contentString,
		'contentBytes': contentBytes,
		'headers': headers,
	}

def lib_http_pollRequest(requestNativeData):
	requestNativeData[1].acquire()
	isDone = requestNativeData[2]
	requestNativeData[1].release()
	return isDone
