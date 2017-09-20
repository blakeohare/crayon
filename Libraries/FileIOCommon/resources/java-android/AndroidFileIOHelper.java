package org.crayonlang.libraries.fileiocommon;

import org.crayonlang.interpreter.structs.Value;

import java.util.ArrayList;

public class AndroidFileIOHelper {
    public static String getDirRoot(String path) {
        throw new RuntimeException();
    }

    public static boolean directoryExists(String path) {
        throw new RuntimeException();
    }

    public static int getDirParent(String path, String[] outBuffer) {
        throw new RuntimeException();
    }

    public static int createDirectory(String path) {
        throw new RuntimeException();
    }

    public static int deleteDirectory(String path) {
        throw new RuntimeException();
    }

    public static int getDirectoryList(String path, boolean useFullPath, ArrayList<String> output) {
        throw new RuntimeException();
    }

    public static int moveDirectory(String fromPath, String toPath) {
        throw new RuntimeException();
    }

    public static int fileDelete(String path) {
        throw new RuntimeException();
    }

    public static void getFileInfo(String path, int flags, int[] outputBufferInt, double[] outputBufferFloat) {
        throw new RuntimeException();
    }

    public static int fileMove(String fromPath, String toPath, boolean b1, boolean b2) {
        throw new RuntimeException();
    }

    public static int fileRead(String path, boolean b1, String[] outputBufferStr, Value[] intCache, ArrayList<Value> list1) {
        throw new RuntimeException();
    }

    public static int fileWrite(String path, int i1, String s1, Object o1) {
        throw new RuntimeException();
    }

    public static String getCurrentDirectory() {
        throw new RuntimeException();
    }

    public static String getUserDirectory() {
        throw new RuntimeException();
    }

    public static boolean isWindows() {
        return false;
    }

    public static void textToLines(String value, ArrayList<String> output) {
        throw new RuntimeException();
    }
}
