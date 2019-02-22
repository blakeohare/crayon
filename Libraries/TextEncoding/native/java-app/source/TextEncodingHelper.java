package crayonlib.textencoding;

import org.crayonlang.interpreter.structs.*;

public class TextEncodingHelper {

	public static int bytesToText(int[] unwrappedBytes, int format, String[] strOut) {
		throw new RuntimeException();
	}
	
	public static int textToBytes(
		String value,
		boolean includeBom,
		int format,
		java.util.ArrayList<Value> byteList,
		Value[] positiveIntegers) {
		
		byte[] bytes;
		int bytesLength;
		switch (format) {
			case 1:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.US_ASCII);
				bytesLength = bytes.length;
				break;
			case 2:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.UTF_8);
				bytesLength = bytes.length;
				break;
			case 3:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.UTF_16LE);
				bytesLength = bytes.length;
				break;
			case 4:
				int stringLength = value.length();
				int i = 0;
				bytes = new byte[stringLength * 4];
				bytesLength = 0;
				while (i < stringLength) {
					int codePoint = Character.codePointAt(value, i);
					bytes[bytesLength | 3] = (byte) ((codePoint >> 24) & 255);
					bytes[bytesLength | 2] = (byte) ((codePoint >> 16) & 255);
					bytes[bytesLength | 1] = (byte) ((codePoint >> 8) & 255);
					bytes[bytesLength] = (byte) (codePoint & 255);
					i += Character.charCount(codePoint);
					bytesLength += 4;
				}
				break;
			default:
				throw new RuntimeException("Not implemented");
		}
		
		if (includeBom) {
			switch (format) {
				case 1:
					break;
				case 2:
					byteList.add(positiveIntegers[239]);
					byteList.add(positiveIntegers[187]);
					byteList.add(positiveIntegers[191]);
					break;
				case 3:
					byteList.add(positiveIntegers[255]);
					byteList.add(positiveIntegers[254]);
					break;
				case 4:
					byteList.add(positiveIntegers[255]);
					byteList.add(positiveIntegers[254]);
					byteList.add(positiveIntegers[0]);
					byteList.add(positiveIntegers[0]);
					break;
			}
		}
		
		int b;
		for (int i = 0; i < bytesLength; ++i) {
			b = bytes[i];
			if (b < 0) b += 256;
			byteList.add(positiveIntegers[b]);
		}
		
		return 0;
	}
	
	private static byte[] StringToByteArray(String value, java.nio.charset.Charset encoding) {
		//try {
			return value.getBytes(encoding);
		//} catch (java.io.UnsupportedEncodingException uee) {
		//	return null; // not thrown
		//}
	}
}
