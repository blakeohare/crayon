
C$game = 1;

C$game$width = 0;
C$game$height = 0;
C$game$pwidth = 0;
C$game$pheight = 0;
C$game$fps = 60;
C$game$real_canvas = null;
C$game$virtual_canvas = null;
C$game$scaled_mode = false;
C$game$ctx = null;
C$game$last_frame_began = C$common$now();
C$game$execId = -1;

C$game$beginFrame = function () {
    C$game$last_frame_began = C$common$now();
    if (C$game$ctx) {
        C$drawing$drawRect(0, 0, C$game$width, C$game$height, 0, 0, 0, 255);
    }
};

C$game$endFrame = function() {
    if (C$game$scaled_mode) {
        C$game$real_canvas.getContext('2d').drawImage(C$game$virtual_canvas, 0, 0);
    }

    window.setTimeout(C$game$runFrame, C$game$computeDelayMillis());
};

C$game$runFrame = function () {
    C$game$beginFrame();
    v_runInterpreter(C$game$execId); // clockTick will induce endFrame()
};

C$game$computeDelayMillis = function () {
    var ideal = 1.0 / C$game$fps;
    var diff = C$common$now() - C$game$last_frame_began;
    var delay = Math.floor((ideal - diff) * 1000);
    if (delay < 1) delay = 1;
    return delay;
};

C$game$initializeGame = function (fps) {
    C$game$fps = fps;
};

C$game$pumpEventObjects = function () {
  var newEvents = [];
  var output = C$input$eventRelays;
  C$input$eventRelays = newEvents;
  return output;
};

// TODO: also apply keydown and mousedown handlers
// TODO: (here and python as well) throw an error if you attempt to call this twice.
C$game$initializeScreen = function (width, height, pwidth, pheight, execId) {
  var scaledMode;
  var canvasWidth;
  var canvasHeight;
  var virtualCanvas = null;
  if (pwidth === null || pheight === null) {
    scaledMode = false;
    canvasWidth = width;
    canvasHeight = height;
  } else {
    scaledMode = true;
    canvasWidth = pwidth;
    canvasHeight = pheight;
    virtualCanvas = document.createElement('canvas');
    virtualCanvas.width = width;
    virtualCanvas.height = height;
  }
  var canvasHost = C$common$getElement('crayon_host');
  canvasHost.innerHTML =
    '<canvas id="crayon_screen" width="' + canvasWidth + '" height="' + canvasHeight + '"></canvas>' +
    '<div style="display:none;">' +
      '<img id="crayon_image_loader" onload="Q._finish_loading()" crossOrigin="anonymous" />' +
      '<div id="crayon_image_loader_queue"></div>' +
      '<div id="crayon_image_store"></div>' +
      '<div id="crayon_temp_image"></div>' +
    '</div>';
  var canvas = C$common$getElement('crayon_screen');

  C$game$scaled_mode = scaledMode;
  C$game$real_canvas = canvas;
  C$game$virtual_canvas = scaledMode ? virtualCanvas : canvas;
  C$game$ctx = canvas.getContext('2d');
  C$game$width = width;
  C$game$height = height;
  C$game$execId = execId;

  C$images$image_loader = C$common$getElement('crayon_image_loader');
  C$images$image_store = C$common$getElement('crayon_image_store');
  C$images$temp_image = C$common$getElement('crayon_temp_image');

  document.onkeydown = C$input$keydown;
  document.onkeyup = C$input$keyup;

  canvas.addEventListener('mousedown', C$input$mousedown);
  canvas.addEventListener('mouseup', C$input$mouseup);
  canvas.addEventListener('mousemove', C$input$mousemove);

  C$game$ctx.imageSmoothingEnabled = false;
  C$game$ctx.mozImageSmoothingEnabled = false;
  C$game$ctx.msImageSmoothingEnabled = false;
  C$game$ctx.webkitImageSmoothingEnabled = false;

  if (scaledMode) {
      C$game$ctx.scale(pwidth / width, pheight / height);
  }

  C$game$runFrame();
};

C$game$setTitle = function (title) {
  window.document.title = title;
};

window.addEventListener('keydown', function(e) {
  if ([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
    e.preventDefault();
  }
}, false);
