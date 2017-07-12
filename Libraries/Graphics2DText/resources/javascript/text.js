LIB$graphics2dtext$createNativeFont = function(sourceType, fontClass, fontPath) {
	switch (sourceType) {
		case 0: // default
			throw "TODO";
		case 1: // resource
			var b64 = C$common$getBinaryResBase64(fontPath);
			var nativeFont = [fontPath, false, b64];
			LIB$graphics2dtext$loadFontStyle(nativeFont);
			return nativeFont;
		case 2: // file
			throw "TODO";
		case 3: // system font
			throw "TODO";
	}
};

LIB$graphics2dtext$fontLoaderCounter = 0;

LIB$graphics2dtext$loadFontStyle = function(nativeFont) {
	if (nativeFont[1]) return true; // already loaded
	
	var loader = document.getElementById('crayon_font_loader'); // window not opened yet
	if (!!loader) {
		var base64 = nativeFont[2];
		var fontFamily = 'CrayonFont' + ++LIB$graphics2dtext$fontLoaderCounter;
		loader.innerHTML += [
			'<style type="text/css">',
				'@font-face { ',
					"font-family: '" + fontFamily + "';",
					'src: url(data:font/truetype;char;charset=utf-8;base64,' + base64 + ") format('truetype');",
					'font-weight: normal;',
					'font-style: normal;',
				'}',
			'</style>'].join('\n');
		nativeFont[1] = true;
		nativeFont[2] = fontFamily;
		return true;
	}
	return false;
}

LIB$graphics2dtext$isSystemFontAvailable = function(name) {
	return false;
};

LIB$graphics2dtext$tempCanvas = document.createElement('canvas');

LIB$graphics2dtext$renderText = function(sizeout, nativeFont, size, isBold, isItalic, r, g, b, text) {

	if (!nativeFont[1]) LIB$graphics2dtext$loadFontStyle(nativeFont);
	if (!nativeFont[1]) throw false; // TODO: prevent renderText before game window is initialized
	
	var effectiveSize = Math.floor(size * 70 / 54 + .5);
	
	var ctx = LIB$graphics2dtext$tempCanvas.getContext('2d');
	// TODO: bold, italic
	var fontCss = effectiveSize + 'px ' + nativeFont[2];
	if (isBold) fontCss = 'bold ' + fontCss;
	if (isItalic) fontCss = 'italic ' + fontCss;
	ctx.font = fontCss;
	var measure = ctx.measureText(text);
	var width = Math.ceil(measure.width);
	var height = effectiveSize;
	var surface = C$imageresources$generateNativeBitmapOfSize(width, height * 4 / 3);
	ctx = surface.getContext('2d');
	ctx.font = fontCss;
	ctx.fillStyle = C$drawing$HEXR[r] + C$drawing$HEX[g] + C$drawing$HEX[b];
	ctx.fillText(text, 0, height);
	sizeout[0] = surface.width;
	sizeout[1] = surface.height;
	return surface;
};
