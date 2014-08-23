var R = {};

R.now = function () {
    return (Date.now ? Date.now() : new Date().getTime()) / 1000.0;
};

R.get_image_impl = function(key) {
    return [%%%TYPE_NATIVE_OBJECT%%%, [%%%TYPE_NATIVE_OBJECT_IMAGE%%%, R._global_vars.image_downloads[key]]];
}

R._global_vars = {
    'width': 0,
    'height': 0,
    'fps': 60,
    'canvas': null,
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

R.blit = function(ignored, canvas, x, y) {
    R._global_vars.ctx.drawImage(canvas, x, y);
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
    img.src = url;

};

R.finish_load_image = function(id) {
    var key = R._global_vars.image_keys_by_index[id];
    var img = document.getElementById('image_loader_img_' + id);
    var canvas = document.getElementById('image_loader_canvas_' + id);
    canvas.width = img.width;
    canvas.height = img.height;
    var context = canvas.getContext('2d');
    context.drawImage(img, 0, 0);
    R._global_vars.image_downloads[key] = canvas;
}

R.beginFrame = function() {
    R._global_vars.last_frame_began = R.now();
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
R.initializeScreen = function (width, height) {
    var canvasHost = document.getElementById('crayon_host');
    canvasHost.innerHTML =
        '<canvas id="crayon_screen" width="' + width + '" height="' + height + '"></canvas>' +
        '<div style="display:none;">' +
            '<img id="crayon_image_loader" onload="Q._finish_loading()" crossOrigin="anonymous" />' +
            '<div id="crayon_image_loader_queue"></div>' +
            '<div id="crayon_image_store"></div>' +
            '<div id="crayon_temp_image"></div>' +
        '</div>' +
        '<div style="font-family:&quot;Courier New&quot;; font-size:11px;" id="crayon_print_output"></div>';
    var canvas = document.getElementById('crayon_screen');
    R._global_vars['canvas'] = canvas;
    R._global_vars['image_loader'] = document.getElementById('crayon_image_loader');
    R._global_vars['image_store'] = document.getElementById('crayon_image_store');
    R._global_vars['temp_image'] = document.getElementById('crayon_temp_image');
    R._global_vars['print_output'] = document.getElementById('crayon_print_output');
    R._global_vars['ctx'] = canvas.getContext('2d');
    R._global_vars['print_lines'] = [];

    document.onkeydown = R._keydown;
    document.onkeyup = R._keyup;
    
    canvas.addEventListener('mousedown', R._mousedown);
    canvas.addEventListener('mouseup', R._mouseup);
    canvas.addEventListener('mousemove', R._mousemove);
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
    var x = Math.floor(pos[0]);
    var y = Math.floor(pos[1]);
    var data = [];

    if (click) {
        var rightclick = false;
        if (!ev) ev = window.event;
        if (ev.which) rightclick = (ev.which == 3);
        else if (ev.button) rightclick = (ev.button == 2);
        data.push(R._commonStrings['s_mouse' + (rightclick ? 'right' : 'left') + (down ? 'down' : 'up')]);
    } else {
        data.push(R._commonStrings.s_mousemove);
    }
    data.push([%%%TYPE_INTEGER%%%, x]);
    data.push([%%%TYPE_INTEGER%%%, y]);

    R._global_vars.event_queue.push([%%%TYPE_LIST%%%, data]);
};

R._mouse_get_pos_from_event = function (ev) {
	var obj = R._global_vars.canvas;
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
};

R._commonStrings = {
    s_key: [%%%TYPE_STRING%%%, 'key'],
    s_mouseleftdown: [%%%TYPE_STRING%%%, 'mouseleftdown'],
    s_mouseleftup: [%%%TYPE_STRING%%%, 'mouseleftup'],
    s_mousemove: [%%%TYPE_STRING%%%, 'mousemove'],
    s_mouserightdown: [%%%TYPE_STRING%%%, 'mouserightdown'],
    s_mouserightup: [%%%TYPE_STRING%%%, 'mouserightup']
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
        R._global_vars.event_queue.push([%%%TYPE_LIST%%%, [R._commonStrings.s_key, down ? v_VALUE_TRUE : v_VALUE_FALSE, keycode]]);
    }
};

R._keyCodeLookup = {
	'k13': 'enter',
	'k16': 'shift', 'k17': 'ctrl', 'k18': 'alt',
	'k32': 'space',
	
	'k48': '0', 'k49': '1', 'k50': '2', 'k51': '3', 'k52': '4',
	'k53': '5', 'k54': '6', 'k55': '7', 'k56': '8', 'k57': '9',
	
	'k65': 'a', 'k66': 'b', 'k67': 'c', 'k68': 'd', 'k69': 'e',
	'k70': 'f', 'k71': 'g', 'k72': 'h', 'k73': 'i', 'k74': 'j',
	'k75': 'k', 'k76': 'l', 'k77': 'm', 'k78': 'n', 'k79': 'o',
	'k80': 'p', 'k81': 'q', 'k82': 'r', 'k83': 's', 'k84': 't',
	'k85': 'u', 'k86': 'v', 'k87': 'w', 'k88': 'x', 'k89': 'y',
	'k90': 'z',
	
	'k37': 'left', 'k38': 'up', 'k39': 'right', 'k40': 'down',
	
	'k187': '=',
	'k189': '-'
};

for (var _key in R._keyCodeLookup) {
    R._keyCodeLookup[_key] = [%%%TYPE_STRING%%%, R._keyCodeLookup[_key]];
}

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

R.drawRect = function (x, y, width, height, r, g, b) {
    var gb = R._global_vars;
    gb.ctx.fillStyle = R._toHex(r, g, b);
    gb.ctx.fillRect(x, y, width, height);
};

/////////////////////////////////

var Q = {};
Q._hexDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'];
Q._image_load_queue = [];
Q._loading_currently = null;
Q._images = {};
Q._functions = {};

Q._useFrameRate = 60;

Q.setFrameRate = function(fps) {
	Q._useFrameRate = fps;
};

Q.setUpdateMethod = function(pointer) {
	Q._functions['updater'] = pointer;
};

Q.setRenderMethod = function(pointer) {
	Q._functions['renderer'] = pointer;
};

Q.setMainMethod = function(pointer) {
	Q._functions['main'] = pointer;
};

Q.setKeyDown = function(pointer) {
	Q._functions['keydown'] = pointer;
};

Q.setKeyUp = function(pointer) {
	Q._functions['keyup'] = pointer;
};

Q.setMouseDown = function(pointer) {
	Q._functions['mousedown'] = pointer;
};

Q.setMouseUp = function(pointer) {
	Q._functions['mouseup'] = pointer;
};

Q.setMouseMove = function(pointer) {
	Q._functions['mousemove'] = pointer;
};

Q.setMouseDrag = function(pointer) {
	Q._functions['mousedrag'] = pointer;
};

Q.setMouseDoubleClick = function(pointer) {
	Q._functions['mousedoubleclick'] = pointer;
};

Q.setMainMethod(null);
Q.setMouseDown(null);
Q.setMouseUp(null);
Q.setMouseMove(null);
Q.setMouseDrag(null);
Q.setMouseDoubleClick(null);
Q.setKeyDown(null);
Q.setKeyUp(null);

Q.setHostDiv = function (hostId) {
	var div = document.getElementById(hostId);
	div.innerHTML =
		'<canvas id="screen"></canvas>' +
		'<div style="display:none;">' +
			'<img id="image_loader" onload="Q._finish_loading()" crossOrigin="anonymous" />' +
			'<div id="image_store"></div>' +
			'<div id="temp_image"></div>' +
		'</div>' +
		'<div style="font-family:&quot;Courier New&quot;; font-size:11px;" id="_q_debug_output"></div>';
	if (Q._printedLines.length > 0) Q._synchPrintHost();
};

Q._htmlspecialchars = function(value) {
	var output = [];
	var c;
	for (var i = 0; i < value.length; ++i) {
		c = value.charAt(i);
		if (c == '<') { c = '&lt;'; }
		else if (c == '>') { c = '&gt;'; }
		else if (c == '&') { c = '&amp;'; }
		else if (c == '"') { c = '&quot;'; }
		else if (c == '\n') { c = '<br />'; }
		else if (c == '\t') { c = '&nbsp;&nbsp;&nbsp;&nbsp;'; }
		else if (c == ' ') { c = '&nbsp;'; }
		
		output.push(c);
	}
	
	return output.join('');
}

Q._printedLines = [];
Q.print = function (value) {
	var i;
	Q._printedLines.push('' + value);
	if (Q._printedLines.length > 20) {
		var newLines = [];
		for (i = 20; i > 0; --i) {
			newLines.push(Q._printedLines[Q._printedLines.length - i]);
		}
		Q._printedLines = newLines;
	}

	Q._synchPrintHost();
}

Q._synchPrintHost = function () {
	var output = '';
	for (i = 0; i < Q._printedLines.length; ++i) {
		output += Q._htmlspecialchars(Q._printedLines[i] + '\n');
	}

	var printHost = document.getElementById('_q_debug_output');
	if (printHost) printHost.innerHTML = output;
}

Q.loadImage = function(key, path) {
	Q._image_load_queue.push([key, path]);
};

Q.toHex = function(r, g, b) {
	var hd = Q._hexDigits;
	return '#'
		+ hd[r >> 4] + hd[r & 15]
		+ hd[g >> 4] + hd[g & 15]
		+ hd[b >> 4] + hd[b & 15];
};

Q.begin = function(width, height, color) {
	Q._game_width = width;
	Q._game_height = height;
	Q._game_bgcolor = color;
	Q._load_images();
};

Q._load_images = function() {
	if (Q._image_load_queue.length > 0) {
		image = Q._image_load_queue[Q._image_load_queue.length - 1];
		Q._loading_currently = image;
		Q._image_load_queue.length -= 1;
		document.getElementById('image_loader').src = image[1];
	} else {
		Q._load_complete();
	}
};

Q._finish_loading = function() {
	var index = Q._image_load_queue.length;
	document.getElementById('image_store').innerHTML += 
		'<canvas id="image_store_child_' + index + '"></canvas>';
	var loader = document.getElementById('image_loader');
	var canvas = document.getElementById('image_store_child_' + index);
	
	canvas.width = loader.width;
	canvas.height = loader.height
	var context = canvas.getContext('2d');
	context.drawImage(loader, 0, 0)
	
	Q._images[Q._loading_currently[0]] = new Q.Image(canvas);
	Q._load_images();
};

Q._load_complete = function() {
	var screenCanvas = document.getElementById('screen');
	screenCanvas.width = Q._game_width;
	screenCanvas.height = Q._game_height;
	
	screenCanvas.addEventListener('mousedown', Q._mousedown);
	screenCanvas.addEventListener('mouseup', Q._mouseup);
	screenCanvas.addEventListener('mousemove', Q._mousemove);
	
	document.onkeydown = Q._keydown;
	document.onkeyup = Q._keyup;
	
	var bg = Q._game_bgcolor;
	Q.screen = new Q.Image(screenCanvas);
	Q.screen.fill(bg[0], bg[1], bg[2]);
	Q._functions['main']();
	if (Q._useFrameRate !== null) {
		Q._doTick();
	}
};

Q._doTick = function() {
	
	var start = (new Date()).getTime();
	Q._functions['updater']();
	Q._functions['renderer'](Q.screen);
	
	var end = (new Date()).getTime();
	var diff = end - start;
	var delay = Math.floor(1000.0 / Q._useFrameRate) - diff;
	if (delay < 0) {
		delay = 0;
	}
	window.setTimeout('Q._doTick()', delay);
};

Q._scaleFactor = 1;

Q.setScaleFactor = function(factor) {
	Q._scaleFactor = factor;
}

Q._keydown = function(ev) {
	var key = Q._getKeyCode(ev);
	if (key != null) {
		var fp = Q._functions['keydown'];
		if (fp) {
			fp(key);
		}
		Q._isKeyPressed[key] = true;
	}
};

Q._keyup = function(ev) {
	var key = Q._getKeyCode(ev);
	if (key != null) {
		var fp = Q._functions['keyup'];
		if (fp) {
			fp(key);
		}
		Q._isKeyPressed[key] = false;
	}
};

Q.isKeyPressed = function(key) {
	var output = Q._isKeyPressed[key];
	if (output === undefined) return false;
	return output;
};

Q._keyCodeLookup = {
	'k13': 'enter',
	'k16': 'shift', 'k17': 'ctrl', 'k18': 'alt',
	'k32': 'space',
	
	'k48': '0', 'k49': '1', 'k50': '2', 'k51': '3', 'k52': '4',
	'k53': '5', 'k54': '6', 'k55': '7', 'k56': '8', 'k57': '9',
	
	'k65': 'a', 'k66': 'b', 'k67': 'c', 'k68': 'd', 'k69': 'e',
	'k70': 'f', 'k71': 'g', 'k72': 'h', 'k73': 'i', 'k74': 'j',
	'k75': 'k', 'k76': 'l', 'k77': 'm', 'k78': 'n', 'k79': 'o',
	'k80': 'p', 'k81': 'q', 'k82': 'r', 'k83': 's', 'k84': 't',
	'k85': 'u', 'k86': 'v', 'k87': 'w', 'k88': 'x', 'k89': 'y',
	'k90': 'z',
	
	'k37': 'left', 'k38': 'up', 'k39': 'right', 'k40': 'down',
	
	'k187': '=',
	'k189': '-'
};

Q._isKeyPressed = {};

Q._getKeyCode = function(ev) {
	var keyCode = ev.which ? ev.which : ev.keyCode;
	var output = Q._keyCodeLookup['k' + keyCode];
	return output === undefined ? null : output;
};

Q._last_mouse_down_loc = [0, 0];
Q._last_mouse_down_time = 0;
Q._last_click_was_double = false;

Q._mousedown = function (ev) {
	var pos = Q._mouse_get_pos_from_event(ev);
	Q._mouse_last_x = pos[0];
	Q._mouse_last_y = pos[1];
	Q._is_mouse_down = true;

	var fp = Q._functions['mousedown'];
	if (fp != null) {
		var rightclick = false;
		if (!ev) ev = window.event;
		if (ev.which) rightclick = (ev.which == 3);
		else if (ev.button) rightclick = (ev.button == 2);
		fp(pos[0], pos[1], !rightclick);
	}

	var time = (new Date()).getTime();
	var diff = time - Q._last_mouse_down_time;
	if (Q._last_click_was_double) {
		Q._last_click_was_double = false;
	} else {
		if (diff < 250) {
			var ppos = Q._last_mouse_down_loc;
			var dx = pos[0] - ppos[0];
			var dy = pos[1] - ppos[1];
			if (dx * dx + dy * dy < 100) { // within 10 pixels
				fp = Q._functions['mousedoubleclick'];
				if (fp != null) {
					fp(pos[0], pos[1]);
					Q._last_click_was_double = true;
				}
			}
		}
	}
	Q._last_mouse_down_time = time;
	Q._last_mouse_down_loc = [pos[0], pos[1]];
};

Q._mouseup = function(ev) {
	var pos = Q._mouse_get_pos_from_event(ev);
	Q._mouse_last_x = pos[0];
	Q._mouse_last_y = pos[1];
	Q._is_mouse_down = false;
	
	var fp = Q._functions['mouseup'];
	if (fp != null) {
		var rightclick = false;
		if (!ev) ev = window.event;
		if (ev.which) rightclick = (ev.which == 3);
		else if (ev.button) rightclick = (ev.button == 2);
		fp(pos[0], pos[1], !rightclick);
	}
};

Q._mousemove = function(ev) {
	var orig_x = Q._mouse_last_x;
	var orig_y = Q._mouse_last_y;
	var pos = Q._mouse_get_pos_from_event(ev);
	Q._mouse_last_x = pos[0];
	Q._mouse_last_y = pos[1];
	
	var fp = Q._functions['mousemove'];
	if (fp != null) {
		fp(orig_x, orig_y, pos[0], pos[1]);
	}
	
	if (Q._is_mouse_down) {
		fp = Q._functions['mousedrag'];
		if (fp != null) {
			fp(orig_x, orig_y, pos[0] - orig_x, pos[1] - orig_y);
		}
	}
};

Q._is_mouse_down = false;
Q._mouse_last_x = 0;
Q._mouse_last_y = 0;

Q._mouse_get_pos_from_event = function (ev) {
	var obj = Q.screen.canvas;
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

Q.getImage = function(name) {
	var output = Q._images[name];
	if (output === undefined) return null;
	return output;
}

Q.setImage = function(name, image) {
	Q._images[name] = image;
}

Q.Image = function() {
	this.imageData = null;
	if (arguments.length == 2) {
		this.width = arguments[0];
		this.height = arguments[1];
		var temp_image = document.getElementById('temp_image');
		temp_image.innerHTML = '<canvas id="temp_image_child"></canvas>';
		this.canvas = document.getElementById('temp_image_child');
		this.canvas.width = this.width;
		this.canvas.height = this.height;
		temp_image.innerHTML = '';
	} else if (arguments.length == 1) {
		this.canvas = arguments[0];
		this.width = this.canvas.width;
		this.height = this.canvas.height;
	}
	this.context = this.canvas.getContext('2d');
};

Q.Image.prototype.blit = function(image, x, y) {
	this.context.drawImage(image.canvas, x, y);
};

Q.Image.prototype.fill = function(r, g, b) {
	this.context.fillStyle = Q.toHex(r, g, b);
	this.context.fillRect(0, 0, this.width, this.height);
}

Q.Image.prototype.beginPixelEditing = function() {
	this.imageData = this.context.getImageData(0, 0, this.width, this.height);
};

Q.Image.prototype.endPixelEditing = function() {
	this.context.putImageData(this.imageData, 0, 0);
	this.imageData = null;
};

Q.Image.prototype.setPixel = function(x, y, r, g, b, a) {
	var index = (x + y * this.width) * 4
	this.imageData.data[index] = r;
	this.imageData.data[index + 1] = g;
	this.imageData.data[index + 2] = b;
	this.imageData.data[index + 3] = a;
};

Q.Image.prototype.swapColor = function(colorA, colorB) {
	this.beginPixelEditing();
	
	var pixelCount = this.width * this.height;
	
	var oldR = colorA[0];
	var oldG = colorA[1];
	var oldB = colorA[2];
	var oldA = colorA[3];
	
	var newR = colorB[0];
	var newG = colorB[1];
	var newB = colorB[2];
	var newA = colorB[3];
	
	var totalBytes = pixelCount * 4;
	for (var i = 0; i < totalBytes; i += 4) {
		if (this.imageData.data[i] == oldR &&
			this.imageData.data[i + 1] == oldG &&
			this.imageData.data[i + 2] == oldB &&
			this.imageData.data[i + 3] == oldA) {
			this.imageData.data[i] = newR;
			this.imageData.data[i + 1] = newG;
			this.imageData.data[i + 2] = newB;
			this.imageData.data[i + 3] = newA;
		}
	}
	
	this.endPixelEditing();
};

function sys_array_add_all(array, items) {
	for (var i = 0; i < items.length; ++i) {
		array.push(items[i]);
	}
}

function sys_array_clear(array) {
	array.splice(0, array.length);
}

Q.Draw = {};

Q.Draw.rectangle = function(image, x, y, width, height, border, r, g, b) {
	var context = image.context;
	context.fillStyle = Q.toHex(r, g, b);
	context.fillRect(x, y, 1, height);
	context.fillRect(x, y, width, 1);
	context.fillRect(x + width - 1, y, 1, height);
	context.fillRect(x, y + height - 1, width, 1);
};

Q.Draw.rectangleFilled = function(image, x, y, width, height, r, g, b) {
	var context = image.context;
	context.fillStyle = Q.toHex(r, g, b);
	context.fillRect(x, y, width, height);
};

Q.Draw.ellipse = function(image, centerX, centerY, radiusX, radiusY, r, g, b) {
	radiusX = radiusX * 4 / 3; // no idea...
	var context = image.context;
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
	context.fillStyle = Q.toHex(r, g, b);
	context.fill();
	context.closePath();
};

Q.Draw.line = function(image, startX, startY, endX, endY, width, r, g, b) {
	var context = image.context;
	var offset = ((width % 2) == 0) ? 0 : .5;
	context.beginPath();
	context.moveTo(startX + offset, startY + offset);
	context.lineTo(endX + offset, endY + offset);
	context.lineWidth = width;
	context.strokeStyle = Q.toHex(r, g, b);
	context.stroke();
	context.closePath();
};

Q.Draw.text = function(image, text, size, x, y, r, g, b) {
	var context = image.context;
	context.fillStyle = Q.toHex(r, g, b);
	context.font = "bold " + (1.3 * size) + "px Arial";
	context.fillText(text, x, y + size + 3);
};

Q.stretchImage = function(imageSource, imageTarget) {
	imageTarget.context.drawImage(imageSource.canvas, 0, 0, imageTarget.width, imageTarget.height);
};


function setup() {
	Q.setHostDiv('host');
	Q.setMainMethod(dd_main);
	Q.setUpdateMethod(dd_update);
	Q.setRenderMethod(dd_render);
	Q.setKeyDown(dd_keydown);
	Q.setKeyUp(dd_keyup);
	Q.setFrameRate(30);
	
	for (var i = 0; i < IMAGE_KEYS.length; ++i) {
		Q.loadImage(IMAGE_KEYS[i], '/files/arcade/dungeondude/' + IMAGE_KEYS[i] + '.png');
	}
	
	Q.begin(640, 480, [0, 0, 0]);
}

window.addEventListener('keydown', function(e) {
	if ([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
		e.preventDefault();
	}
}, false);
