
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
	var rwidth = R.globals.real_canvas.width;
	var rheight = R.globals.real_canvas.height;
	var vwidth = R.globals.virtual_canvas.width;
	var vheight = R.globals.virtual_canvas.height;

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
    var obj = R.globals.real_canvas;
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
	var keycode = ev.which ? ev.which : ev.keyCode;
	if (!!keycode) {
		if (keycode == 59) keycode = 186; // semicolon oddities different across browsers
		if (keycode == 92) keycode--; // left-windows key and right-windows key is just one enum value.
		if (keycode == 173) keycode = 189; // hyphen

		if (down && R._pressed_keys[keycode]) {
			// do not allow key repeats.
			return;
		}
		R._pressed_keys[keycode] = down;
		R._eventRelays.push(v_buildRelayObj(down ? 16 : 17, keycode, 0, 0, 0, ''));
	}
};
