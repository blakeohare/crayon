R.gamepad = {};

R.gamepad.isSupported = function () {
	return true;
};

R.gamepad.getDeviceCount = function () {
	return 0;
};

R.gamepad.getDevice = function (i) {
	return null;
};

R.gamepad.getName = function (device) {
	return "No Name";
};

R.gamepad.getButtonCount = function (device) {
	return 0;
};

R.gamepad.getAxisCount = function (device) {
	return 0;
};

R.gamepad.getButtonState = function (device, index) {
	return false;
};

R.gamepad.getAxisState = function (device, index) {
	return 0;
};
