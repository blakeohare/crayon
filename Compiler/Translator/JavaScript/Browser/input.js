
R._eventRelays = [];

R._mousedown = function (ev) {
	R._mousething(ev, true, true);
};

R._mouseup = function (ev) {
	R._mousething(ev, true, false);
};

R._mousemove = function (ev) {
	R._mousething(ev, false, 'ignored');
};

R._mousething = function (ev, click, down) {
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
		R._eventRelays.push(v_buildRelayObj(33 + (rightclick ? 2 : 0) + (down ? 0 : 1), x, y, 0, 0, ''));
	} else {
		R._eventRelays.push(v_buildRelayObj(32, x, y, 0, 0, ''));
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

R._keydown = function (ev) {
	R._keydownup(ev, true);
};

R._keyup = function (ev) {
	R._keydownup(ev, false);
};

R._pressed_keys = {};

R._keydownup = function (ev, down) {
	var keycode = R._getKeyCode(ev);
	if (keycode != null) {
		if (down && R._pressed_keys[keycode]) {
			// do not allow key repeats.
			return;
		}
		R._pressed_keys[keycode] = down;
		R._eventRelays.push(v_buildRelayObj(down ? 16 : 17, keycode, 0, 0, 0, ''));
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
	'k186': 'semicolon',
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
