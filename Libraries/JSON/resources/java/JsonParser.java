package %%%PACKAGE%%%;

import java.util.ArrayList;

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
			return CrayonWrapper.v_buildString((String) thing);
		} else if (thing instanceof Integer) {
			return CrayonWrapper.v_buildInteger((Integer) thing);
		} else if (thing instanceof Double) {
			return CrayonWrapper.v_buildFloat((Double) thing);
		} else if (thing instanceof Boolean) {
			return ((Boolean) thing) ? CrayonWrapper.v_VALUE_TRUE : CrayonWrapper.v_VALUE_FALSE;
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
		return CrayonWrapper.v_buildDictionary(keys, values);
	}
	
	private static Value convertJsonToList(org.json.JSONArray list) {
		ArrayList<Value> items = new ArrayList<>();
		for (int i = 0; i < list.length(); ++i) {
			items.add(convertJsonThing(list.get(i)));
		}
		return CrayonWrapper.v_buildList(items);
	}
}
