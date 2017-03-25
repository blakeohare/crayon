
C$imageresources = 1;

C$imageresources$imageLoad = function (filename, nativeData, imageLoaderNativeData) {

    var image = new Image();
    var statusOut = imageLoaderNativeData;

    image.onerror = function () {
        imageLoaderNativeData[2] = 2;
    };

    image.onload = function () {
        var w = image.width;
        var h = image.height;
        if (w < 1 || h < 1) { // another possible error case
            imageLoaderNativeData[2] = 2;
        } else {
            var canvas = C$imageresources$generateNativeBitmapOfSize(w, h);
            var ctx = canvas.getContext('2d');
            ctx.drawImage(image, 0, 0);
            nativeData[0] = canvas;
            nativeData[1] = w;
            nativeData[2] = h;
            imageLoaderNativeData[2] = 1;
        }
    };

    image.src = C$common$jsFilePrefix + 'resources/images/' + filename;
};

C$imageresources$getImageResourceManifest = function () {
    var v = C$common$getTextRes('image_sheets.txt');
    return !v ? '' : v;
};

C$imageresources$generateNativeBitmapOfSize = function (width, height) {
    var canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    return canvas;
};

C$imageresources$imageResourceBlitImage = function (target, source, targetX, targetY, sourceX, sourceY, width, height) {
    target.getContext('2d').drawImage(source, sourceX, sourceY, width, height, targetX, targetY, width, height);
};

C$imageresources$checkLoaderIsDone = function (imageLoaderNativeData, nativeImageDataNativeData, output) {
    output[0] = v_buildInteger(imageLoaderNativeData[2]);
};
