package org.crayonlang.libraries.processutil;

import java.util.ArrayList;
import org.crayonlang.interpreter.structs.*;

final class ProcessUtilHelper {
	
	private ProcessUtilHelper() { }
	
	static void launchProcessImpl(Object[] bridge, String execName, String[] argStrings, boolean isAsync, Value cb, VmContexct vm) { }
	
	static int readBridgeInt(Object mtx, Object[] bridge, int type) { }
	
	static void readBridgeStrings(Object mtx, Object[] bridge, int type, ArrayList<String> output) { }
}
