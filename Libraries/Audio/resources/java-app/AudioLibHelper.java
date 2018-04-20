package org.crayonlang.libraries.audio;

class AudioLibHelper {

	// TODO: Ogg decoding in Java
	private static class MusicDummyObject {
		private String path;
		public MusicDummyObject(String path) {
			this.path = path;
		}
	}

    public static Object loadMusicFromResource(String path) {
        return new MusicDummyObject(path);
    }

}
