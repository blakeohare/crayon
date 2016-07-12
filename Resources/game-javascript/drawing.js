

if (!C$game) { throw 1; } // Cannot use the drawing library without the game library.

// Lookups for fast hex conversion
C$drawing = 1;
C$drawing$HEX = [];
C$drawing$HEXR = [];

for (var i = 0; i < 256; ++i) {
    var t = i.toString(16);
    if (i < 16) t = '0' + t;
    C$drawing$HEX.push(t);
    C$drawing$HEXR.push('#' + t);
}

C$drawing$blitRotated = function (canvas, x, y, theta) {
    C$game$ctx.save();
    C$game$ctx.translate(x, y);
    C$game$ctx.rotate(theta);
    C$game$ctx.drawImage(canvas, -canvas.width / 2, -canvas.height / 2);
    C$game$ctx.restore();
};

C$drawing$blitPartial = function (canvas, tx, ty, tw, th, sx, sy, sw, sh) {
	if (tw == 0 || th == 0 || sw == 0 || sh == 0) return;

	C$game$ctx.drawImage(canvas, sx, sy, sw, sh, tx, ty, tw, th);
};

C$drawing$drawImageWithAlpha = function (canvas, x, y, a) {
	if (a == 0) return;
	if (a != 255) {
	    C$game$ctx.globalAlpha = a / 255;
	    C$game$ctx.drawImage(canvas, 0, 0, canvas.width, canvas.height, x, y, canvas.width, canvas.height);
	    C$game$ctx.globalAlpha = 1;
	} else {
	    C$game$ctx.drawImage(canvas, 0, 0, canvas.width, canvas.height, x, y, canvas.width, canvas.height);
	}
};

C$drawing$fillScreen = function (r, g, b) {
    C$game$ctx.fillStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
    C$game$ctx.fillRect(0, 0, C$game$width, C$game$height);
};

C$drawing$drawRect = function (x, y, width, height, r, g, b, a) {
    C$game$ctx.fillStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
	if (a != 255) {
	    C$game$ctx.globalAlpha = a / 255;
	    C$game$ctx.fillRect(x, y, width + .1, height + .1);
	    C$game$ctx.globalAlpha = 1;
	} else {
	    C$game$ctx.fillRect(x, y, width + .1, height + .1);
	}
};

C$drawing$drawTriangle = function (ax, ay, bx, by, cx, cy, r, g, b, a) {
	if (a == 0) return;

	var tpath = new Path2D();
	tpath.moveTo(ax, ay);
	tpath.lineTo(bx, by);
	tpath.lineTo(cx, cy);

	C$game$ctx.fillStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
	if (a != 255) {
	    C$game$ctx.globalAlpha = a / 255;
	    C$game$ctx.fill(tpath);
	    C$game$ctx.globalAlpha = 1;
	} else {
	    C$game$ctx.fill(tpath);
	}
};

C$drawing$drawEllipse = function (left, top, width, height, r, g, b, alpha) {
	var radiusX = width / 2;
	var radiusY = height / 2;
	var centerX = left + radiusX;
	var centerY = top + radiusY;

	radiusX = radiusX * 4 / 3; // no idea...
	C$game$ctx.beginPath();
	C$game$ctx.moveTo(centerX, centerY - radiusY);
	C$game$ctx.bezierCurveTo(
		centerX + radiusX, centerY - radiusY,
		centerX + radiusX, centerY + radiusY,
		centerX, centerY + radiusY);
	C$game$ctx.bezierCurveTo(
		centerX - radiusX, centerY + radiusY,
		centerX - radiusX, centerY - radiusY,
		centerX, centerY - radiusY);
	C$game$ctx.fillStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
	if (alpha != 255) {
	    C$game$ctx.globalAlpha = alpha / 255;
	    C$game$ctx.fill();
	    C$game$ctx.closePath();
	    C$game$ctx.globalAlpha = 1;
	} else {
	    C$game$ctx.fill();
	    C$game$ctx.closePath();
	}
};

C$drawing$drawLine = function (startX, startY, endX, endY, width, r, g, b, a) {
	var offset = ((width % 2) == 0) ? 0 : .5;
	C$game$ctx.beginPath();
	C$game$ctx.moveTo(startX + offset, startY + offset);
	C$game$ctx.lineTo(endX + offset, endY + offset);
	C$game$ctx.lineWidth = width;
	if (a != 255) {
	    C$game$ctx.globalAlpha = a / 255;
	    C$game$ctx.strokeStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
	    C$game$ctx.stroke();
	    C$game$ctx.closePath();
	    C$game$ctx.globalAlpha = 1;
	} else {
	    C$game$ctx.strokeStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
	    C$game$ctx.stroke();
	    C$game$ctx.closePath();
	}
};

C$drawing$flipImage = function (canvas, flipX, flipY) {
	var output = document.createElement('canvas');

	output.width = canvas.width;
	output.height = canvas.height;

	var ctx = output.getContext('2d');

	if (flipX) {
	    ctx.translate(canvas.width, 0);
	    ctx.scale(-1, 1);
	}
	if (flipY) {
	    ctx.translate(0, canvas.height);
	    ctx.scale(1, -1);
	}

	ctx.drawImage(canvas, 0, 0);

	if (flipX) {
	    ctx.scale(-1, 1);
	    ctx.translate(-canvas.width, 0);
	}
	if (flipY) {
	    ctx.scale(1, -1);
	    ctx.translate(0, -canvas.height);
	}

	return output;
};

C$drawing$scaleImage = function (originalCanvas, width, height) {
	var output = document.createElement('canvas');
	var ctx = output.getContext('2d');
	output.width = width;
	output.height = height;
	ctx.webkitImageSmoothingEnabled = false;
	ctx.mozImageSmoothingEnabled = false;
	ctx.msImageSmoothingEnabled = false;
	ctx.imageSmoothingEnabled = false;
	ctx.drawImage(
		originalCanvas,
		0, 0, originalCanvas.width, originalCanvas.height,
		0, 0, width, height);
	return output;
};
