package org.crayonlang.libraries.json;

import java.util.ArrayList;

import java.util.ArrayList;
import java.util.HashMap;
import org.crayonlang.interpreter.AwtTranslationHelper;
import org.crayonlang.interpreter.FastList;
import org.crayonlang.interpreter.Interpreter;
import org.crayonlang.interpreter.ResourceReader;
import org.crayonlang.interpreter.structs.*;
import org.crayonlang.interpreter.TranslationHelper;
import org.crayonlang.interpreter.VmGlobal;

class JsonParser {

	private static class JsonParserException extends RuntimeException {
		private static final long serialVersionUID = 1L; 
	}

	public static Value parseJsonIntoValue(String rawValue) {
		try {
			org.json.JSONObject jo = new org.json.JSONObject(rawValue);
			return convertJsonThing(jo);
		} catch (org.json.JSONException je) {
			return null;
		}
	}
	
	private static Value convertJsonThing(Object thing) {
		if (thing instanceof String) {
			return Interpreter.v_buildString((String) thing);
		} else if (thing instanceof Integer) {
			return Interpreter.v_buildInteger((Integer) thing);
		} else if (thing instanceof Double) {
			return Interpreter.v_buildFloat((Double) thing);
		} else if (thing instanceof Boolean) {
			return ((Boolean) thing) ? VmGlobal.VALUE_TRUE : VmGlobal.VALUE_FALSE;
		} else if (thing instanceof org.json.JSONObject) {
			return convertJsonToDictionary((org.json.JSONObject) thing);
		} else if (thing instanceof org.json.JSONArray) {
			return convertJsonToList((org.json.JSONArray) thing);
		} else {
			throw new RuntimeException("Unknown JSON value: " + thing);
		}
	}
	
	private static Value convertJsonToDictionary(org.json.JSONObject obj) {
		String[] keys = org.json.JSONObject.getNames(obj);
		Value[] values = new Value[keys.length];
		for (int i = 0; i < keys.length; ++i) {
			values[i] = convertJsonThing(obj.get(keys[i]));
		}
		return Interpreter.v_buildDictionary(keys, values);
	}
	
	private static Value convertJsonToList(org.json.JSONArray list) {
		ArrayList<Value> items = new ArrayList<>();
		for (int i = 0; i < list.length(); ++i) {
			items.add(convertJsonThing(list.get(i)));
		}
		return Interpreter.v_buildList(new FastList().initializeValueCollection(items));
	}
}
