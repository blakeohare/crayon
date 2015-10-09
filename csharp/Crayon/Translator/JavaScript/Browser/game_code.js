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

R.blitPartial = function(canvas, tx, ty, tw, th, sx, sy, sw, sh) {
	if (tw == 0 || th == 0 || sw == 0 || sh == 0) return;

	R._global_vars.ctx.drawImage(canvas, sx, sy, sw, sh, tx, ty, tw, th);
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

R.scaleImage = function(originalCanvas, width, height) {
	var output = document.createElement('canvas');
	var context = output.getContext('2d');
	output.width = width;
	output.height = height;
	context.drawImage(
		originalCanvas,
		0, 0, originalCanvas.width, originalCanvas.height,
		0, 0, width, height);
	return output;
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
		'</div>';// +
		//'<div style="font-family:&quot;Courier New&quot;; font-size:11px;" id="crayon_print_output"></div>';
	var canvas = document.getElementById('crayon_screen');
	R._global_vars['scaled_mode'] = scaledMode;
	R._global_vars['real_canvas'] = canvas;
	R._global_vars['virtual_canvas'] = scaledMode ? virtualCanvas : canvas;
	R._global_vars['image_loader'] = document.getElementById('crayon_image_loader');
	R._global_vars['image_store'] = document.getElementById('crayon_image_store');
	R._global_vars['temp_image'] = document.getElementById('crayon_temp_image');
	R._global_vars['print_output'] = document.getElementById('crayon_print_output');
	R._global_vars['ctx'] = canvas.getContext('2d');
	R._global_vars['print_lines'] = [];
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

R._mousedown = function(ev) {
	R._mousething(ev, true, true);
};

R._mouseup = function(ev) {
	R._mousething(ev, true, false);
};

R._mousemove = function(ev) {
	R._mousething(ev, false, 'ignored');
};

R._mousething = function(ev, click, down) {
	var pos = R._mouse_get_pos_from_event(ev);
	var x = pos[0];
	var y = pos[1];
	var rwidth = R._global_vars.real_canvas.width;
	var rheight = R._global_vars.real_canvas.height;
	var vwidth = R._global_vars.virtual_canvas.width;
	var vheight = R._global_vars.virtual_canvas.height;

	x = Math.floor(x * vwidth / rwidth);
	y = Math.floor(y * vheight / rheight);

	if (click) {
		var rightclick = false;
		if (!ev) ev = window.event;
		if (ev.which) rightclick = (ev.which == 3);
		else if (ev.button) rightclick = (ev.button == 2);
		var button = rightclick ? 'right' : 'left';
		R._global_vars.event_queue.push(v_buildGameEvent('mouse' + button + (down ? 'down' : 'up'), 'mouse', x, y, 0, down, button));
	} else {
		R._global_vars.event_queue.push(v_buildGameEvent('mousemove', 'mouse', x, y, 0, false, null));
	}
};

R._mouse_get_pos_from_event = function (ev) {
	var obj = R._global_vars.real_canvas;
	var obj_left = 0;
	var obj_top = 0;
	var xpos;
	var ypos;
	while (obj.offsetParent) {
		obj_left += obj.offsetLeft;
		obj_top += obj.offsetTop;
		obj = obj.offsetParent;
	}
	if (ev) {
		//FireFox
		xpos = ev.pageX;
		ypos = ev.pageY;
	} else {
		//IE
		xpos = window.event.x + document.body.scrollLeft - 2;
		ypos = window.event.y + document.body.scrollTop - 2;
	}
	xpos -= obj_left;
	ypos -= obj_top;
	return [xpos, ypos];
};

R.shiftLines = function () {
	while (R._global_vars.print_lines.length >= 20) {
		R._global_vars.print_lines = R._global_vars.print_lines.slice(1);
	}
	var last = [];
	R._global_vars.print_lines.push(last);
	return last;
};

R.print = function (value) {
	/*
	if (R._global_vars.print_output == null) {
		window.alert(value);
	} else {
		var line = R.shiftLines();
		var col = 0;
		var i;

		for (i = 0; i < value.length; ++i, ++col) {
			if (col == 80) {
				line = R.shiftLines();
				col = 0;
			}
			var c = value.charAt(i);
			switch (c) {
				case '<': line.push('&lt;'); break;
				case '>': line.push('&gt;'); break;
				case '&': line.push('&amp;'); break;
				case ' ': line.push('&nbsp;'); break;
				case '\t': line.push('&nbsp;&nbsp;&nbsp;&nbsp;'); break;
				case '\n': 
					line = R.shiftLines();
					col = 0;
					break;
				default: line.push(c); break;
			}
		}

		var lines = R._global_vars.print_lines;
		var output = [];
		for (i = 0; i < lines.length; ++i) {
			output.push(lines[i].join(''));
		}
		R._global_vars.print_output.innerHTML = output.join('<br />');
	}//*/
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

R._keydown = function (ev) {
	R._keydownup(ev, true);
};

R._keyup = function (ev) {
	R._keydownup(ev, false);
};

R._keydownup = function (ev, down) {
	var keycode = R._getKeyCode(ev);
	if (keycode != null) {
		R._global_vars.event_queue.push(v_buildGameEvent('key' + (down ? 'down' : 'up'), 'key', 0, 0, 0, down, keycode));
	}
};

R._keyCodeLookup = {
	'k3': 'break',
	'k8': 'backspace',
	'k9': 'tab',
	'k13': 'enter',
	'k27': 'escape',
	'k16': 'shift', 'k17': 'ctrl', 'k18': 'alt',
	'k19': 'pause',
	'k20': 'capslock',
	'k32': 'space',
	'k33': 'pageup', 'k34': 'pagedown',
	'k35': 'end', 'k36': 'home',
	'k44': 'printscreen',
	'k45': 'insert', 'k46': 'delete',

	'k48': '0', 'k49': '1', 'k50': '2', 'k51': '3', 'k52': '4',
	'k53': '5', 'k54': '6', 'k55': '7', 'k56': '8', 'k57': '9',

	'k65': 'a', 'k66': 'b', 'k67': 'c', 'k68': 'd', 'k69': 'e',
	'k70': 'f', 'k71': 'g', 'k72': 'h', 'k73': 'i', 'k74': 'j',
	'k75': 'k', 'k76': 'l', 'k77': 'm', 'k78': 'n', 'k79': 'o',
	'k80': 'p', 'k81': 'q', 'k82': 'r', 'k83': 's', 'k84': 't',
	'k85': 'u', 'k86': 'v', 'k87': 'w', 'k88': 'x', 'k89': 'y',
	'k90': 'z',

	'k112': 'f1', 'k113': 'f2', 'k114': 'f3', 'k115': 'f4',
	'k116': 'f5', 'k117': 'f6', 'k118': 'f7', 'k119': 'f8',
	'k120': 'f9', 'k121': 'f10', 'k122': 'f11', 'k123': 'f12',

	'k37': 'left', 'k38': 'up', 'k39': 'right', 'k40': 'down',

	'k59': 'semicolon',
	'k61': 'equals',
	'k93': 'menu',
	'k145': 'scrolllock',
	'k173': 'hyphen',
	'k187': 'equals',
	'k188': 'comma',
	'k190': 'period',
	'k191': 'slash',
	'k192': 'tilde',
	'k219': 'openbracket',
	'k220': 'backslash',
	'k221': 'closebracket',
	'k222': 'apostrophe'
};

R._getKeyCode = function (ev) {
	var keyCode = ev.which ? ev.which : ev.keyCode;
	var output = R._keyCodeLookup['k' + keyCode];
	return output === undefined ? null : output;
};

R._toHex = function (r, g, b) {
	var hd = '0123456789abcdef';
	return '#'
		+ hd[r >> 4] + hd[r & 15]
		+ hd[g >> 4] + hd[g & 15]
		+ hd[b >> 4] + hd[b & 15];
};

R.fillScreen = function (r, g, b) {
	var gb = R._global_vars;
	gb.ctx.fillStyle = R._toHex(r, g, b);
	gb.ctx.fillRect(0, 0, gb.width, gb.height);
};

R.drawRect = function (x, y, width, height, r, g, b, a) {
	if (a == 0) return;
	var ctx = R._global_vars.ctx;
	ctx.fillStyle = R._toHex(r, g, b);
	if (a != 255) {
		ctx.globalAlpha = a / 255;
		ctx.fillRect(x, y, width + .1, height + .1);
		ctx.globalAlpha = 1;
	} else {
		ctx.fillRect(x, y, width + .1, height + .1);
	}
};

R.drawTriangle = function(ax, ay, bx, by, cx, cy, r, g, b, a) {
	if (a == 0) return;
	var ctx = R._global_vars.ctx;
	
	var tpath = new Path2D();
	tpath.moveTo(ax, ay);
	tpath.lineTo(bx, by);
	tpath.lineTo(cx, cy);

	ctx.fillStyle = R._toHex(r, g, b);
	if (a != 255) {
		ctx.globalAlpha = a / 255;
		ctx.fill(tpath);
		ctx.globalAlpha = 1;
	} else {
		ctx.fill(tpath);
	}
};

R.drawEllipse = function(left, top, width, height, r, g, b, alpha) {
	var radiusX = width / 2;
	var radiusY = height / 2;
	var centerX = left + radiusX;
	var centerY = top + radiusY;
	
	var context = R._global_vars.ctx;
	radiusX = radiusX * 4 / 3; // no idea...
	context.beginPath();
	context.moveTo(centerX, centerY - radiusY);
	context.bezierCurveTo(
		centerX + radiusX, centerY - radiusY,
		centerX + radiusX, centerY + radiusY,
		centerX, centerY + radiusY);
	context.bezierCurveTo(
		centerX - radiusX, centerY + radiusY,
		centerX - radiusX, centerY - radiusY,
		centerX, centerY - radiusY);
	context.fillStyle = R._toHex(r, g, b);
	if (alpha != 255) {
		context.globalAlpha = alpha / 255;
		context.fill();
		context.closePath();
		context.globalAlpha = 1;
	} else {
		context.fill();
		context.closePath();
	}
};

R.drawLine = function(startX, startY, endX, endY, width, r, g, b, a) {
	var context = R._global_vars.ctx;
	var offset = ((width % 2) == 0) ? 0 : .5;
	context.beginPath();
	context.moveTo(startX + offset, startY + offset);
	context.lineTo(endX + offset, endY + offset);
	context.lineWidth = width;
	if (a != 255) {
		context.globalAlpha = a / 255;
		context.strokeStyle = R._toHex(r, g, b);
		context.stroke();
		context.closePath();
		context.globalAlpha = 1;
	} else {
		context.strokeStyle = R._toHex(r, g, b);
		context.stroke();
		context.closePath();
	}
};

R.flipImage = function(canvas, flipX, flipY) {
	var output = document.createElement('canvas');

	output.width = canvas.width;
	output.height = canvas.height;

	var outputContext = output.getContext('2d');

	if (flipX) {
		outputContext.translate(canvas.width, 0);
		outputContext.scale(-1, 1);
	}
	if (flipY) {
		outputContext.translate(0, canvas.height);
		outputContext.scale(1, -1);
	}

	outputContext.drawImage(canvas, 0, 0);

	if (flipX) {
		outputContext.scale(-1, 1);
		outputContext.translate(-canvas.width, 0);
	}
	if (flipY) {
		outputContext.scale(1, -1);
		outputContext.translate(0, -canvas.height);
	}

	return output;
};

R.playSound = function(platformSound) {
	// TODO: playSound
};

R.sortedCopyOfArray = function(nums) {
	var newArray = nums.concat([]);
	newArray.sort();
	return newArray;
};

R.readResourceText = function(path) {
	var output = R.resources[path];
	if (output === undefined) return null;
	return output;
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

R.IO = {};

R.IO.virtualDisk = null;
R.IO.userData = null;
R.IO.get_disk = function(isUserData) {
	if (isUserData) {
		if (R.IO.userData === null) {
			try {
				if ('localStorage' in window && window['localStorage'] !== null) {
					R.IO.userData = createFakeDisk(localStorage);
				}
			} catch (e) {}
			if (R.IO.userData == null) R.IO.userData = createFakeDisk(null);
		}
		return R.IO.userData;
	} else {
		if (R.IO.virtualDisk === null) {
			R.IO.virtualDisk = createFakeDisk(null);
		}
		return R.IO.virtualDisk;
	}
}

R.IO.checkPath = function(path, isDir, isUserData) {
	disk = R.IO.get_disk(isUserData);
	if (isDir) {
		return disk.is_directory(path);
	} else {
		return disk.path_exists(path);
	}
	return false;
};

R.IO.listFiles = function(path, isUserData) {
	disk = R.IO.get_disk(isUserData);
	return disk.list_dir(path);
};

R.IO.readFile = function(path, isUserData) {
	disk = R.IO.get_disk(isUserData);
	return disk.read_file(path);
};

R.IO.writeFile = function(path, content, isUserData) {
	disk = R.IO.get_disk(isUserData);
	return disk.write_text(path, content);
};

R.makeHttpRequest = function(requestObj, method, url, body, userAgent, contentType, contentLength, headerNames, headerValues) {
	var requestSender = null;
	if (window.ActiveXObject) {
		requestSender = new ActiveXObject("Microsoft.XMLHTTP");
	} else if (window.XMLHttpRequest) {
		requestSender = new XMLHttpRequest({mozSystem: true});
	}

	if (requestSender != null) {
		requestSender.open(method, url, true);
		requestSender.onreadystatechange = function() {
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

R.musicSetVolume = function() {};

R.musicShouldLoopByFilename = {};
R.musicPlayNow = function(audioPath, audioObject, loop) {
	audioObject.currentTime = 0;
	R.musicShouldLoopByFilename[audioPath] = loop;
	R.currentlyPlayingMusic = audioObject;
	audioObject.play();
};

R.musicPause = function() {
	if (R.currentlyPlayingMusic != null) {
		R.currentlyPlayingMusic.pause();
	}
};

R.musicResume = function() {
	if (R.currentlyPlayingMusic != null) {
		R.currentlyPlayingMusic.play();
	}
};

R.currentlyPlayingMusic = null;

R.musicLoadFromResource = function(filepath, statusOut) {
	statusOut[0] = 0;
	var audioObject = new Audio(filepath);
	R.musicShouldLoopByFilename[filepath] = false;
	audioObject.addEventListener('ended', function() {
		if (R.musicShouldLoopByFilename[filepath]) {
			this.currentTime = 0;
			this.play();
		}
	}, false);
	return v_instantiateMusicInstance(filepath, audioObject, filepath, true);
};


R.pumpAsyncMessageQueue = function() {
	return null;
};

window.addEventListener('keydown', function(e) {
	if ([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
		e.preventDefault();
	}
}, false);
