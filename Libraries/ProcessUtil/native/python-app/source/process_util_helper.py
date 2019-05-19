
def _ProcessUtilHelper_launch_process_impl(bridgeNativeData, execName, argStrings, isAsync, callback, dispatcherQueue):
	execArg = [execName]
	if os.name == 'nt':
		execArg = [r"C:\Windows\System32\cmd.exe", '/C', execName]
	args = execArg + argStrings
	p = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
	
	if isAsync:
		threadFn = lambda: _ProcessUtilHelper_async_process_watcher(p, bridgeNativeData, isAsync, callback, dispatcherQueue)
		threading.Thread(target = threadFn).start()
	else:
		_ProcessUtilHelper_async_process_watcher(p, bridgeNativeData, isAsync, callback, None)

def _ProcessUtilHelper_async_process_watcher(process, bridge, isAsync, callback, dispatcherQueue):
	mtx = bridge[4]
	stdout, stderr = process.communicate()
	try:
		mtx.acquire()
		_ProcessUtilHelper_write_string_to_bridge(None, bridge, 2, stdout)
		_ProcessUtilHelper_write_string_to_bridge(None, bridge, 3, stderr)
		bridge[0] = False
		bridge[1] = 0
	finally:
		mtx.release()
	
	if isAsync:
		dispatcherQueue[0].acquire()
		try:
			dispatcherQueue[1].append(callback)
		finally:
			dispatcherQueue[0].release()
	
def _ProcessUtilHelper_write_string_to_bridge(mtx, bridgeNativeData, ndIndex, rawValue):
	if sys.version_info[0] >= 3:
		try:
			s = rawValue.decode('utf-8')
		except:
			s = rawValue.decode('windows-1252')
	else:
		# Python 2 uses strings directly
		s = rawValue
	
	if len(s) == 0: return
	
	lines = s.split('\n')
	
	if mtx != None:
		mtx.acquire()
	try:
		for line in lines:
			bridgeNativeData[ndIndex].append(line)
	finally:
		if mtx != None:
			mtx.release()

def _ProcessUtilHelper_read_bridge_int(mtx, bridgeNativeData, ndIndex):
	mtx.acquire()
	try:
		return bridgeNativeData[ndIndex]
	finally:
		mtx.release()

def _ProcessUtilHelper_read_bridge_strings(mtx, bridgeNativeData, ndIndex, stringsOut):
	mtx.acquire()
	try:
		buffer = bridgeNativeData[ndIndex]
		for s in buffer:
			stringsOut.append(s)
		while len(buffer) > 0: buffer.pop()
	finally:
		mtx.release()
