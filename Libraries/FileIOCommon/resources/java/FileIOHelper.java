package crayonlib.fileiocommon;

import %%%PACKAGE%%%.Value;
import java.util.ArrayList;

public class FileIOHelper {
	private static int _isWindows = -1;
	
	public static boolean isWindows() {
		throw new RuntimeException();
	}
	
	public static String getUserDirectory() {
		throw new RuntimeException();
	}
	
	public static String getCurrentDirectory() {
		throw new RuntimeException();
	}
	
	private static String normalizePath(String path) {
		if (path.length() == 2 && path.charAt(1) == ':')
		{
			path += "/";
		}
		if (isWindows())
		{
			path = path.replace('/', '\\');
		}
		return path;
	}
	
    public static void getFileInfo(String path, int mask, int[] intOut, double[] floatOut) {
		throw new RuntimeException();
    }
    
    public static int getDirectoryList(String path, boolean includeFullPath, ArrayList<String> output) {
		throw new RuntimeException();
    }
    
    public static int fileRead(String path, boolean isBytes, String[] stringOut, Value[] integers, ArrayList<Value> byteOutput) {
		throw new RuntimeException();
    }

	public static int fileWrite(String path, int format, String content, Object bytesObj) {
		throw new RuntimeException();
	}
	
	public static int fileDelete(String path) {
		throw new RuntimeException();
	}
	
	public static int fileMove(String fromPath, String toPath, boolean isCopy, boolean allowOverwrite) {
		throw new RuntimeException();
	}
	
	public static void textToLines(String text, ArrayList<String> output) {
		throw new RuntimeException();
	}
	
	public static int createDirectory(String path) {
		throw new RuntimeException();
	}
	
	private static int createDirectoryWithStatus(String path) {
		throw new RuntimeException();
	}
	
	public static int deleteDirectory(String path) {
		throw new RuntimeException();
	}
	
	public static int moveDirectory(String from, String to) {
		throw new RuntimeException();
	}
	
	public static boolean directoryExists(String path) {
		throw new RuntimeException();
	}
	
	public static String getDirRoot(String path) {
		throw new RuntimeException();
	}
	
	public static int getDirParent(String path, String[] pathOut) {
		throw new RuntimeException();
	}
}
