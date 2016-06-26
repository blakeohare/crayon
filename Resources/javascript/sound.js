
/*
A music native object is a struct-like list of 3 items.
music[0] -> audio object
music[1] -> user-given filepath
music[2] -> URL
music[3] -> is looping

*/

R._dummyAudio = new Audio();

R.isAudioSupported = function () {
	return R.isAudioEnabled(R._dummyAudio);
};

R.isAudioEnabled = function (audioObj) {
	return !!R._dummyAudio.canPlayType('audio/ogg');
};

R.musicSetVolume = function (r) {
	if (R.musicCurrent != null)
		R.musicCurrent[0].volume = r;
};

R.musicCurrent = null;

R.musicPlay = function (music, loop) {
	if (R.musicIsPlaying()) R.musicCurrent[0].pause();

	if (R.isAudioEnabled(music[0])) {
		if (music[0].readyState == 2) {
			music[0].currentTime = 0;
		}
		music[3] = loop;
		R.musicCurrent = music;
		music[0].play();
	}
};

R.musicStop = function () {
	if (R.musicIsPlaying()) R.musicCurrent[0].pause();
};

R.musicIsPlaying = function () {
	return R.musicCurrent != null && !R.musicCurrent.paused;
};

R.musicLoad = function (filepath) {
	var audioObject = new Audio(%%%JS_FILE_PREFIX%%% + 'resources/audio/' + filepath);
	var m = [
		audioObject,
		filepath,
		filepath,
		false
	];

	audioObject.addEventListener('ended', function () {
		if (m[3]) { // isLooping.
			this.currentTime = 0;
			this.play();
		}
	}, false);

	return m;
};

/*
	At the core of the sound mixer is a giant list of sound "structs" which are individually treated
	as the "native sound object" to the rest of the translated code.
	Each sound struct is a list composed of 3 items:
	- soundStruct[0] -> a list of JS Audio objects.
	- soundStruct[1] -> the original file name that should be used as the input to new Audio objects.
	- soundStruct[2] -> the index of this object in the overall list of sound structs.

	There is also a reverse lookup of filepaths to the index of where they are in this list.
	When a sound is being loaded, the index is looked up in the reverse lookup. If none is
	found, then a new sound struct is created with exactly 1 Audio object in it with that filename.

	When a sound struct is going to be played, the first Audio object that is not currently playing has
	its .play() method invoked. If there are no available objects, a new one is created and pushed to the
	end of the list. Servers/browsers should be smart enough to start playing this instantly with a 304.

	A channel instance is the following list-struct
	- channelStruct[0] -> soundStruct index
	- channelStruct[1] -> the current audio element that's playing
	- channelStruct[2] -> currentTime value if paused, or null if playing
	- channelStruct[3] -> current state: 0 -> playing, 1 -> paused, 2 -> stopped
*/

R.soundObjectIndexByFilename = {};
R.soundObjectsByIndex = [];

R.prepSoundForLoading = function (filepath) {
	var index = R.soundObjectIndexByFilename[filepath];
	if (index === undefined) {
		index = R.soundObjectsByIndex.length;
		var data = [[new Audio(%%%JS_FILE_PREFIX%%% + 'resources/audio/' + filepath)], filepath, index];
		R.soundObjectIndexByFilename[filepath] = index;
		R.soundObjectsByIndex.push(data);
	}

	return R.soundObjectsByIndex[index];
};

R.stopSound = function (channel, isPause) {
	if (channel[3] == 0) {
		var s = R.soundObjectsByIndex[channel[0]];
		var audio = s[0][channel[1]];
		if (!audio.ended) {
			channel[2] = audio.currentTime;
			audio.pause();
			channle[3] = isPause ? 1 : 2;
		}
	}
};

R.resumeSound = function (channel) {
	if (channel[3] == 1) {
		var s = R.soundObjectsByIndex[channel[0]];
		newChannel = R.playSound(s[1], channel[2]); // just discard the new channel object and apply the info to the currently existing reference.
		channel[1] = newChannel[1];
		channel[3] = 0;
	}
};

R.playSound = function (sound, startFrom) {
	var audioList = sound[0];
	var audio = null;
	var audioIndex = 0;
	for (var i = 0; i < audioList.length; ++i) {
		if (!audioList[i].playing) {
			audio = audioList[i];
			audioIndex = i;
			break;
		}
	}
	if (audio == null) {
		audioIndex = audioList.length;
		audio = new Audio(%%%JS_FILE_PREFIX%%% + sound[1]);
		audioList.push(audio);
	}
	if (R.isAudioEnabled(audio)) {
		if (audio.readyState == 2) {
			audio.currentTime = startFrom;
		}
		audio.play();

		// See channel struct comment above.
		return [sound[2], audioIndex, null, 0];
	}
	return [sound[2], audioIndex, null, 2];
};
