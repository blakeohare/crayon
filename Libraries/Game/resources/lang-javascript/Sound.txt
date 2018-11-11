
/*
A music native object is a struct-like list of 3 items.
music[0] -> audio object
music[1] -> user-given filepath
music[2] -> URL
music[3] -> is looping

*/

C$audio = 1;
C$audio$dummyAudio = new Audio();
C$audio$musicCurrent = null;
C$audio$soundObjectIndexByFilename = {};
C$audio$soundObjectsByIndex = [];

C$audio$isAudioSupported = function () {
    return C$audio$isAudioEnabled(C$audio$dummyAudio);
};

C$audio$isAudioEnabled = function (audioObj) {
    return !!C$audio$dummyAudio.canPlayType('audio/ogg');
};

C$audio$musicSetVolume = function (r) {
    if (C$audio$musicCurrent != null)
        C$audio$musicCurrent[0].volume = r;
};

C$audio$musicPlay = function (music, loop) {
    if (C$audio$musicIsPlaying()) C$audio$musicCurrent[0].pause();

    if (C$audio$isAudioEnabled(music[0])) {
    if (music[0].readyState == 2) {
      music[0].currentTime = 0;
    }
    music[3] = loop;
    C$audio$musicCurrent = music;
    music[0].play();
  }
};

C$audio$musicStop = function () {
    if (C$audio$musicIsPlaying()) C$audio$musicCurrent[0].pause();
};

C$audio$musicIsPlaying = function () {
    return C$audio$musicCurrent != null && !C$audio$musicCurrent.paused;
};

C$audio$musicLoad = function (filepath) {
    var audioObject = new Audio(C$common$jsFilePrefix + 'resources/audio/' + filepath);
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

C$audio$prepSoundForLoading = function (filepath) {
    var index = C$audio$soundObjectIndexByFilename[filepath];
  if (index === undefined) {
      index = C$audio$soundObjectsByIndex.length;
      var data = [[new Audio(C$common$jsFilePrefix + 'resources/audio/' + filepath)], filepath, index];
    C$audio$soundObjectIndexByFilename[filepath] = index;
    C$audio$soundObjectsByIndex.push(data);
  }

  return C$audio$soundObjectsByIndex[index];
};

C$audio$stopSound = function (channel, isPause) {
  if (channel[3] == 0) {
      var s = C$audio$soundObjectsByIndex[channel[0]];
    var audio = s[0][channel[1]];
    if (!audio.ended) {
      channel[2] = audio.currentTime;
      audio.pause();
      channle[3] = isPause ? 1 : 2;
    }
  }
};

C$audio$resumeSound = function (channel) {
  if (channel[3] == 1) {
      var s = C$audio$soundObjectsByIndex[channel[0]];
      newChannel = C$audio$playSound(s[1], channel[2]); // just discard the new channel object and apply the info to the currently existing reference.
    channel[1] = newChannel[1];
    channel[3] = 0;
  }
};

C$audio$playSound = function (sound, startFrom) {
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
    audio = new Audio(C$common$jsFilePrefix + sound[1]);
    audioList.push(audio);
  }
  if (C$audio$isAudioEnabled(audio)) {
    if (audio.readyState == 2) {
      audio.currentTime = startFrom;
    }
    audio.play();

    // See channel struct comment above.
    return [sound[2], audioIndex, null, 0];
  }
  return [sound[2], audioIndex, null, 2];
};
