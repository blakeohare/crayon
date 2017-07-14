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
					"font-family: " + fontFamily + ";",
					'src: url(data:font/truetype;charset=utf-8;base64,' + base64 + ") format('truetype');",
				'}',
			'</style>'].join('\n');
		nativeFont[1] = true;
		nativeFont[2] = fontFamily;
		
		/*
			Add the lowercase letter 'l' to two span elements.
			Each has the font applied, but one uses a monospace fallback (which will be wide) and 
			the other will have a sans-serif fallback (which will be skinny). The reason for this
			mechanism is two-fold:
			- some browsers will not load a font face resource (even if directly embedded as base64)
			  unless it is applied to some element on the page.
			- some browsers will not synchronously load the font so applying the font with a fallback
			  font that is known to have different widths is an easy way to query whether the font
			  is truly loaded (by checking the width of the span elements).
		*/
		var loader1 = LIB$graphics2dtext$getFontLoader(1);
		var loader2 = LIB$graphics2dtext$getFontLoader(2);
		loader1.style.fontFamily = fontFamily + ",monospace";
		loader2.style.fontFamily = fontFamily + ",sans-serif";
		loader1.innerHTML = 'l';
		loader2.innerHTML = 'l';
		
		return true;
	}
	return false;
}

LIB$graphics2dtext$getFontLoader = function(id) {
	return document.getElementById('crayon_font_loader_' + id);
}

LIB$graphics2dtext$isDynamicFontLoaded = function() {
	var loader1 = LIB$graphics2dtext$getFontLoader(1);
	var loader2 = LIB$graphics2dtext$getFontLoader(2);
	var w1 = loader1.getBoundingClientRect().width;
	var w2 = loader2.getBoundingClientRect().width;
	var loaded = Math.floor(w1 * 1000 + .5) == Math.floor(w2 * 1000 + .5);
	if (loaded) {
		// get rid of the l's in the DOM as soon as possible.
		loader1.innerHTML = '';
		loader2.innerHTML = '';
	}
	return loaded;
};

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
