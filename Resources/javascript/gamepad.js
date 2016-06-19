R.gamepad = {};

// devices connected but not yet flushed to the universe.
R.gamepad.queue = [];

// devices made known to the user.
R.gamepad.devices = [];

R.gamepad._support = !!navigator.getGamepads;

R.gamepad.isSupported = function () {
	return R.gamepad._support;
};

R.gamepad.refresh = function () {
	for (i = 0; i < R.gamepad.queue.length; ++i) {
		R.gamepad.devices.push(R.gamepad.queue[i]);
	}
	R.gamepad.queue = [];
};

R.gamepad.getDeviceCount = function () {
	return R.gamepad.devices.length;
};

R.gamepad.getDevice = function (i) {
	return R.gamepad.devices[i];
};

R.gamepad.getName = function (device) {
	return device.id;
};

R.gamepad.getButtonCount = function (device) {
	return device.buttons.length;
};

R.gamepad.getAxisCount = function (device) {
	return device.axes.length;
};

R.gamepad.getButtonState = function (device, index) {
	return device.buttons[index].pressed;
};

R.gamepad.getAxisState = function (device, index) {
	return device.axes[index];
};

window.addEventListener("gamepadconnected", function (e) {
	R.gamepad.queue.push(e.gamepad);
});

window.addEventListener("gamepaddisconnected", function (e) {
	// ignore for now.
});
