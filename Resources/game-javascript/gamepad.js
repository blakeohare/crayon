
C$gamepad = 1;

// devices connected but not yet flushed to the universe.
C$gamepad$queue = [];

// devices made known to the user.
C$gamepad$devices = [];

C$gamepad$support = !!navigator.getGamepads;

C$gamepad$isSupported = function () {
    return C$gamepad$support;
};

C$gamepad$refresh = function () {
    for (i = 0; i < C$gamepad$queue.length; ++i) {
        C$gamepad$devices.push(C$gamepad$queue[i]);
	}
    C$gamepad$queue = [];
};

C$gamepad$getDeviceCount = function () {
    return C$gamepad$devices.length;
};

C$gamepad$getDevice = function (i) {
    return C$gamepad$devices[i];
};

C$gamepad$getName = function (device) {
	return device.id;
};

C$gamepad$getButtonCount = function (device) {
	return device.buttons.length;
};

C$gamepad$getAxisCount = function (device) {
	return device.axes.length;
};

C$gamepad$getButtonState = function (device, index) {
	return device.buttons[index].pressed;
};

C$gamepad$getAxisState = function (device, index) {
	return device.axes[index];
};

window.addEventListener("gamepadconnected", function (e) {
    C$gamepad$queue.push(e.gamepad);
});

window.addEventListener("gamepaddisconnected", function (e) {
	// ignore for now.
});
