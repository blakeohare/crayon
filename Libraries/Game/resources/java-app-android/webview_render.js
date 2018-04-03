var HEXR = [];
var HEX = [];
var hex = '0123456789abcdef';
for (var num = 0; num < 256; ++num) {
	var n = hex.charAt(num >> 4) + hex.charAt(num & 15);
	HEX.push(n);
	HEXR.push('#' + n);
}

function performRender(ev, imageIds) {
	var evLength = ev.length;
    var imagesIndex = 0;
    var image = null;
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

            case 6:
                textureId = imageIds[imagesIndex++];
                image = loadedImages[textureId];
                if (image) {
                    x = ev[i | 8];
                    y = ev[i | 9];
                    w = image.width;
                    h = image.height;
                    mask = ev[i | 1];
                    if (mask == 0) {
                        // basic case
                        ctx.drawImage(image, 0, 0, w, h, x, y, w, h);
                    } else if ((mask & 4) != 0) {
                        // rotation is involved
                        theta = ev[i | 10] / 1048576.0;
                        if ((mask & 3) == 0) {
                            ctx.save();
                            ctx.translate(x, y);
                            ctx.rotate(theta);

                            if ((mask & 8) == 0) {
                                ctx.drawImage(image, -w / 2, -h / 2);
                            } else {
                                ctx.globalAlpha = ev[i | 11] / 255;
                                ctx.drawImage(image, -w / 2, -h / 2);
                                ctx.globalAlpha = 1;
                            }
                            ctx.restore();
                        } else {
                            // TODO: slice and scale a picture and rotate it.
                        }
                    } else {
                        // no rotation
                        if ((mask & 1) == 0) {
                            sx = 0;
                            sy = 0;
                            sw = w;
                            sh = h;
                        } else {
                            sx = ev[i | 2];
                            sy = ev[i | 3];
                            sw = ev[i | 4];
                            sh = ev[i | 5];
                        }
                        if ((mask & 2) == 0) {
                            tw = sw;
                            th = sh;
                        } else {
                            tw = ev[i | 6];
                            th = ev[i | 7];
                        }

                        if ((mask & 8) == 0) {
                            ctx.drawImage(image, sx, sy, sw, sh, x, y, tw, th);
                        } else {
                            ctx.globalAlpha = ev[i | 11] / 255;
                            ctx.drawImage(image, sx, sy, sw, sh, x, y, tw, th);
                            ctx.globalAlpha = 1;
                        }
                    }
                } else {
                    console.log("Image not loaded yet: " + textureId)
                }
                break;

            default:
            	console.log("TODO: " + ev[i] + " events");
            	break;
        }
    }
}

function msgDecode(value) {
    var nums = value.split(' ');
    var output = [];
    var length = nums.length;
    for (var i = 0; i < length; ++i) {
        output.push(String.fromCharCode(parseInt(nums[i])));
    }
    return output.join('');
}

var loadedImages = {};
function receiveMessage(type, msg, encoded) {
    var msgOriginal = msg;
    if (encoded) {
        type = msgDecode(type);
        msg = msgDecode(msg);
    }
	var parts = msg.split(',');
	switch (type) {
		case 'clock-tick':
			clockTick(parts[0], parts[1]);
			break;
		case 'screen-size':
			var canvas = document.getElementById('crayon_render_canvas');
			canvas.width = parseInt(parts[0]);
			canvas.height = parseInt(parts[1]);
			canvas.style.width = '100%';
			canvas.style.height = '100%';
			break;
		case 'load-image':
		    var textureId = parseInt(parts[0]);
		    var imageData = 'data:image/png;base64,' + parts[1];
		    var imageLoader = new Image();
		    imageLoader.onload = function() {
		        var canvas = document.createElement('canvas');
                var body = document.getElementsByTagName("body")[0];
                body.appendChild(canvas);
		        var ctx = canvas.getContext('2d');
		        ctx.drawImage(imageLoader, 0, 0);
                loadedImages[textureId] = canvas;
		        console.log("Finished loading image #" + textureId);
		    };
		    imageLoader.src = imageData;
		    console.log("Loading image #" + textureId);
			break;

		default:
			console.log("Unknown message type received on JS end: " + type);
			break;
	}
}

function clockTick(nums, imageIds) {
	var renderEvents = decodeInstructions(nums);
	performRender(renderEvents, decodeInstructions(imageIds));
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
