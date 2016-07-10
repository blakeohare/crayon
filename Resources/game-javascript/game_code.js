R.alwaysTrue = function() { return true; };
R.alwaysFalse = function() { return false; };

R.now = function () {
	return (Date.now ? Date.now() : new Date().getTime()) / 1000.0;
};

R.globals = {
	width: 0,
	height: 0,
	pwidth: 0,
	pheight: 0,
	fps: 60,
	real_canvas: null,
	virtual_canvas: null,
	scaled_mode: false,
	image_loader: null,
	image_store: null,
	temp_image: null,
	print_output: null,
	ctx: null,
	last_frame_began: R.now(),
	image_downloads: {},
	image_download_counter: 0,
	image_keys_by_index: [null],
	textResources: {},
};

R.addTextRes = function (path, value) {
    R.globals.textResources[path] = value;
};

R.getTextRes = function(path) {
    return R.globals.textResources[path];
};


R.is_image_loaded = function(key) {
    return R.globals.image_downloads[key] !== undefined;
};

R.enqueue_image_download = function(key, url) {
    var id = ++R.globals.image_download_counter;
    R.globals.image_keys_by_index.push(key);
	var loader_queue = getElement('crayon_image_loader_queue');
	loader_queue.innerHTML += '<img id="image_loader_img_' + id + '" onload="R.finish_load_image(' + id + ')" crossOrigin="anonymous" />' +
		'<canvas id="image_loader_canvas_' + id + '" />';
	var img = getElement('image_loader_img_' + id);
	img.src = %%%JS_FILE_PREFIX%%% + url;
	return true;
};

R.autogenDownloaderKey = 1;
R.better_enqueue_image_download = function(url) {
	var key = 'k' + R.autogenDownloaderKey++;
	var loader_queue = getElement('crayon_image_loader_queue');
	loader_queue.innerHTML += '<img id="better_downloader_' + key + '" onload="R.better_finish_load_image(&quot;' + key + '&quot;)" crossOrigin="anonymous" />' +
		'<canvas id="better_image_loader_canvas_' + key + '" />';
	var img = getElement('better_downloader_' + key);
	img.src = %%%JS_FILE_PREFIX%%% + url;
	return key;
};

// TODO: blit to a canvas that isn't in the DOM and then delete the img and canvas when completed.
// TODO: figure out if there are any in flight downloads, and if not, clear out the load queue DOM.
R.better_finish_load_image = function(key) {
    var img = getElement('better_downloader_' + key);
    var canvas = getElement('better_image_loader_canvas_' + key);
	canvas.width = img.width;
	canvas.height = img.height;
	var ctx = canvas.getContext('2d');
	ctx.drawImage(img, 0, 0);
	R.better_completed_image_lookup[key] = canvas;
};

R.get_completed_image_if_downloaded = function(key) {
	var canvas = R.better_completed_image_lookup[key];
	if (!!canvas) return canvas;
	return null;
};

R.better_completed_image_lookup = {};

R.finish_load_image = function(id) {
    var key = R.globals.image_keys_by_index[id];
	var img = getElement('image_loader_img_' + id);
	var canvas = getElement('image_loader_canvas_' + id);
	canvas.width = img.width;
	canvas.height = img.height;
	canvas.getContext('2d').drawImage(img, 0, 0);
	R.globals.image_downloads[key] = canvas;
};

R.flushImagette = function(imagette) {
	var width = imagette[0];
	var height = imagette[1];
	var images = imagette[2];
	var xs = imagette[3];
	var ys = imagette[4];
	var canvasAndContext = R.createCanvasAndContext(width, height);
	var canvas = canvasAndContext[0];
	var ctx = canvasAndContext[1];
	for (var i = 0; i < images.length; ++i) {
	    ctx.drawImage(images[i], xs[i], ys[i]);
	}
	return canvas;
};

R.createCanvasAndContext = function(width, height) {
    R.globals.temp_image.innerHTML = '<canvas id="temp_image_canvas"></canvas>';
	var canvas = getElement('temp_image_canvas');
	canvas.width = width;
	canvas.height = height;
	var ctx = canvas.getContext('2d');
	R.globals.temp_image.innerHTML = '';
	return [canvas, ctx];
};

R.beginFrame = function() {
    R.globals.last_frame_began = R.now();
    if (R.globals.ctx) {
        R.drawRect(0, 0, R.globals.width, R.globals.height, 0, 0, 0, 255);
	}
};

R.endFrame = function() {
    var gb = R.globals;
	if (gb.scaled_mode) {
		gb.real_canvas.getContext('2d').drawImage(gb.virtual_canvas, 0, 0);
	}
	window.setTimeout(R.runFrame, R.computeDelayMillis());
};

R.runFrame = function() {
    R.beginFrame();
    var cont = v_runInterpreter(R.globals.execId);
    if (!cont) return;
    R.endFrame();
};

R.computeDelayMillis = function () {
    var ideal = 1.0 / R.globals.fps;
    var diff = R.now() - R.globals.last_frame_began;
	return Math.floor((ideal - diff) * 1000);
};

R.initializeGame = function (fps) {
    R.globals.fps = fps;
};

R.pump_event_objects = function () {
	var new_events = [];
	var output = R._eventRelays;
	R._eventRelays = new_events;
	return output;
};

// TODO: also apply keydown and mousedown handlers
// TODO: (here and python as well) throw an error if you attempt to call this twice.
R.initializeScreen = function (width, height, pwidth, pheight, execId) {
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
		virtualCanvas = getElement('canvas');
		virtualCanvas.width = width;
		virtualCanvas.height = height;
	}
	var canvasHost = getElement('crayon_host');
	canvasHost.innerHTML =
		'<canvas id="crayon_screen" width="' + canvasWidth + '" height="' + canvasHeight + '"></canvas>' +
		'<div style="display:none;">' +
			'<img id="crayon_image_loader" onload="Q._finish_loading()" crossOrigin="anonymous" />' +
			'<div id="crayon_image_loader_queue"></div>' +
			'<div id="crayon_image_store"></div>' +
			'<div id="crayon_temp_image"></div>' +
		'</div>';
	var canvas = getElement('crayon_screen');
	var ctx = canvas.getContext('2d');
	R.globals.scaled_mode = scaledMode;
	R.globals.real_canvas = canvas;
	R.globals.virtual_canvas = scaledMode ? virtualCanvas : canvas;
	R.globals.image_loader = getElement('crayon_image_loader');
	R.globals.image_store = getElement('crayon_image_store');
	R.globals.temp_image = getElement('crayon_temp_image');
	R.globals.ctx = ctx;
	R.globals.width = width;
	R.globals.height = height;
	R.globals.execId = execId;

	document.onkeydown = R._keydown;
	document.onkeyup = R._keyup;
	
	canvas.addEventListener('mousedown', R._mousedown);
	canvas.addEventListener('mouseup', R._mouseup);
	canvas.addEventListener('mousemove', R._mousemove);

	ctx.imageSmoothingEnabled = false;
	ctx.mozImageSmoothingEnabled = false;
	ctx.msImageSmoothingEnabled = false;
	ctx.webkitImageSmoothingEnabled = false;

	if (scaledMode) {
	    ctx.scale(pwidth / width, pheight / height);
	}

	R.runFrame();
};

R.print = function (value) {
	console.log(value);
};

R.is_valid_integer = function (value) {
	var test = parseInt(value);
	// NaN produces a paradocical value that fails the following tests...
	// TODO: verify this on all browsers
	return test < 0 || test >= 0;
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

R.typeClassify = function(t) {
	if (t === null) return 'null';
	if (t === true || t === false) return 'bool';
	if (typeof t == "string") return 'string';
	if (typeof t == "number") {
		if (t % 1 == 0) return 'int';
		return 'float';
	}
	ts = Object.prototype.toString.call(t);
	if (ts == '[object Array]') {
		return 'list';
	}
	if (ts == '[object Object]') {
		return 'dict';
	}
	return 'null';
};

R.pumpAsyncMessageQueue = function() {
	return null;
};

window.addEventListener('keydown', function(e) {
	if ([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
		e.preventDefault();
	}
}, false);
