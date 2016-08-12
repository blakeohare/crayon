package crayonlib.http;

import java.util.ArrayList;
import %%%PACKAGE%%%.Value;

public class HttpHelper {

	public static void readResponseData(
		Object httpRequest,
		int[] intOut,
		String[] stringOut,
		Object[] responseNativeData,
		ArrayList<String> headersOut) {
		throw new RuntimeException();
	}

	public static void getResponseBytes(
		Object byteArrayObj,
		Value[] integersCache,
		ArrayList<Value> output) {
		throw new RuntimeException();
	}

	public static void sendRequestAsync(
		Object[] requestNativeData,
		String method,
		String url,
		ArrayList<String> headers,
		int contentMode, // 0 - null, 1 - text, 2 - binary
		Object content,
		boolean outputIsBinary) {
		
		throw new RuntimeException();
	}

	public static boolean sendRequestSync(
		Object[] requestNativeData,
		String method,
		String url,
		ArrayList<String> headers,
		int contentMode, // 0 - null, 1 - text, 2 - binary
		Object content,
		boolean outputIsBinary) {
		
		throw new RuntimeException();
	}

	private static HttpResponseValue sendRequestSyncImpl(
		Object[] requestNativeData,
		String method,
		String url,
		ArrayList<String> headers,
		int contentMode, // 0 - null, 1 - text, 2 - binary
		Object content,
		boolean outputIsBinary) {
		throw new RuntimeException();
	}

	private static class HttpResponseValue
	{
		public int StatusCode;
		public String StatusDescription;
		public boolean IsContentBinary;
		public String ContentString;
		public byte[] ContentBytes;
		public String[] Headers;
	}

	public static boolean pollRequest(Object[] requestNativeData) {
		throw new RuntimeException();
	}
}
