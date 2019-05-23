package crayonlib.zip;

import org.crayonlang.interpreter.structs.*;

import java.io.DataOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

final class ZipHelper {
	private ZipHelper() { }
	
	public static Object createZipReader(int[] byteArray) {
		throw new RuntimeException("Not implemented");
	}
	
	public static void readNextZipEntry(Object zipArchiveObj, int index, boolean[] boolsOut, String[] nameOut, ArrayList<Integer> bytesOut) {
		throw new RuntimeException("Not implemented");
	}
}
