
C$input = 1;
C$input$eventRelays = [];
C$input$pressedKeys = {};

C$input$mousedown = function (ev) {
    C$input$mousething(ev, true, true);
};

C$input$mouseup = function (ev) {
    C$input$mousething(ev, true, false);
};

C$input$mousemove = function (ev) {
    C$input$mousething(ev, false, 'ignored');
};

C$input$mousething = function (ev, click, down) {
    var pos = C$input$getMousePos(ev);
	var x = pos[0];
	var y = pos[1];
	var rwidth = C$game$real_canvas.width;
	var rheight = C$game$real_canvas.height;
	var vwidth = C$game$virtual_canvas.width;
	var vheight = C$game$virtual_canvas.height;

	x = Math.floor(x * vwidth / rwidth);
	y = Math.floor(y * vheight / rheight);

	if (click) {
		var rightclick = false;
		if (!ev) ev = window.event;
		if (ev.which) rightclick = (ev.which == 3);
		else if (ev.button) rightclick = (ev.button == 2);
		var button = rightclick ? 'right' : 'left';
		C$input$eventRelays.push(v_buildRelayObj(33 + (rightclick ? 2 : 0) + (down ? 0 : 1), x, y, 0, 0, ''));
	} else {
	    C$input$eventRelays.push(v_buildRelayObj(32, x, y, 0, 0, ''));
	}
};

C$input$getMousePos = function (ev) {
    var obj = C$game$real_canvas;
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
		// Most browsers
		xpos = ev.pageX;
		ypos = ev.pageY;
	} else {
		// Legacy IE
		xpos = window.event.x + document.body.scrollLeft - 2;
		ypos = window.event.y + document.body.scrollTop - 2;
	}
	xpos -= obj_left;
	ypos -= obj_top;
	return [xpos, ypos];
};

C$input$keydown = function (ev) {
    C$input$keydownup(ev, true);
};

C$input$keyup = function (ev) {
    C$input$keydownup(ev, false);
};

C$input$keydownup = function (ev, down) {
	var keycode = ev.which ? ev.which : ev.keyCode;
	if (!!keycode) {
		if (keycode == 59) keycode = 186; // semicolon oddities different across browsers
		if (keycode == 92) keycode--; // left-windows key and right-windows key is just one enum value.
		if (keycode == 173) keycode = 189; // hyphen

		if (down && C$input$pressedKeys[keycode]) {
			// do not allow key repeats.
			return;
		}
		C$input$pressedKeys[keycode] = down;
		C$input$eventRelays.push(v_buildRelayObj(down ? 16 : 17, keycode, 0, 0, 0, ''));
	}
};
