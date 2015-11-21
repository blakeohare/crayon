
R.playSound = function (platformSound) {
	// TODO: playSound
};

R.musicSetVolume = function () { };

R.musicShouldLoopByFilename = {};
R.musicPlayNow = function (audioPath, audioObject, loop) {
	if (R.currentlyPlayingMusic != null) {
		R.currentlyPlayingMusic.pause();
	}
	audioObject.currentTime = 0;
	R.musicShouldLoopByFilename[audioPath] = loop;
	R.currentlyPlayingMusic = audioObject;
	audioObject.play();
};

R.musicPause = function () {
	if (R.currentlyPlayingMusic != null) {
		R.currentlyPlayingMusic.pause();
	}
};

R.musicResume = function () {
	if (R.currentlyPlayingMusic != null) {
		R.currentlyPlayingMusic.play();
	}
};

R.currentlyPlayingMusic = null;

R.musicLoadFromResource = function (filepath, statusOut) {
	statusOut[0] = 0;
	var audioObject = new Audio(filepath);
	R.musicShouldLoopByFilename[filepath] = false;
	audioObject.addEventListener('ended', function () {
		if (R.musicShouldLoopByFilename[filepath]) {
			this.currentTime = 0;
			this.play();
		}
	}, false);
	return v_instantiateMusicInstance(filepath, audioObject, filepath, true);
};

R.soundObjectIndexByFilename = {};
R.soundObjectsByIndex = [];

R.prepSoundForLoading = function (filepath) {
	var index = R.soundObjectIndexByFilename[filepath];
	if (index === undefined) {
		index = R.soundObjectsByIndex.length;
		var data = [[new Audio(filepath)], filepath, index];
		R.soundObjectIndexByFilename = index;
		R.soundObjectsByIndex.push(data);
	}

	return R.soundObjectsByIndex[index];
};

R.playSound = function (soundStruct) {
	var audioList = soundStruct[0];
	var audio = null;
	for (var i = 0; i < audioList.length; ++i) {
		if (audioList[i].ended) {
			audio = audioList[i];
			break;
		}
	}
	if (audio == null) {
		audio = new Audio(soundStruct[1]);
		audioList.push(audio);
	}
	audio.currentTime = 0;
	audio.play();
};
