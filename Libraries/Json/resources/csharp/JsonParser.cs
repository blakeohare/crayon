using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace %%%PROJECT_ID%%%.Library.Json
{
	internal static class JsonParser
	{
		private class JsonParserException : Exception { }

		private class Index
		{
			public int Value;
		}
		
		public static Value ParseJsonIntoValue(string rawValue)
		{
			try
			{
				return ParseJsonIntoValue(rawValue.ToCharArray(), rawValue.Length, new Index() { Value = 0 });
			}
			catch (JsonParserException)
			{
				return null;
			}
		}

		private static Value ParseJsonIntoValue(char[] rawValue, int length, Index i)
		{
			Value output = ParseJsonThing(rawValue, length, i);
			SkipWhitespace(rawValue, length, i);
			if (i.Value < length) throw new JsonParserException();
			return output;
		}

		private static Value ParseJsonThing(char[] rawValue, int length, Index i)
		{
			SkipWhitespace(rawValue, length, i);
			Value value = null;
			char c = rawValue[i.Value];
			if (c == '{')
			{
				value = ParseJsonDictionary(rawValue, length, i);
			}
			else if (c == '[')
			{
				value = ParseJsonList(rawValue, length, i);
			}
			else if (c == '"')
			{
				value = ParseJsonString(rawValue, length, i);
			}
			else if (c == '.' || c == '-' || (c >= '0' && c <= '9'))
			{
				value = ParseJsonNumber(rawValue, length, i);
			}
			else if (PopIfPresent(rawValue, length, i, "true"))
			{
				value = WrapBoolean(true);
			}
			else if (PopIfPresent(rawValue, length, i, "false"))
			{
				value = WrapBoolean(false);
			}
			else if (PopIfPresent(rawValue, length, i, "null"))
			{
				value = WrapNull();
			}
			else
			{
				throw new JsonParserException();
			}
			return value;
		}

		private static Value ParseJsonNumber(char[] rawValue, int length, Index i)
		{
            int sign = 1;
            char c = rawValue[i.Value];
            if (c == '-')
            {
                sign = -1;
                i.Value++;
            }
			StringBuilder sb = new StringBuilder();
			while (i.Value < length)
			{
				c = rawValue[i.Value++];

				if (c != '.' && (c < '0' || c > '9'))
				{
					i.Value--;
					break;
				}
				else
				{
					sb.Append(c);
				}
			}

			string stringValue = sb.ToString();
			if (stringValue.Contains('.'))
			{
				double value;
				if (double.TryParse(stringValue, out value))
				{
					return WrapFloat(value * sign);
				}
			}
			else
			{
				int value;
				if (int.TryParse(stringValue, out value))
				{
					return WrapInteger(value * sign);
				}
			}

			throw new JsonParserException();
		}

		private static Value ParseJsonString(char[] rawValue, int length, Index i)
		{
			i.Value++; // opening quote
			StringBuilder sb = new StringBuilder();
			while (i.Value < length && rawValue[i.Value] != '"')
			{
				char c = rawValue[i.Value++];
				if (c == '\\')
				{
					switch (rawValue[i.Value++])
					{
						case '\\': c = '\\'; break;
						case '"': c = '"'; break;
						case '\'': c = '\''; break;
						case 'n': c = '\n'; break;
						case 't': c = '\t'; break;
						case 'r': c = '\r'; break;
						case '0': c = '\0'; break;
						default: throw new JsonParserException();
					}
				}
				sb.Append(c);
			}

			if (i.Value >= length) throw new JsonParserException();
			i.Value++; // closing quote
			return WrapString(sb.ToString());
		}

		private static Value ParseJsonList(char[] rawValue, int length, Index i)
		{
			i.Value++; // '['
			SkipWhitespace(rawValue, length, i);
			List<Value> items = new List<Value>();
			while (i.Value < length && rawValue[i.Value] != ']')
			{
				if (items.Count > 0)
				{
					PopExpected(rawValue, length, i, ",");
					SkipWhitespace(rawValue, length, i);
				}

				Value item = ParseJsonThing(rawValue, length, i);
				SkipWhitespace(rawValue, length, i);
				items.Add(item);
			}

			if (i.Value < length) PopExpected(rawValue, length, i, "]");

			return WrapList(items);
		}

		private static Value ParseJsonDictionary(char[] rawValue, int length, Index i)
		{
			i.Value++; // '{'
			SkipWhitespace(rawValue, length, i);
			List<Value> keys = new List<Value>();
			List<Value> values = new List<Value>();
			while (i.Value < length && rawValue[i.Value] != '}')
			{
				if (keys.Count > 0)
				{
					PopExpected(rawValue, length, i, ",");
					SkipWhitespace(rawValue, length, i);
				}

				Value key = ParseJsonThing(rawValue, length, i);
				if (key.type != %%%TYPE_ID_STRING%%%) throw new JsonParserException();
				SkipWhitespace(rawValue, length, i);
				PopExpected(rawValue, length, i, ":");
				SkipWhitespace(rawValue, length, i);
				Value value = ParseJsonThing(rawValue, length, i);
				SkipWhitespace(rawValue, length, i);
				keys.Add(key);
				values.Add(value);
			}

			if (i.Value < length)
			{
				i.Value++; // '}'
			}
			else
			{
				throw new JsonParserException(); // EOF
			}

			return WrapDictionary(keys, values);
		}

		private static Value WrapInteger(int value)
		{
			return CrayonWrapper.v_buildInteger(value);
		}

		private static Value WrapFloat(double value)
		{
			return new Value(%%%TYPE_ID_FLOAT%%%, value);
		}

		private static void PopExpected(char[] rawValue, int length, Index index, string value)
		{
			if (!PopIfPresent(rawValue, length, index, value))
			{
				throw new JsonParserException();
			}
		}

		private static bool PopIfPresent(char[] rawValue, int length, Index index, string value)
		{
			if (index.Value + value.Length > length)
			{
				return false;
			}

			for (int i = 0; i < value.Length; ++i)
			{
				if (rawValue[index.Value + i] != value[i])
				{
					return false;
				}
			}
			index.Value += value.Length;
			return true;
		}

		private static readonly HashSet<char> WHITESPACE = new HashSet<char>(" \t\r\n".ToCharArray());
		private static void SkipWhitespace(char[] rawValue, int length, Index i)
		{
			while (i.Value < length && WHITESPACE.Contains(rawValue[i.Value]))
			{
				i.Value++;
			}
		}

		private static Value WrapNull()
		{
			return CrayonWrapper.v_VALUE_NULL;
		}

		private static Value WrapBoolean(bool value)
		{
			return value ? CrayonWrapper.v_VALUE_TRUE : CrayonWrapper.v_VALUE_FALSE;
		}

		private static Value WrapString(string value)
		{
			return value.Length == 0 ? CrayonWrapper.v_VALUE_EMPTY_STRING : new Value(%%%TYPE_ID_STRING%%%, value);
		}

		private static Value WrapList(List<Value> items)
		{
			return new Value(%%%TYPE_ID_LIST%%%, new List<Value>(items));
		}

		private static Value WrapDictionary(List<Value> keys, List<Value> values)
		{
			Dictionary<string, Value> dictKeyLookup = new Dictionary<string, Value>();
			Dictionary<string, Value> dictValueLookup = new Dictionary<string, Value>();

			for (int i = 0; i < keys.Count; ++i)
			{
				string key = keys[i].internalValue.ToString();
				dictKeyLookup[key] = keys[i];
				dictValueLookup[key] = values[i];
			}

			DictImpl dictionary = new DictImpl(
				new Dictionary<int, Value>(),
				new Dictionary<int, Value>(),
				dictKeyLookup,
				dictValueLookup,
				dictKeyLookup.Count, %%%TYPE_ID_STRING%%%);
			return new Value(%%%TYPE_ID_DICTIONARY%%%, dictionary);
		}
	}
}
