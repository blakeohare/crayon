
C$images = 1;
C$images$image_loader = null;
C$images$image_store = null;
C$images$temp_image = null;
C$images$image_downloads = {};
C$images$image_download_counter = 0;
C$images$image_keys_by_index = [null];
C$images$better_completed_image_lookup = {};
C$images$autogenDownloaderKey = 1;

C$images$is_image_loaded = function (key) {
    return C$images$image_downloads[key] !== undefined;
};

C$images$enqueue_image_download = function (key, url) {
    var id = ++C$images$image_download_counter;
    C$images$image_keys_by_index.push(key);
    var loader_queue = C$common$getElement('crayon_image_loader_queue');
    loader_queue.innerHTML += '<img id="image_loader_img_' + id + '" onload="C$images$finish_load_image(' + id + ')" crossOrigin="anonymous" />' +
		'<canvas id="image_loader_canvas_' + id + '" />';
    var img = C$common$getElement('image_loader_img_' + id);
    img.src = C$common$jsFilePrefix + url;
    return true;
};

C$images$better_enqueue_image_download = function (url) {
    var key = 'k' + C$images$autogenDownloaderKey++;
    var loader_queue = C$common$getElement('crayon_image_loader_queue');
    loader_queue.innerHTML += '<img id="better_downloader_' + key + '" onload="C$images$better_finish_load_image(&quot;' + key + '&quot;)" crossOrigin="anonymous" />' +
		'<canvas id="better_image_loader_canvas_' + key + '" />';
    var img = C$common$getElement('better_downloader_' + key);
    img.src = C$common$jsFilePrefix + url;
    return key;
};

// TODO: blit to a canvas that isn't in the DOM and then delete the img and canvas when completed.
// TODO: figure out if there are any in flight downloads, and if not, clear out the load queue DOM.
C$images$better_finish_load_image = function (key) {
    var img = C$common$getElement('better_downloader_' + key);
    var canvas = C$common$getElement('better_image_loader_canvas_' + key);
    canvas.width = img.width;
    canvas.height = img.height;
    var ctx = canvas.getContext('2d');
    ctx.drawImage(img, 0, 0);
    C$images$better_completed_image_lookup[key] = canvas;
};

C$images$get_completed_image_if_downloaded = function (key) {
    var canvas = C$images$better_completed_image_lookup[key];
    if (!!canvas) return canvas;
    return null;
};

C$images$finish_load_image = function (id) {
    var key = C$images$image_keys_by_index[id];
    var img = C$common$getElement('image_loader_img_' + id);
    var canvas = C$common$getElement('image_loader_canvas_' + id);
    canvas.width = img.width;
    canvas.height = img.height;
    canvas.getContext('2d').drawImage(img, 0, 0);
    C$images$image_downloads[key] = canvas;
};

C$images$flushImagette = function (imagette) {
    var width = imagette[0];
    var height = imagette[1];
    var images = imagette[2];
    var xs = imagette[3];
    var ys = imagette[4];
    var canvasAndContext = C$images$createCanvasAndContext(width, height);
    var canvas = canvasAndContext[0];
    var ctx = canvasAndContext[1];
    for (var i = 0; i < images.length; ++i) {
        ctx.drawImage(images[i], xs[i], ys[i]);
    }
    return canvas;
};

C$images$createCanvasAndContext = function (width, height) {
    C$images$temp_image.innerHTML = '<canvas id="temp_image_canvas"></canvas>';
    var canvas = C$common$getElement('temp_image_canvas');
    canvas.width = width;
    canvas.height = height;
    var ctx = canvas.getContext('2d');
    C$images$temp_image.innerHTML = '';
    return [canvas, ctx];
};
