var HEXR = [];
var HEX = [];
var hex = '0123456789abcdef';
for (var num = 0; num < 256; ++num) {
	var n = hex.charAt(num >> 4) + hex.charAt(num & 15);
	HEX.push(n);
	HEXR.push('#' + n);
}

function performRender(ev) {
	var evLength = ev.length;
    var images = [];
    var imagesIndex = 0;
    var mask = 0;
    var x = 0;
    var y = 0;
    var w = 0;
    var h = 0;
    var r = 0;
    var g = 0;
    var b = 0;
    var a = 0;
    var tw = 0;
    var th = 0;
    var sx = 0;
    var sy = 0;
    var sw = 0;
    var sh = 0;
    var alpha = 0;
    var theta = 0;
    var radiusX = 0;
    var radiusY = 0;
    var path;
    var offset;
    var font;
    var text;
    var textIndex = 0;

	var canvas = document.getElementById('crayon_render_canvas');
	var ctx = canvas.getContext('2d');

    for (var i = 0; i < evLength; i += 16) {
        switch (ev[i]) {
            case 1:
                // rectangle
                x = ev[i | 1];
                y = ev[i | 2];
                w = ev[i | 3];
                h = ev[i | 4];
                r = ev[i | 5];
                g = ev[i | 6];
                b = ev[i | 7];
                a = ev[i | 8];

                ctx.fillStyle = HEXR[r] + HEX[g] + HEX[b];
                if (a != 255) {
                    ctx.globalAlpha = a / 255.0;
                    ctx.fillRect(x, y, w + .1, h + .1); // TODO: get to the bottom of this mysterious .1. Is it still necessary?
                    ctx.globalAlpha = 1;
                } else {
                    ctx.fillRect(x, y, w + .1, h + .1);
                }
                break;
            default:
            	console.log("TODO: " + ev[i] + " events");
            	break;
        }
    }
}

function receiveMessage(type, msg) {
	switch (type) {
		case 'clock-tick':
			clockTick(msg);
			break;
		case 'screen-size':
			var parts = msg.split(',');
			var canvas = document.getElementById('crayon_render_canvas');
			canvas.width = parseInt(parts[0]);
			canvas.height = parseInt(parts[1]);
			canvas.style.width = '100%';
			canvas.style.height = '100%';
			break;
		default:
			console.log("Unknown message type received on JS end: " + type);
			break;
	}
}

function clockTick(nums) {
	var renderEvents = decodeInstructions(nums);
	performRender(renderEvents);
	window.setTimeout(triggerNextFrame, 33);
}

function triggerNextFrame() {
	sendMessage('trigger-next-frame', '.');
}

function sendMessage(type, msg) {
	JavaScriptBridge.onSendNativeMessage(type, msg);
}

function decodeInstructions(nums) {
	var values = nums.split(' ');
	var output = [];
	var length = values.length;
	for (var i = 0; i < length; ++i) {
		output[i] = parseInt(values[i]);
	}
	return output;
}
