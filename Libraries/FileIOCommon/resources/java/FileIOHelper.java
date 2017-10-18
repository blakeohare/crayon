package org.crayonlang.libraries.fileiocommon;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.ArrayList;
import org.crayonlang.interpreter.FastList;
import org.crayonlang.interpreter.structs.Value;

public class FileIOHelper {
	private static int _isWindows = -1;
	
	public static boolean isWindows() {
		if (_isWindows == -1) {
			String osName = System.getProperty("os.name");
			_isWindows = osName.toLowerCase().startsWith("windows") ? 1 : 0;
		}
		return _isWindows == 1;
	}
	
	public static String getUserDirectory() {
		if (isWindows())
		{
			return System.getenv("APPDATA");
		}
		return "~";
	}
	
	private static String currentDirectory = null;
	
	public static String getCurrentDirectory() {
		if (currentDirectory == null) {
			currentDirectory = System.getProperty("user.dir");
		}
		return currentDirectory;
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
		File file = new File(path);
		intOut[0] = file.exists() ? 1 : 0;
		intOut[1] = file.isDirectory() ? 1 : 0;
		double sizeDbl = file.length();
		int size = Integer.MAX_VALUE;
		if (size <= Integer.MAX_VALUE) size = (int) sizeDbl;
		intOut[2] = size;
		intOut[3] = !file.canWrite() ? 1 : 0;
		floatOut[1] = file.lastModified() / 1000.0;
		if ((mask & 4) != 0) {
			Path pathObj = Paths.get(path);
			try {
				BasicFileAttributes attributes = Files.readAttributes(pathObj, BasicFileAttributes.class);
				floatOut[0] = attributes.creationTime().toMillis() / 1000.0;
			} catch (java.io.IOException ioe) {
				floatOut[0] = 0.0;
			}
		}
    }
    
    public static int getDirectoryList(String path, boolean includeFullPath, ArrayList<String> output) {
		File dir = new File(path);
		if (!dir.exists() || !dir.isDirectory()) {
			return 4;
		}
		String prefix = "";
		if (includeFullPath) {
			prefix = path + "/";
			if (isWindows()) {
				prefix = prefix.replace("/", "\\");
			}
		}
		for (File file : dir.listFiles()) {
			output.add(prefix + file.getName());
		}
		return 0;
    }
    
    public static int fileRead(String path, boolean isBytes, String[] stringOut, Value[] integers, FastList byteOutput) {
		File file = new File(path);
		if (!file.exists() || file.isDirectory()) return 4;
		if (file.length() > Integer.MAX_VALUE) return 1;
		
		byte[] bytes = new byte[(int) file.length()];
		int length = bytes.length;
		int bytesRead = 1;
		int offset = 0;
		InputStream stream = null;
		try {
			stream = new FileInputStream(file);
			while (bytesRead > 0 && offset < length) {
				bytesRead = stream.read(bytes, offset, length - offset);
			}
		} catch (java.io.FileNotFoundException fnfe) {
			return 4;
		} catch (java.io.IOException ioe) {
			return 1;
		} finally {
			if (stream != null) {
				try {
					stream.close();
				} catch (java.io.IOException ioe) {
					return 1;
				}
			}
		}
		
		if (isBytes) {
			Value[] outputItems = new Value[length];
			byteOutput.items = outputItems;
			byteOutput.length = length;
			byteOutput.capacity = length;
			for (int i = 0; i < length; ++i) {
				outputItems[i] = integers[(int) bytes[i]];
			}
		} else {
			
			if (length >= 3 && bytes[0] == (byte) 239 && bytes[1] == (byte) 187 && bytes[2] == (byte) 191) {
				length -= 3;
				byte[] tbytes = new byte[length];
				System.arraycopy(bytes, 3, tbytes, 0, length);
				bytes = tbytes;
			}
			
			// Oh dear...
			// It may be prudent to pass in the expected formatting of the file as an optional parameter and fall back to UTF8 as a default.
			try {
				stringOut[0] = new String(bytes, "UTF-8");
			} catch (java.io.UnsupportedEncodingException uee) {
				return 1;
			}
		}
		
		return 0;
    }

	public static int fileWrite(String path, int format, String content, Object bytesObj) {
		byte[] bytes = null;
		try {
			switch (format) {
				case 0:
					bytes = (byte[]) bytesObj;
					break;
				case 1: // UTF8
					bytes = content.getBytes("UTF-8");
					break;
				case 2: // UTF8 w/ BOM
					bytes = content.getBytes("UTF-8");
					byte[] tbytes = new byte[bytes.length + 3];
					System.arraycopy(bytes, 0, tbytes, 3, bytes.length);
					bytes = tbytes;
					bytes[0] = (byte) 239;
					bytes[1] = (byte) 187;
					bytes[2] = (byte) 191;
					break;
				case 3: // UTF16
					bytes = content.getBytes("UTF-16LE");
					break;
				case 4: // UTF32
					bytes = content.getBytes("UTF-32LE");
					break;
				case 5:
					bytes = content.getBytes("ISO-8859-1");
					break;
				default: return 3;
			}
		} catch (java.io.UnsupportedEncodingException uee) {
			throw new RuntimeException("Unsupported encoding exception in fileWrite");
		}
		
		FileOutputStream stream = null;
		try {
			stream = new FileOutputStream(path);
			stream.write(bytes);
			return 0;
		} catch (java.io.IOException ioe) {
			return 1;
		} finally {
			if (stream != null) {
				try {
					stream.close();
				} catch (java.io.IOException ioe) {
					return 1;
				}
			}
		}
	}
	
	public static int fileDelete(String path) {
		File file = new File(path);
		if (!file.exists() || file.isDirectory()) return 4;
		if (file.delete()) {
			return 0;
		}
		return 1;
	}
	
	public static int fileMove(String fromPath, String toPath, boolean isCopy, boolean allowOverwrite) {
		throw new RuntimeException();
	}
	
	public static void textToLines(String text, ArrayList<String> output) {
		int startIndex = 0;
		int length = text.length();
		char c;
		for (int i = 0; i < length; ++i) {
			c = text.charAt(i);
			if (c == '\r' && i + 1 < length && text.charAt(i + 1) == '\n') {
				output.add(text.substring(startIndex, i + 2));
				startIndex = i + 2;
				i++;
			} else if (c == '\r' || c == '\n') {
				output.add(text.substring(startIndex, i + 1));
				startIndex = i + 1;
			}
		}
		output.add(text.substring(startIndex));
	}
	
	public static int createDirectory(String path) {
		System.out.println("Creating a directory: " + path);
		return createDirectoryWithStatus(path);
	}
	
	private static int createDirectoryWithStatus(String path) {
		if (new File(path).mkdir()) {
			return 0;
		}
		// TODO: implement correct return status codes.
		System.out.println("Create dir failure: " + path);
		return 1;
	}
	
	private static boolean deleteStuffRecursive(File item) {
		if (item.exists()) {
			if (item.isDirectory()) {
				for (File child : item.listFiles()) {
					if (!deleteStuffRecursive(child)) {
						return false;
					}
				}
			}
			return item.delete();
		}
		return false;
	}
	
	public static int deleteDirectory(String path) {
		File dir = new File(path);
		if (!dir.exists() || !dir.isDirectory()) return 4;
		if (deleteStuffRecursive(dir)) {
			return 0;
		}
		return 1;
	}
	
	public static int moveDirectory(String from, String to) {
		File fromFile = new File(from);
		File toFile = new File(to);
		if (!fromFile.exists() || !fromFile.isDirectory()) return 4;
		if (toFile.exists()) return 1; // TODO: a better exception
		
		File toDir = toFile.getParentFile();
		if (!toDir.exists() || !toDir.isDirectory()) return 4; // TODO: maybe a better exception here too.
		
		if (fromFile.renameTo(toFile)) {
			return 0;
		}
		return 1;
	}
	
	public static boolean directoryExists(String path) {
		File file = new File(path);
		return file.exists() && file.isDirectory();
	}
	
	public static String getDirRoot(String path) {
		if (isWindows())
		{
			int colonIndex = path.indexOf(':');
			if (colonIndex == -1) throw new RuntimeException("No colon in absolute windows filepath?");
			return path.substring(0, colonIndex) + ":\\";
		}

		return "/";
	}
	
	public static int getDirParent(String path, String[] pathOut) {
		int lastSlash = path.lastIndexOf('/'); // always forward slashes
		if (lastSlash == -1) return 1;
		pathOut[0] = path.substring(0, lastSlash);
		return 0;
	}
}
