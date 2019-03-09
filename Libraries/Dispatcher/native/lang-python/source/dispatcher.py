def lib_dispatcher_flushNativeQueueImpl(nativeData, output):
	
	mtx = nativeData[0]
	q = nativeData[1]
	
	mtx.acquire()
	
	for fp in q:
		output.append(fp)
	
	while len(q) > 0:
		q.pop()
		
	mtx.release()
	