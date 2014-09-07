var R = {};

R.now = function () {
	return (Date.now ? Date.now() : new Date().getTime()) / 1000.0;
};

R.get_image_impl = function(key) {
	return [%%%TYPE_NATIVE_OBJECT%%%, [%%%TYPE_NATIVE_OBJECT_IMAGE%%%, R._global_vars.image_downloads[key]]];
};

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

R.blit = function(canvas, x, y) {
	R._global_vars.ctx.drawImage(canvas, x, y);5
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
};

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
	R._global_vars['width'] = width;
	R._global_vars['height'] = height;

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
	var ctx = R._global_vars.ctx;
	ctx.fillStyle = R._toHex(r, g, b);
	ctx.fillRect(x, y, width, height);
};

R.drawEllipse = function(centerX, centerY, radiusX, radiusY, r, g, b) {
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
	context.fill();
	context.closePath();
};

R.drawLine = function(startX, startY, endX, endY, width, r, g, b) {
	var context = R._global_vars.ctx;
	var offset = ((width % 2) == 0) ? 0 : .5;
	context.beginPath();
	context.moveTo(startX + offset, startY + offset);
	context.lineTo(endX + offset, endY + offset);
	context.lineWidth = width;
	context.strokeStyle = R._toHex(r, g, b);
	context.stroke();
	context.closePath();
};

window.addEventListener('keydown', function(e) {
	if ([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
		e.preventDefault();
	}
}, false);
