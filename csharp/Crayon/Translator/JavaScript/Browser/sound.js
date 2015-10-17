
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
