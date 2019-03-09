package org.crayonlang.libraries.dispatcher;

import java.util.ArrayList;
import org.crayonlang.interpreter.structs.*;

public class DispatcherHelper {

	public static void flushNativeQueue(Object[] nativeData, ArrayList<Value> output) {
		synchronized (nativeData[0]) {
			ArrayList<Value> q = (ArrayList<Value>) nativeData[1];
			for (int i = 0; i < q.size(); ++i) {
				output.add(q.get(i));
			}
			q.clear();
		}
	}

}
