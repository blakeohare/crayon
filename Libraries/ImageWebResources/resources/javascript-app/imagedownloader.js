LIB$imagewebresources$download = function(url, output) {
    var image = new Image();
    image.onerror = function () {
		output[0] = true;
		output[1] = false;
		output[2] = null;
    };
    image.onload = function () {
		output[0] = true;
		var width = image.width;
		var height = image.height;
		if (width < 1 || height < 1) {
			output[1]  = false;
		} else {
			var canvas = document.createElement('canvas');
			canvas.width = width;
			canvas.height = height;
			var ctx = canvas.getContext('2d');
			ctx.drawImage(image, 0, 0);
			output[1]  = true;
			output[2] = canvas;
			output[3] = width;
			output[4] = height;
		}
    };
    image.src = url;
};
