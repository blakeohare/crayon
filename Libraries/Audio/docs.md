# Audio Library

The Audio library allows you to load and play audio inside a Nori-based UI window (Nori windows can be created with either the Nori library or Game library).

## Basic Usage

To load a sound file, you first need an `Audio.SoundLoader` instance. The constructor for this takes in a reference to the Nori context, either a Nori.Frame instance or a Game.GameWindow instance:

`soundLoader = new Audio.SoundLoader(myWindow);`

A `SoundLoader` can be used to load audio from one of three sources:

* **Bytes** from a sound file (e.g. from a file). You would generally get these by using the `FileIO.fileReadBytes()` function.
* **Project resource files** that are included in your source code.
* Raw audio **sample information** to construct custom sounds and effects.

Each of these sources are represented by both a synchronous and asynchronous method:

### Load sound from bytes asynchronously
```js
bytes = FileIO.readFileBytes("myFile.ogg");
soundLoader.loadFromBytes(bytes, sound => {
  print("Sound loaded and ready to use!");
}, err => {
  print("Could not load sound: " + err);
});
```
The error handler callback is optional.

> TODO: what is the error callback's parameter?

> TODO: if no error callback is provided, what happens on error?


### Load sound from bytes synchronously
```js
bytes = FileIO.readFileBytes("myFile.ogg");
sound = soundLoader.awaitLoadFromBytes(bytes);
print("Sound loaded and ready to use!");
// throws Exception on error
```

> TODO: create a more specific exception

### Load sound from project resource asynchronously

```js
soundLoader.loadFromResource("myFile.ogg", sound => {
  print("Sound loaded and ready to use!");
}, err => {
  print("Could not load sound: " + err);
});
```
> TODO: same TODO's from above

The error handler callback is optional.

### Load sound from project resource synchronously

```js
sound = soundLoader.awaitLoadFromResource("myFile.ogg");
print("Sound loaded and ready to use!");
// throws Exception on error
```

> TODO: same TODO's from above

### Load sound from raw samples asynchronously

Samples are a `Audio.SoundBuffer` instance. You can read more about how to create these below. 
```js
soundLoader.loadFromSamples(buffer, sound => {
  print("Sound loaded and ready to use!");
}, err => {
  print("Could not load sound: " + err);
});
```

> TODO: same TODO's from above

The error handler callback is optional.

### Load sound from raw samples synchronously

```js
sound = soundLoader.awaitLoadFromSamples(buffer);
print("Sound loaded and ready to use!");
// throws Exception on error
```

> TODO: same TODO's as above

## The Sound class

An `Audio.Sound` instance represents a single sound and can be created using a `Audio.SoundLoader` instance (see above). A sound instance allows you to play or stop a sound.

### Playing a sound

```js
// This will play the sound once
sound.play(); 

// This will play a sound multiple times
sound.playInLoop(); 

// This will fade a sound in over the course of the duration. The duration is a number of seconds.
sound.fadeIn(duration); 
```

All methods are **non-blocking**.

### Stopping a sound

This will stop the sound immediately and not save its current position. If you pay it again, it will start from the beginning.
```js
sound.stop();
```

Optionally, stop can take in a fade duration parameter. The sound will fade out over the course of the given duration and then stop, without saving its current position. 
`fadeDuration` is a number in seconds.
```js
sound.stop(fadeDuration);
```

This will stop the sound immediately but the current position will be saved. If you play it again, it will start from where it left off.
```js
sound.pause();
```

Optionally, pause can also take in a fade duration parameter. The sound will fade out over the course of the given duration and then pause. If you play the sound again, it will start from the position that it reached 0 volume. 
`fadeDuration` is a number in seconds
```js
sound.pause(fadeDuration);
```

> TODO: what happens when you try to stop a sound that's already stopped?

## The SoundBuffer class

An `Audio.SoundBuffer` instance can be used to create an arbitrary custom sound using raw sample information.

To create a SoundBuffer, the constructor can be called directly:

```js
channels = 2;
sampleRate = 44100;
buffer = new Audio.SoundBuffer(channels, sampleRate);
```
Use 1 as the value for `channels` if you want a mono sound. Use 2 for stereo.

The sample rate is the number of samples to be played per second, i.e. the number of hertz. This number must be from the standard set of options.

Valid sample rate values are the following: `8000`, `11025`, `44100`, `48000`, `96000`, `192000`

8000 is considered high compression, and 192000 is considered "studio quality". 44.1 kHz is considered standard.

Both of these arguments are optional, and if unspecified, the default values are a 1-channel 44.1 kHz sound buffer.

### Setting Duration

The channel buffers are initialized to be empty. To ensure the sample buffers have a given length, you can set the SoundBuffer's duration with the `setDuration` method.

```js
buffer.setDuration(4.5); // 4.5 seconds
```
This will add or remove samples to all channels such that the sound will be the given duration at the buffer's current sample rate.

The given number is the number of seconds. This is multiplied by the sample rate to get an integer number of samples and rounded, if necessary. 

Samples that are added to the channel have a default value of 0. If the new duration is shorter than the previous duration, samples are removed from the end of the buffer.

### Channel buffers

Channel buffers are represented as a list of integers. To access each channel, you can call the function `getChannelBuffer(n)` which will return a mutable integer list. You can add or modify numbers in this list to edit the sample data. The value `n` is the buffer index which is 0-indexed. For a mono sound with one channel, the only valid value for `n` is 0. For stereo sounds, `0` and `1` are valid.

The integer values in the buffer should range between -32768 and 32767 (e.g. a 16-bit sound).

### Channel buffer end-to-end example
In this example, an A-440 (an A above middle C) that lasts for 2 seconds is created using the sound buffer.

```js
// mono, 44.1 kHz
buffer = new Audio.SoundBuffer(1, 44100);

// lasts for 2 seconds, e.g. there are 88.2k samples
buffer.setDuration(2); 

channel = buffer.getChannelBuffer(0);

for (i = 0; i < channel.length; i++) {
  time = i / 44100.0; // current time offset

  // create a sine wave with amplitude of 32k and frequency 440
  value = Math.sin(time * (2 * Math.PI * 440)) * 32000;
  channel[i] = value;
}

// Create a loader from the current UI context
loader = new SoundLoader(frameOrWindow);

// convert the buffer into a sound instance
sound = loader.awaitLoadFromSamples(buffer);

// play the sound
sound.play();
```
