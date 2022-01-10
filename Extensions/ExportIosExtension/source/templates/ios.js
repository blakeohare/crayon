﻿
C$common$envDescriptor = () => 'ios|javascript';

console.log = function(msg) {
  window.webkit.messageHandlers.interop.postMessage(msg + '');
};

C$ios$invokePointer = function(type, x, y) {
  var x = Math.floor(C$game$width * x);
  var y = Math.floor(C$game$height * y);
  var typeStr = type == 0 ? 'DOWN' : (type == 1 ? 'UP' : 'MOVE');
  if (type < 2) {
    C$input$eventRelays.push(buildRelayObj(33 + type, x, y, 0, 0, ''));
  } else {
    C$input$eventRelays.push(buildRelayObj(32, x, y, 0, 0, ''));
  }
}

C$ios$knownSize = null;
C$ios$adjustScreen = function() {
  var screen = [window.innerWidth, window.innerHeight];

  if (C$ios$knownSize === null ||
      C$ios$knownSize[0] != screen[0] ||
      C$ios$knownSize[1] != screen[1]) {
	var phsCanvas = C$game$real_canvas;
	var gameCanavs = C$game$virtual_canvas;
    C$ios$knownSize = screen;
    phsCanvas.width = screen[0];
    phsCanvas.height = screen[1];
    C$game$ctx.scale(screen[0] / C$game$width, screen[1] / C$game$height);
  }
};

C$game$everyFrame = function() {
	C$ios$adjustScreen();
};

C$game$screenInfo = function(o) {
    o[0] = 1;
    o[1] = window.innerWidth;
    o[2] = window.innerHeight;
    return true;
}

function sendMessage(name, value) {
    switch (name) {
        case "view-type":
            window.webkit.messageHandlers.viewType.postMessage(value);
            break;
        default:
            throw "Unknown message type: " + name;
    }
}

C$common$envIsMobile = () => true;

COMMON.setUrlPath = (path) => { };
COMMON.getUrlPath = () => null;
COMMON.launchBrowser = (url, fr, name) => { console.log("TODO: Open URLs (" + url + ")"); };
