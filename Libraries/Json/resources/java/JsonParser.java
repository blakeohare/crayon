package org.crayonlang.libraries.json;

import java.util.ArrayList;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.FastList;
import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.ResourceReader;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

class JsonParser {

	private static class JsonParserException extends RuntimeException {
		private static final long serialVersionUID = 1L; 
	}

	public static Value parseJsonIntoValue(VmGlobals globals, String rawValue) {
		try {
			org.json.JSONObject jo = new org.json.JSONObject(rawValue);
			return convertJsonThing(globals, jo);
		} catch (org.json.JSONException je) {
			return null;
		}
	}
	
	private static Value convertJsonThing(VmGlobals globals, Object thing) {
		if (thing instanceof String) {
			return Interpreter.v_buildString(globals, (String) thing);
		} else if (thing instanceof Integer) {
			return Interpreter.v_buildInteger(globals, (Integer) thing);
		} else if (thing instanceof Double) {
			return Interpreter.v_buildFloat(globals, (Double) thing);
		} else if (thing instanceof Boolean) {
			return ((Boolean) thing) ? globals.boolTrue: globals.boolFalse;
		} else if (thing instanceof org.json.JSONObject) {
			return convertJsonToDictionary(globals, (org.json.JSONObject) thing);
		} else if (thing instanceof org.json.JSONArray) {
			return convertJsonToList(globals, (org.json.JSONArray) thing);
		} else {
			throw new RuntimeException("Unknown JSON value: " + thing);
		}
	}
	
	private static Value convertJsonToDictionary(VmGlobals globals, org.json.JSONObject obj) {
		String[] keys = org.json.JSONObject.getNames(obj);
		Value[] values = new Value[keys.length];
		for (int i = 0; i < keys.length; ++i) {
			values[i] = convertJsonThing(globals, obj.get(keys[i]));
		}
		return Interpreter.v_buildStringDictionary(globals, keys, values);
	}
	
	private static Value convertJsonToList(VmGlobals globals, org.json.JSONArray list) {
		FastList output = new FastList();
		for (int i = 0; i < list.length(); ++i) {
			output.add(convertJsonThing(globals, list.get(i)));
		}
		return Interpreter.v_buildList(output);
	}
}
