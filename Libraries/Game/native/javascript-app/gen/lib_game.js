PST$createNewArray = function(s) {
	var o = [];
	while (s-- > 0) o.push(null);
	return o;
};

PST$multiplyList = function(l, n) {
	var o = [];
	var s = l.length;
	var i;
	while (n-- > 0) {
		for (i = 0; i < s; ++i) {
			o.push(l[i]);
		}
	}
	return o;
};

PST$intBuffer16 = PST$multiplyList([0], 16);

PST$dictionaryKeys = function(d) {
	var o = [];
	for (var k in d) {
		o.push(k);
	}
	return o;
};

PST$extCallbacks = {};

PST$registerExtensibleCallback = (name, fn) => { PST$extCallbacks[name] = fn; };

var lib_audio_load_sfx_from_resourceImpl = function(obj, path) {
	var sfx = C$audio$prepSoundForLoading(path);
	obj[3] = PST$createNewArray(1);
	obj[3][0] = sfx;
	return 1;
};

var lib_audio_music_load_from_resourceImpl = function(musicObj, path) {
	var nativeMusicObject = C$audio$musicLoad(path);
	if ((nativeMusicObject != null)) {
		musicObj[3] = PST$createNewArray(1);
		musicObj[3][0] = nativeMusicObject;
		return true;
	}
	return false;
};

var lib_audio_music_playImpl = function(musicObject, isResource, path, startingVolume, isLoop) {
	C$audio$musicSetVolume(startingVolume);
	var nativeObject = null;
	if ((musicObject[3] != null)) {
		nativeObject = musicObject[3][0];
	}
	if (isResource) {
		C$audio$musicPlay(nativeObject, isLoop);
	} else {
		if (!false) {
			return -1;
		}
		C$common$alwaysTrue();
	}
	return 0;
};

var lib_audio_sfx_get_stateImpl = function(channel, sfxResource, resourceId) {
	return (channel[2] + 1);
};

var lib_audio_sfx_launch = function(sfxResource, channelNativeDataOut, volume, pan) {
	var channel = C$audio$playSound(sfxResource, 0);
	if ((channel == null)) {
		return 0;
	}
	channelNativeDataOut[0] = channel;
	return 1;
};

var lib_audio_sfx_set_panImpl = function(channel, sfxResource, pan) {
	return 0;
};

var lib_audio_sfx_set_volumeImpl = function(channel, sfxResource, volume) {
	return 0;
};

var lib_audio_sfx_stopImpl = function(channel, resource, resourceId, isActivelyPlaying, hardStop) {
	C$audio$stopSound(channel, true);
	return 0;
};

var lib_audio_sfx_unpause = function(channel, sfxResource, volume, pan) {
	C$audio$resumeSound(channel);
	return 0;
};

var lib_audio_stop = function(sound, reset) {
	C$audio$stopSound(sound);
	return 0;
};

var lib_game_audio_getAudioResourcePath = function(vm, args) {
	return resource_manager_getResourceOfType(vm, args[0][1], "SND");
};

var lib_game_audio_is_supported = function(vm, args) {
	if (C$audio$isAudioSupported()) {
		return vm[13][1];
	}
	return vm[13][2];
};

var lib_game_audio_music_is_playing = function(vm, args) {
	if (C$audio$musicIsPlaying()) {
		return vm[13][1];
	}
	return vm[13][2];
};

var lib_game_audio_music_load_from_file = function(vm, args) {
	return vm[13][0];
};

var lib_game_audio_music_load_from_resource = function(vm, args) {
	var objInstance1 = args[0][1];
	if (lib_audio_music_load_from_resourceImpl(objInstance1, args[1][1])) {
		return vm[13][1];
	}
	return vm[13][2];
};

var lib_game_audio_music_play = function(vm, args) {
	return buildBoolean(vm[13], (lib_audio_music_playImpl(args[0][1], args[1][1], args[2][1], args[3][1], args[4][1]) != -1));
};

var lib_game_audio_music_set_volume = function(vm, args) {
	C$audio$musicSetVolume(args[0][1]);
	return vm[13][0];
};

var lib_game_audio_music_stop = function(vm, args) {
	return vm[13][0];
};

var lib_game_audio_sfx_get_state = function(vm, args) {
	var channelInstance = args[0][1];
	var nativeChannel = channelInstance[3][0];
	var soundInstance = args[1][1];
	var nativeSound = soundInstance[3][0];
	var resourceId = args[2][1];
	return buildInteger(vm[13], lib_audio_sfx_get_stateImpl(nativeChannel, nativeSound, resourceId));
};

var lib_game_audio_sfx_load_from_file = function(vm, args) {
	return vm[13][0];
};

var lib_game_audio_sfx_load_from_resource = function(vm, args) {
	var soundInstance = args[0][1];
	lib_audio_load_sfx_from_resourceImpl(soundInstance, args[1][1]);
	return vm[13][0];
};

var lib_game_audio_sfx_play = function(vm, args) {
	var channelInstance = args[0][1];
	var resourceInstance = args[1][1];
	channelInstance[3] = PST$createNewArray(1);
	var nativeResource = resourceInstance[3][0];
	var vol = args[2][1];
	var pan = args[3][1];
	return buildInteger(vm[13], lib_audio_sfx_launch(nativeResource, channelInstance[3], vol, pan));
};

var lib_game_audio_sfx_resume = function(vm, args) {
	var sndInstance = args[0][1];
	var nativeSound = sndInstance[3][0];
	var sndResInstance = args[1][1];
	var nativeResource = sndResInstance[3][0];
	var vol = args[2][1];
	var pan = args[3][1];
	lib_audio_sfx_unpause(nativeSound, nativeResource, vol, pan);
	return vm[13][0];
};

var lib_game_audio_sfx_set_pan = function(vm, args) {
	var channel = args[0][1];
	var nativeChannel = channel[3][0];
	var resource = args[1][1];
	var nativeResource = resource[3][0];
	lib_audio_sfx_set_panImpl(nativeChannel, nativeResource, args[2][1]);
	return vm[13][0];
};

var lib_game_audio_sfx_set_volume = function(vm, args) {
	var channel = args[0][1];
	var nativeChannel = channel[3][0];
	var resource = args[1][1];
	var nativeResource = resource[3][0];
	lib_audio_sfx_set_volumeImpl(nativeChannel, nativeResource, args[2][1]);
	return vm[13][0];
};

var lib_game_audio_sfx_stop = function(vm, args) {
	var channel = args[0][1];
	var nativeChannel = channel[3][0];
	var resource = args[1][1];
	var nativeResource = resource[3][0];
	var resourceId = args[2][1];
	var currentState = args[3][1];
	var completeStopAndFreeChannel = args[4][1];
	var isAlreadyPaused = ((currentState == 2) && !completeStopAndFreeChannel);
	if (((currentState != 3) && !isAlreadyPaused)) {
		lib_audio_sfx_stopImpl(nativeChannel, nativeResource, resourceId, (currentState == 1), completeStopAndFreeChannel);
	}
	return vm[13][0];
};

var lib_game_clock_tick = function(vm, args) {
	C$game$endFrame();
	vm_suspend_context_by_id(vm, args[0][1], 1);
	return vm[14];
};

var lib_game_gamepad_current_device_count = function(vm, args) {
	var total = 0;
	if ((true && C$gamepad$isSupported())) {
		total = C$gamepad$getDeviceCount();
	}
	return buildInteger(vm[13], total);
};

var lib_game_gamepad_get_axis_1d_state = function(vm, args) {
	var index = args[1][1];
	var dev = args[0][1];
	return buildFloat(vm[13], C$gamepad$getAxisState(dev[3][0], index));
};

var lib_game_gamepad_get_axis_2d_state = function(vm, args) {
	var arg1 = args[0];
	var arg2 = args[1];
	var arg3 = args[2];
	var objInstance1 = arg1[1];
	var int1 = arg2[1];
	var list1 = arg3[1];
	0;
	clearList(list1);
	addToList(list1, buildInteger(vm[13], PST$intBuffer16[0]));
	addToList(list1, buildInteger(vm[13], PST$intBuffer16[1]));
	return vm[14];
};

var lib_game_gamepad_get_button_state = function(vm, args) {
	var int1 = 0;
	var objInstance1 = null;
	var arg1 = args[0];
	var arg2 = args[1];
	objInstance1 = arg1[1];
	int1 = arg2[1];
	if (C$gamepad$getButtonState(objInstance1[3][0], int1)) {
		return vm[15];
	}
	return vm[16];
};

var lib_game_gamepad_get_save_file_path = function(vm, args) {
	var string1 = ".crayon-js.gamepad.config";
	if ((string1 != null)) {
		return buildString(vm[13], string1);
	}
	return vm[14];
};

var lib_game_gamepad_getPlatform = function(vm, args) {
	return buildInteger(vm[13], 3);
};

var lib_game_gamepad_initialize_device = function(vm, args) {
	var int1 = args[0][1];
	var objInstance1 = args[1][1];
	var list1 = args[2][1];
	var object1 = C$gamepad$getDevice(int1);
	objInstance1[3] = PST$createNewArray(1);
	objInstance1[3][0] = object1;
	clearList(list1);
	addToList(list1, buildString(vm[13], C$gamepad$getName(object1)));
	addToList(list1, buildInteger(vm[13], C$gamepad$getButtonCount(object1)));
	addToList(list1, buildInteger(vm[13], C$gamepad$getAxisCount(object1)));
	addToList(list1, buildInteger(vm[13], 0));
	return vm[14];
};

var lib_game_gamepad_is_supported = function(vm, args) {
	if ((true && C$gamepad$isSupported())) {
		return vm[15];
	} else {
		return vm[16];
	}
};

var lib_game_gamepad_jsIsOsx = function(vm, args) {
	return buildInteger(vm[13], (window.navigator.platform == 'MacIntel' ? 1 : 0));
};

var lib_game_gamepad_platform_requires_refresh = function(vm, args) {
	if ((true && C$gamepad$isSupported())) {
		return vm[15];
	}
	return vm[16];
};

var lib_game_gamepad_poll_universe = function(vm, args) {
	C$common$alwaysTrue();
	return vm[14];
};

var lib_game_gamepad_refresh_devices = function(vm, args) {
	C$gamepad$refresh();
	return vm[14];
};

var lib_game_getScreenInfo = function(vm, args) {
	var outList = args[0];
	var o = PST$intBuffer16;
	if (C$game$screenInfo(o)) {
		var output = outList[1];
		clearList(output);
		addToList(output, buildBoolean(vm[13], (o[0] == 1)));
		addToList(output, buildInteger(vm[13], o[1]));
		addToList(output, buildInteger(vm[13], o[2]));
	}
	return outList;
};

var lib_game_getTouchState = function(vm, args) {
	var output = args[0][1];
	var data = PST$createNewArray(31);
	data[0] = 0;
	C$game$getTouchState(data);
	var _len = data[0];
	var end = ((_len * 3) + 1);
	var i = 1;
	while ((i < end)) {
		addToList(output, buildInteger(vm[13], data[i]));
		i += 1;
	}
	return vm[14];
};

var lib_game_initialize = function(vm, args) {
	C$game$initializeGame(getFloat(args[0]));
	return vm[14];
};

var lib_game_initialize_screen = function(vm, args) {
	var ecId = args[4][1];
	var ec = getExecutionContext(vm, ecId);
	C$game$initializeScreen(args[0][1], args[1][1], args[2][1], args[3][1], ecId);
	vm_suspend_for_context(ec, 1);
	return vm[14];
};

var lib_game_pump_events = function(vm, args) {
	var output = args[0][1];
	var eventList = C$game$pumpEventObjects();
	var globals = vm[13];
	clearList(output);
	var _len = eventList.length;
	if ((_len > 0)) {
		var i = 0;
		i = 0;
		while ((i < _len)) {
			var ev = eventList[i];
			addToList(output, buildInteger(globals, ev[0]));
			var t = ev[0];
			addToList(output, buildInteger(globals, ev[1]));
			if ((t >= 32)) {
				addToList(output, buildInteger(globals, ev[2]));
				if ((t == 37)) {
					addToList(output, buildFloat(globals, ev[4]));
				} else if (((t >= 64) && (t < 80))) {
					addToList(output, buildInteger(globals, ev[3]));
				}
			}
			i += 1;
		}
	}
	return args[0];
};

var lib_game_set_title = function(vm, args) {
	C$game$setTitle(args[0][1]);
	return vm[14];
};

var lib_game_set_window_mode = function(vm, args) {
	var mode = args[0][1];
	var width = args[1][1];
	var height = args[2][1];
	C$common$alwaysFalse();
	return vm[14];
};

var lib_game_setInstance = function(vm, args) {
	var o = args[0][1];
	var nd = PST$createNewArray(1);
	nd[0] = null;
	o[3] = nd;
	return vm[14];
};

var lib_game_startup = function(vm, args) {
	var functionLookup = C$game$getCallbackFunctions();
	var names = PST$dictionaryKeys(functionLookup);
	var i = 0;
	while ((i < names.length)) {
		var fn = null;
		var name = names[i];
		fn = functionLookup[name];
		if (fn === undefined) fn = null;
		if ((fn != null)) {
			registerNamedCallback(vm, "Game", name, fn);
		}
		i += 1;
	}
	return vm[14];
};

var lib_game_syncPointers = function(vm, args) {
	var pts = [];
	C$game$syncPointers(pts);
	if ((pts.length == 0)) {
		return vm[14];
	}
	var values = [];
	var i = 0;
	while ((i < pts.length)) {
		values.push(buildFloat(vm[13], pts[i]));
		i += 1;
	}
	return buildList(values);
};
