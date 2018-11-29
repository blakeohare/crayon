package org.crayonlang.libraries.game;

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

  public static boolean checkPathExistence(String path) {
    java.nio.file.Path p;
    try {
      p = java.nio.file.Paths.get(path);
    } catch (java.nio.file.InvalidPathException ipe) {
      return false;
    }
    return java.nio.file.Files.exists(p);
  }

}
