package crayonlib.http;

import org.crayonlang.interpreter.structs.*;

import java.io.DataOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class HttpHelper {

	public static void readResponseData(
		Object httpRequest,
		int[] intOut,
		String[] stringOut,
		Object[] responseNativeData,
		ArrayList<String> headersOut) {
		
		HttpResponseValue response = (HttpResponseValue)httpRequest;
		intOut[0] = response.statusCode;
		intOut[1] = response.isContentBinary ? 1 : 0;
		stringOut[0] = response.statusMessage;
		stringOut[1] = response.contentString;
		responseNativeData[0] = response.contentBytes;
		for (int i = 0; i < response.headers.length; ++i) {
			headersOut.add(response.headers[i]);
		}
	}

	public static void getResponseBytes(
		Object byteArrayObj,
		Value[] integersCache,
		ArrayList<Value> output) {
		
		byte[] bytes = (byte[])byteArrayObj;
		int length = bytes.length;
		for (int i = 0; i < length; ++i)
		{
			output.add(integersCache[bytes[i]]);
		}
	}

	public static void sendRequestAsync(
		final Object[] requestNativeData,
		String method,
		String url,
		ArrayList<String> headers,
		int contentMode, // 0 - null, 1 - text, 2 - binary
		Object content,
		boolean outputIsBinary,
		VmContext vmContext,
		Value callbackFp,
		Object[] mutexStuff) {
		
		requestNativeData[1] = new Object();
		
		new Thread(new Runnable() {
			public void run() {
				HttpResponseValue response = sendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary);
				synchronized (requestNativeData[1]) {
					requestNativeData[0] = response;
					requestNativeData[2] = true;
				}
				
				synchronized(mutexStuff[0]) {
					((ArrayList<Value>)mutexStuff[1]).add(callbackFp);
				}
			}
		}).start();
	}

	public static boolean sendRequestSync(
		Object[] requestNativeData,
		String method,
		String url,
		ArrayList<String> headers,
		int contentMode, // 0 - null, 1 - text, 2 - binary
		Object content,
		boolean outputIsBinary) {
		
		HttpResponseValue response = sendRequestSyncImpl(requestNativeData, method, url, headers, contentMode, content, outputIsBinary);
		requestNativeData[0] = response;
		requestNativeData[2] = true;
		return false;
	}

	private static HttpResponseValue sendRequestSyncImpl(
		Object[] requestNativeData,
		String method,
		String urlString,
		ArrayList<String> headers,
		int contentMode, // 0 - null, 1 - text, 2 - binary
		Object content,
		boolean outputIsBinary) {
		
		String responseString = null;
		byte[] responseBytes = null;
		int statusCode = 0;
		String statusMessage = "";
		ArrayList<String> responseHeaders = new ArrayList<>();
		
		HttpURLConnection connection = null;
		try {
			URL url = new URL(urlString);
			connection = (HttpURLConnection) url.openConnection();
			connection.setRequestMethod(method);
			boolean contentLengthSet = false;
			for (int i = 0; i < headers.size(); i += 2) {
				String headerName = headers.get(i);
				String headerValue = headers.get(i + 1);
				connection.setRequestProperty(headerName, headerValue);
				if (headerName.toLowerCase().equals("content-length")) {
					contentLengthSet = true;
				}
			}
			// TODO: verify this isn't automatically done for you.
			if (!contentLengthSet && contentMode != 0) {
				int length;
				if (contentMode == 1) {
					length = ((String)content).length();
				} else {
					length = ((byte[])content).length;
				}
				connection.setRequestProperty("Content-Length", "" + length);
			}
			
			if (contentMode != 0) {
				DataOutputStream wr = new DataOutputStream(connection.getOutputStream());
				if (contentMode == 1) {
					wr.writeBytes((String) content);
				} else {
					throw new RuntimeException("Bytes not implemented yet.");
				}
				wr.close();
			}
			
			InputStream is = connection.getInputStream();
			
			if (outputIsBinary) {
				java.io.ByteArrayOutputStream outputStream = new java.io.ByteArrayOutputStream();
				int bytesRead = 1;
				byte[] buffer = new byte[1024];
				while (bytesRead > -1) {
					bytesRead = is.read(buffer, 0, buffer.length);
					if (bytesRead > -1) {
						outputStream.write(buffer, 0, bytesRead);
					}
				}
				
				outputStream.flush();
				responseBytes = outputStream.toByteArray();
			} else {
				StringBuilder sb = new StringBuilder();
				char[] buffer = new char[1024];
				try {
					// TODO: something better than just assuming UTF-8
					Reader in = new InputStreamReader(is, "UTF-8");
					while (true) {
						int charsRead = in.read(buffer, 0, buffer.length);
						if (charsRead < 0) {
							break;
						}
						sb.append(buffer, 0, charsRead);
					}
				} catch (Exception e2) {
					
				}
				responseString = sb.toString();
			}
			
			statusCode = connection.getResponseCode();
			statusMessage = connection.getResponseMessage();
			if (statusMessage == null) statusMessage = "";
			for (Map.Entry<String, List<String>> headerPair : connection.getHeaderFields().entrySet()) {
				String headerName = headerPair.getKey();
				if (headerName != null) {
					for (String headerValue : headerPair.getValue()) {
						responseHeaders.add(headerName);
						if (headerValue == null) headerValue = "";
						responseHeaders.add(headerValue);
					}
				}
			}
		} catch (Exception e1) {
			System.out.println(e1);
		} finally {
			if (connection != null) {
				connection.disconnect();
			}
		}
		
		HttpResponseValue output = new HttpResponseValue();
		output.statusCode = statusCode;
		output.statusMessage = statusMessage;
		output.isContentBinary = outputIsBinary;
		output.contentString = responseString;
		output.contentBytes = responseBytes;
		output.headers = responseHeaders.toArray(new String[0]);
		return output;
	}

	private static class HttpResponseValue
	{
		public int statusCode;
		public String statusMessage;
		public boolean isContentBinary;
		public String contentString;
		public byte[] contentBytes;
		public String[] headers;
	}

	public static boolean pollRequest(final Object[] requestNativeData) {
		boolean output = false;
		synchronized (requestNativeData[1]) {
			output = (boolean) requestNativeData[2];
		}
		return output;
	}
}
