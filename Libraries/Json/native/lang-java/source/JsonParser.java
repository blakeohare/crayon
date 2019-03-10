package org.crayonlang.libraries.json;

import java.util.ArrayList;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.vm.CrayonWrapper;
import org.crayonlang.interpreter.ResourceReader;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;

class JsonParser {

	private static class JsonParserException extends RuntimeException {
		private static final long serialVersionUID = 1L; 
	}

	public static Value parseJsonIntoValue(VmGlobals globals, String rawValue) {
		try {
			JSONObject jo = new JSONObject(rawValue);
			return convertJsonThing(globals, jo);
		} catch (JSONException je) {
			return null;
		}
	}
	
	private static Value convertJsonThing(VmGlobals globals, Object thing) {
		if (thing instanceof String) {
			return CrayonWrapper.buildString(globals, (String) thing);
		} else if (thing instanceof Integer) {
			return CrayonWrapper.buildInteger(globals, (Integer) thing);
		} else if (thing instanceof Double) {
			return CrayonWrapper.buildFloat(globals, (Double) thing);
		} else if (thing instanceof Boolean) {
			return ((Boolean) thing) ? globals.boolTrue: globals.boolFalse;
		} else if (thing instanceof JSONObject) {
			return convertJsonToDictionary(globals, (JSONObject) thing);
		} else if (thing instanceof JSONArray) {
			return convertJsonToList(globals, (JSONArray) thing);
		} else {
			throw new RuntimeException("Unknown JSON value: " + thing);
		}
	}
	
	private static Value convertJsonToDictionary(VmGlobals globals, JSONObject obj) {
		String[] keys = JSONObject.getNames(obj);
		Value[] values = new Value[keys.length];
		for (int i = 0; i < keys.length; ++i) {
			values[i] = convertJsonThing(globals, obj.get(keys[i]));
		}
		return CrayonWrapper.buildStringDictionary(globals, keys, values);
	}
	
	private static Value convertJsonToList(VmGlobals globals, JSONArray list) {
		ArrayList<Value> output = new ArrayList<Value>();
		for (int i = 0; i < list.length(); ++i) {
			output.add(convertJsonThing(globals, list.get(i)));
		}
		return CrayonWrapper.buildList(output);
	}
}
