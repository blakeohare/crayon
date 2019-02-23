package crayonlib.textencoding;

import org.crayonlang.interpreter.structs.*;

public class TextEncodingHelper {

	public static int bytesToText(int[] unwrappedBytes, int format, String[] strOut) {
		int length = unwrappedBytes.length;
		
		String encoding = "";
		switch (format) {
			case 1: encoding = "US-ASCII"; break;
			case 2: encoding = "ISO-8859-1"; break;
			case 3: encoding = "UTF-8"; break;
			case 4: encoding = "UTF-16LE"; break;
			case 5: encoding = "UTF-16BE"; break;
			
			case 6:
			case 7:
				int[] codePoints = new int[length / 4];
				int k = 0;
				int j = 0;
				byte b;
				for (int i = 0; i < length; i += 4) {
					if (format == 7) {
						for (j = 0; j < 4; ++j) {
							codePoints[k] = (codePoints[k] << 8) | unwrappedBytes[i | j];
						}
					} else {
						for (j = 3; j >= 0; --j) {
							codePoints[k] = (codePoints[k] << 8) | unwrappedBytes[i | j];
						}
					}
					k++;
				}
				strOut[0] = new String(codePoints, 0, codePoints.length);
				return 0;
		}
		
		byte[] bytes = new byte[length];
		for (int i = 0; i < length; ++i) {
			bytes[i] = (byte) unwrappedBytes[i];
		}
		String s = null;
		try {
			s = new String(bytes, encoding);
		} catch (Exception e) {
			return 1;
		}
		strOut[0] = s;
		return 0;
	}
	
	public static int textToBytes(
		String value,
		boolean includeBom,
		int format,
		java.util.ArrayList<Value> byteList,
		Value[] positiveIntegers,
		int[] intOut) {
		
		intOut[0] = 0;
		
		byte[] bytes;
		int bytesLength;
		switch (format) {
			case 1:
			case 2:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.US_ASCII);
				bytesLength = bytes.length;
				break;
			case 3:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.UTF_8);
				bytesLength = bytes.length;
				break;
			case 4:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.UTF_16LE);
				bytesLength = bytes.length;
				break;
			case 5:
				bytes = value.getBytes(java.nio.charset.StandardCharsets.UTF_16BE);
				bytesLength = bytes.length;
				break;
			case 6:
			case 7:
				int stringLength = value.length();
				int i = 0;
				bytes = new byte[stringLength * 4];
				bytesLength = 0;
				boolean swapEndianness = format == 6;
				while (i < stringLength) {
					int codePoint = Character.codePointAt(value, i);
					bytes[bytesLength | (swapEndianness ? 3 : 0)] = (byte) ((codePoint >> 24) & 255);
					bytes[bytesLength | (swapEndianness ? 2 : 1)] = (byte) ((codePoint >> 16) & 255);
					bytes[bytesLength | (swapEndianness ? 1 : 2)] = (byte) ((codePoint >> 8) & 255);
					bytes[bytesLength | (swapEndianness ? 0 : 3)] = (byte) (codePoint & 255);
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
				case 2:
					break;
				case 3:
					byteList.add(positiveIntegers[239]);
					byteList.add(positiveIntegers[187]);
					byteList.add(positiveIntegers[191]);
					break;
				case 4:
					byteList.add(positiveIntegers[255]);
					byteList.add(positiveIntegers[254]);
					break;
				case 5:
					byteList.add(positiveIntegers[254]);
					byteList.add(positiveIntegers[255]);
					break;
				case 6:
					byteList.add(positiveIntegers[255]);
					byteList.add(positiveIntegers[254]);
					byteList.add(positiveIntegers[0]);
					byteList.add(positiveIntegers[0]);
					break;
				case 7:
					byteList.add(positiveIntegers[0]);
					byteList.add(positiveIntegers[0]);
					byteList.add(positiveIntegers[254]);
					byteList.add(positiveIntegers[255]);
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
}
