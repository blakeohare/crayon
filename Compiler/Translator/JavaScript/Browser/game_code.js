var R = {};

R.now = function () {
	return (Date.now ? Date.now() : new Date().getTime()) / 1000.0;
};

R.get_image_impl = function(key) {
	return [%%%TYPE_NATIVE_OBJECT_IMAGE%%%, R._global_vars.image_downloads[key]];
};

R._global_vars = {
	'width': 0,
	'height': 0,
	'pwidth': 0,
	'pheight': 0,
	'fps': 60,
	'real_canvas': null,
	'virtual_canvas': null,
	'scaled_mode': false,
	'image_loader': null,
	'image_store': null,
	'temp_image': null,
	'print_output': null,
	'event_queue': [],
	'ctx': null,
	'last_frame_began': R.now(),
	'image_downloads': {},
	'image_download_counter': 0,
	'image_keys_by_index': [null]
};

R.is_image_loaded = function(key) {
	return R._global_vars.image_downloads[key] !== undefined;
};

R.enqueue_image_download = function(key, url) {
	var id = ++R._global_vars.image_download_counter;
	R._global_vars.image_keys_by_index.push(key);
	var loader_queue = document.getElementById('crayon_image_loader_queue');
	loader_queue.innerHTML += '<img id="image_loader_img_' + id + '" onload="R.finish_load_image(' + id + ')" crossOrigin="anonymous" />' +
		'<canvas id="image_loader_canvas_' + id + '" />';
	var img = document.getElementById('image_loader_img_' + id);
	img.src = %%%JS_FILE_PREFIX%%% + url;
	return true;
};

R.autogenDownloaderKey = 1;
R.better_enqueue_image_download = function(url) {
	var key = 'k' + R.autogenDownloaderKey++;
	var loader_queue = document.getElementById('crayon_image_loader_queue');
	loader_queue.innerHTML += '<img id="better_downloader_' + key + '" onload="R.better_finish_load_image(&quot;' + key + '&quot;)" crossOrigin="anonymous" />' +
		'<canvas id="better_image_loader_canvas_' + key + '" />';
	var img = document.getElementById('better_downloader_' + key);
	img.src = %%%JS_FILE_PREFIX%%% + url;
	return key;
};

// TODO: blit to a canvas that isn't in the DOM and then delete the img and canvas when completed.
// TODO: figure out if there are any in flight downloads, and if not, clear out the load queue DOM.
R.better_finish_load_image = function(key) {
	var img = document.getElementById('better_downloader_' + key);
	var canvas = document.getElementById('better_image_loader_canvas_' + key);
	canvas.width = img.width;
	canvas.height = img.height;
	var context = canvas.getContext('2d');
	context.drawImage(img, 0, 0);
	R.better_completed_image_lookup[key] = canvas;
};

R.get_completed_image_if_downloaded = function(key) {
	var canvas = R.better_completed_image_lookup[key];
	if (!!canvas) return canvas;
	return null;
};

R.better_completed_image_lookup = {};

R.finish_load_image = function(id) {
	var key = R._global_vars.image_keys_by_index[id];
	var img = document.getElementById('image_loader_img_' + id);
	var canvas = document.getElementById('image_loader_canvas_' + id);
	canvas.width = img.width;
	canvas.height = img.height;
	var context = canvas.getContext('2d');
	context.drawImage(img, 0, 0);
	R._global_vars.image_downloads[key] = canvas;
};

R.flushImagette = function(imagette) {
	var width = imagette[0];
	var height = imagette[1];
	var images = imagette[2];
	var xs = imagette[3];
	var ys = imagette[4];
	var length = images.length;
	var canvasAndContext = R.createCanvasAndContext(width, height);
	var canvas = canvasAndContext[0];
	var context = canvasAndContext[1];
	for (var i = 0; i < length; ++i) {
		context.drawImage(images[i], xs[i], ys[i]);
	}
	return canvas;
};

R.createCanvasAndContext = function(width, height) {
	R._global_vars.temp_image.innerHTML = '<canvas id="temp_image_canvas"></canvas>';
	var canvas = document.getElementById('temp_image_canvas');
	canvas.width = width;
	canvas.height = height;
	var context = canvas.getContext('2d');
	R._global_vars.temp_image.innerHTML = '';
	return [canvas, context];
};

R.beginFrame = function() {
	R._global_vars.last_frame_began = R.now();
	if (R._global_vars.ctx) {
		R.drawRect(0, 0, R._global_vars.width, R._global_vars.height, 0, 0, 0, 255);
	}
};

R.endFrame = function() {
	var gb = R._global_vars;
	if (gb.scaled_mode) {
		var rc = gb.real_canvas;
		var vc = gb.virtual_canvas;
		var rctx = rc.getContext('2d');
		rctx.drawImage(vc, 0, 0);
	}
	window.setTimeout(v_runTick, R.computeDelayMillis());
};

R.computeDelayMillis = function () {
	var ideal = 1.0 / R._global_vars.fps;
	var diff = R.now() - R._global_vars.last_frame_began;
	var delay = ideal - diff;
	return Math.floor(delay * 1000);
};

R.initializeGame = function (fps) {
	R._global_vars['fps'] = fps;
};

R.pump_event_objects = function () {
	var new_events = [];
	var output = R._global_vars['event_queue'];
	R._global_vars['event_queue'] = new_events;
	return output;
};

// TODO: also apply keydown and mousedown handlers
// TODO: (here and python as well) throw an error if you attempt to call this twice.
R.initializeScreen = function (width, height, pwidth, pheight) {
	var scaledMode;
	var canvasWidth;
	var canvasHeight;
	var virtualCanvas = null;
	if (pwidth === null || pheight === null) {
		scaledMode = false;
		canvasWidth = width;
		canvasHeight = height;
	} else {
		scaledMode = true;
		canvasWidth = pwidth;
		canvasHeight = pheight;
		virtualCanvas = document.createElement('canvas');
		virtualCanvas.width = width;
		virtualCanvas.height = height;
	}
	var canvasHost = document.getElementById('crayon_host');
	canvasHost.innerHTML =
		'<canvas id="crayon_screen" width="' + canvasWidth + '" height="' + canvasHeight + '"></canvas>' +
		'<div style="display:none;">' +
			'<img id="crayon_image_loader" onload="Q._finish_loading()" crossOrigin="anonymous" />' +
			'<div id="crayon_image_loader_queue"></div>' +
			'<div id="crayon_image_store"></div>' +
			'<div id="crayon_temp_image"></div>' +
		'</div>';
	var canvas = document.getElementById('crayon_screen');
	R._global_vars['scaled_mode'] = scaledMode;
	R._global_vars['real_canvas'] = canvas;
	R._global_vars['virtual_canvas'] = scaledMode ? virtualCanvas : canvas;
	R._global_vars['image_loader'] = document.getElementById('crayon_image_loader');
	R._global_vars['image_store'] = document.getElementById('crayon_image_store');
	R._global_vars['temp_image'] = document.getElementById('crayon_temp_image');
	R._global_vars['ctx'] = canvas.getContext('2d');
	R._global_vars['width'] = width;
	R._global_vars['height'] = height;

	document.onkeydown = R._keydown;
	document.onkeyup = R._keyup;
	
	canvas.addEventListener('mousedown', R._mousedown);
	canvas.addEventListener('mouseup', R._mouseup);
	canvas.addEventListener('mousemove', R._mousemove);

	R._global_vars['ctx'].imageSmoothingEnabled = false;
	R._global_vars['ctx'].mozImageSmoothingEnabled = false;
	R._global_vars['ctx'].msImageSmoothingEnabled = false;
	R._global_vars['ctx'].webkitImageSmoothingEnabled = false;

	if (scaledMode) {
		R._global_vars['ctx'].scale(pwidth / width, pheight / height);
	}
};

R.print = function (value) {
	console.log(value);
};

R.is_valid_integer = function (value) {
	var test = parseInt(value);
	// NaN produces a paradocical value that fails the following tests...
	// TODO: verify this on all browsers
	if (value < 0) return true;
	if (value >= 0) return true;
	return false;
};

R.setTitle = function (title) {
	window.document.title = title;
};

R.sortedCopyOfArray = function(nums) {
	var newArray = nums.concat([]);
	newArray.sort();
	return newArray;
};

R.floatParseHelper = function(floatOut, text) {
	var output = parseFloat(text);
	if (output + '' == 'NaN') {
		floatOut[0] = -1;
		return;
	}
	floatOut[0] = 1;
	floatOut[1] = output;
};

R.parseJson = function(rawText) {
	try {
		return R.convertJsonThing(JSON.parse(rawText));
	} catch (e) {
		return null;
	}
};

R.convertJsonThing = function(thing) {
	var type = R.typeClassify(thing);
	switch (type) {
		case 'null': return v_VALUE_NULL;
		case 'bool': return thing ? v_VALUE_TRUE : v_VALUE_FALSE;
		case 'string': return v_buildString(thing);
		case 'list':
			var list = [];
			for (i = 0; i < thing.length; ++i) {
				list.push(R.convertJsonThing(thing[i]));
			}
			return v_buildListByWrappingInput(list);
		case 'dict':
			var keys = [];
			var values = [];
			for (var rawKey in thing) {
				keys.push(rawKey);
				values.push(R.convertJsonThing(thing[rawKey]));
			}
			return v_buildDictionary(keys, values);
		case 'int':
			return v_buildInteger(thing);
		case 'float':
			return v_buildFloat(thing);
		default:
			return v_VALUE_NULL;
	}
};

R.typeClassify = function(thing) {
	if (thing === null) return 'null';
	if (thing === true) return 'bool';
	if (thing === false) return 'bool';
	if (typeof thing == "string") return 'string';
	if (typeof thing == "number") {
		if (thing % 1 == 0) return 'int';
		return 'float';
	}
	ts = Object.prototype.toString.call(thing);
	if (ts == '[object Array]') {
		return 'list';
	}
	if (ts == '[object Object]') {
		return 'dict';
	}
	return 'null';
};

R.sortPrimitiveValuesList = function(list) {
	var lookup = {};
	var keys = [];
	var i;
	for (i = 0; i < list.length; ++i) {
		var key = list[i][1];
		lookup[key] = list[i];
		keys.push(key);
	}
	keys.sort();
	for (i = 0; i < list.length; ++i) {
		list[i] = lookup[keys[i]];
	}
};

R.pumpAsyncMessageQueue = function() {
	return null;
};

window.addEventListener('keydown', function(e) {
	if ([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
		e.preventDefault();
	}
}, false);
