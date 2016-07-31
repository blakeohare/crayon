
C$imageresources = 1;

C$imageresources$imageLoadAsync = function (filename, nativeData, imageLoaderNativeData) {
	C$imageresources$imageLoadAsyncImpl(filename, nativeData, imageLoaderNativeData, false, 0);
};

C$imageresources$imageLoaderError = function (isSync, statusOut, execId) {
	imageLoaderNativeData[isSync ? 0 : 2] = 2;

	if (isSync) {
		statusOut[0] = C$common$getBool(false);
		C$imageresources$restartThread(execId);
	} else {
		statusOut[2] = 2;
	}
};

C$imageresources$imageLoadAsyncImpl = function (filename, nativeData, imageLoaderNativeData, isActuallySync, executionContextId) {
	
	var image = new Image();
	var statusOut = imageLoaderNativeData;

	image.onerror = function () {
		C$imageresources$imageLoaderError(isActuallySync, imageLoaderNativeData, executionContextId);
	};

	image.onload = function () {
		if (isActuallySync && !statusOut[0][1]) { // status out is a list of Value bools
			// The timeout handler has already run and marked this as not loaded and yielded back to the VM.
            // It's too late for this handler to do anything.
			return;
		}

		var w = image.width;
		var h = image.height;
		if (w < 1 || h < 1) { // another possible error case
			C$imageresources$imageLoaderError(isActuallySync, imageLoaderNativeData, executionContextId);
		} else {
			var canvas = C$imageresources$generateNativeBitmapOfSize(w, h);
			var ctx = canvas.getContext('2d');
			ctx.drawImage(img1, 0, 0);
			nativeData[0] = canvas;
			nativeData[1] = w;
			nativeData[2] = h;
			if (isActuallySync) {
				statusOut[0] = C$common$getBool(true);
			} else {
				imageLoaderNativeData[2] = 1;
			}

			if (isActuallySync) {
				C$imageresources$restartThread(executionContextId);
			}
		}
	};

	image.src = C$common$jsFilePrefix + filename;
};

C$imageresources$restartThread = function (executionContextId) {
	C$game$runInterpreter(executionContextId);
};


C$imageresources$imageLoadSync = function (filename, nativeData, statusOut, executionContextId) {
	C$imageresources$imageLoadAsyncImpl(filename, nativeData, statusOut, true, executionContextId);
};

C$imageresources$getImageResourceManifest = function () {
	var output = C$common$getTextRes('imageresmanifest.txt');
	if (!output) return '';
	return output;
};

C$imageresources$generateNativeBitmapOfSize = function (width, height) {
	C$images$temp_image.innerHTML = '<canvas id="temp_image_canvas"></canvas>';
	var canvas = C$common$getElement('temp_image_canvas');
	canvas.width = width;
	canvas.height = height;
	var ctx = canvas.getContext('2d');
	C$images$temp_image.innerHTML = '';
	//return [canvas, ctx];

	// TODO: make sure this works and then get rid of the ctx stuff.
	return canvas;
};

C$imageresources$imageResourceBlitImage = function (target, source, targetX, targetY, sourceX, sourceY, width, height) {
	target.getContext('2d').drawImage(source, sourceX, sourceY, width, height, targetX, targetY, width, height);
};

C$imageresources$checkLoaderIsDone = function (imageLoaderNativeData, nativeImageDataNativeData, output) {
	output[0] = (imageLoaderNativeData[2] != 0) ? v_VALUE_TRUE : v_VALUE_FALSE;
};
