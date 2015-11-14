
R.blitPartial = function (canvas, tx, ty, tw, th, sx, sy, sw, sh) {
	if (tw == 0 || th == 0 || sw == 0 || sh == 0) return;

	R._global_vars.ctx.drawImage(canvas, sx, sy, sw, sh, tx, ty, tw, th);
};

R.drawImageWithAlpha = function (canvas, x, y, a) {
	if (a == 0) return;
	var ctx = R._global_vars.ctx;
	if (a != 255) {
		ctx.globalAlpha = a / 255;
		ctx.drawImage(canvas, 0, 0, canvas.width, canvas.height, x, y, canvas.width, canvas.height);
		ctx.globalAlpha = 1;
	} else {
		ctx.drawImage(canvas, 0, 0, canvas.width, canvas.height, x, y, canvas.width, canvas.height);
	}
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

R.drawTriangle = function (ax, ay, bx, by, cx, cy, r, g, b, a) {
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

R.drawEllipse = function (left, top, width, height, r, g, b, alpha) {
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

R.drawLine = function (startX, startY, endX, endY, width, r, g, b, a) {
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

R.flipImage = function (canvas, flipX, flipY) {
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

R.scaleImage = function (originalCanvas, width, height) {
	var output = document.createElement('canvas');
	var context = output.getContext('2d');
	output.width = width;
	output.height = height;
	context.webkitImageSmoothingEnabled = false;
	context.mozImageSmoothingEnabled = false;
	context.msImageSmoothingEnabled = false;
	context.imageSmoothingEnabled = false;
	context.drawImage(
		originalCanvas,
		0, 0, originalCanvas.width, originalCanvas.height,
		0, 0, width, height);
	return output;
};
