
R.blitRotated = function (canvas, x, y, theta) {
    var ctx = R.globals.ctx;
	ctx.save();
	ctx.translate(x, y);
	ctx.rotate(theta);
	ctx.drawImage(canvas, -canvas.width / 2, -canvas.height / 2);
	ctx.restore();
};

R.blitPartial = function (canvas, tx, ty, tw, th, sx, sy, sw, sh) {
	if (tw == 0 || th == 0 || sw == 0 || sh == 0) return;

	R.globals.ctx.drawImage(canvas, sx, sy, sw, sh, tx, ty, tw, th);
};

R.drawImageWithAlpha = function (canvas, x, y, a) {
	if (a == 0) return;
	var ctx = R.globals.ctx;
	if (a != 255) {
		ctx.globalAlpha = a / 255;
		ctx.drawImage(canvas, 0, 0, canvas.width, canvas.height, x, y, canvas.width, canvas.height);
		ctx.globalAlpha = 1;
	} else {
		ctx.drawImage(canvas, 0, 0, canvas.width, canvas.height, x, y, canvas.width, canvas.height);
	}
};

var HEX = [];
var HEXR = [];
for (var i = 0; i < 256; ++i) {
	var t = i.toString(16);
	if (i < 16) t = '0' + t;
	HEX.push(t);
	HEXR.push('#' + t);
}

R.fillScreen = function (r, g, b) {
    var gb = R.globals;
	gb.ctx.fillStyle = HEXR[r] + HEX[g] + HEX[b];
	gb.ctx.fillRect(0, 0, gb.width, gb.height);
};

R.drawRect = function (x, y, width, height, r, g, b, a) {
    var ctx = R.globals.ctx;
	ctx.fillStyle = HEXR[r] + HEX[g] + HEX[b];
	if (a != 255) {
		ctx.globalAlpha = a / 255;
		ctx.fillRect(x, y, width + .1, height + .1);
		ctx.globalAlpha = 1;
	} else {
		ctx.fillRect(x, y, width + .1, height + .1);
	}
};

R.drawTriangle = function (ax, ay, bx, by, cx, cy, r, g, b, a) {
	if (a == 0) return;
	var ctx = R.globals.ctx;

	var tpath = new Path2D();
	tpath.moveTo(ax, ay);
	tpath.lineTo(bx, by);
	tpath.lineTo(cx, cy);

	ctx.fillStyle = HEXR[r] + HEX[g] + HEX[b];
	if (a != 255) {
		ctx.globalAlpha = a / 255;
		ctx.fill(tpath);
		ctx.globalAlpha = 1;
	} else {
		ctx.fill(tpath);
	}
};

R.drawEllipse = function (left, top, width, height, r, g, b, alpha) {
	var radiusX = width / 2;
	var radiusY = height / 2;
	var centerX = left + radiusX;
	var centerY = top + radiusY;

	var ctx = R.globals.ctx;
	radiusX = radiusX * 4 / 3; // no idea...
	ctx.beginPath();
	ctx.moveTo(centerX, centerY - radiusY);
	ctx.bezierCurveTo(
		centerX + radiusX, centerY - radiusY,
		centerX + radiusX, centerY + radiusY,
		centerX, centerY + radiusY);
	ctx.bezierCurveTo(
		centerX - radiusX, centerY + radiusY,
		centerX - radiusX, centerY - radiusY,
		centerX, centerY - radiusY);
	ctx.fillStyle = HEXR[r] + HEX[g] + HEX[b];
	if (alpha != 255) {
	    ctx.globalAlpha = alpha / 255;
		ctx.fill();
		ctx.closePath();
		ctx.globalAlpha = 1;
	} else {
	    ctx.fill();
	    ctx.closePath();
	}
};

R.drawLine = function (startX, startY, endX, endY, width, r, g, b, a) {
    var ctx = R.globals.ctx;
	var offset = ((width % 2) == 0) ? 0 : .5;
	ctx.beginPath();
	ctx.moveTo(startX + offset, startY + offset);
	ctx.lineTo(endX + offset, endY + offset);
	ctx.lineWidth = width;
	if (a != 255) {
	    ctx.globalAlpha = a / 255;
	    ctx.strokeStyle = HEXR[r] + HEX[g] + HEX[b];
	    ctx.stroke();
	    ctx.closePath();
	    ctx.globalAlpha = 1;
	} else {
	    ctx.strokeStyle = HEXR[r] + HEX[g] + HEX[b];
	    ctx.stroke();
	    ctx.closePath();
	}
};

R.flipImage = function (canvas, flipX, flipY) {
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

R.scaleImage = function (originalCanvas, width, height) {
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
