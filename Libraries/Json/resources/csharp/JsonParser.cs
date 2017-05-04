using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter.Structs;
using Interpreter.Vm;

namespace Interpreter.Libraries.Json
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
                value = Globals.v_VALUE_TRUE;
			}
			else if (PopIfPresent(rawValue, length, i, "false"))
			{
                value = Globals.v_VALUE_FALSE;
			}
			else if (PopIfPresent(rawValue, length, i, "null"))
			{
                value = Globals.v_VALUE_NULL;
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
					return CrayonWrapper.v_buildFloat(value * sign);
				}
			}
			else
			{
				int value;
				if (int.TryParse(stringValue, out value))
				{
					return CrayonWrapper.v_buildInteger(value * sign);
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
			return CrayonWrapper.v_buildString(sb.ToString());
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

			return CrayonWrapper.v_buildList(items);
		}

		private static Value ParseJsonDictionary(char[] rawValue, int length, Index i)
		{
            int stringTypeId = Globals.v_VALUE_EMPTY_STRING.type;

			i.Value++; // '{'
			SkipWhitespace(rawValue, length, i);
			List<string> keys = new List<string>();
			List<Value> values = new List<Value>();
			while (i.Value < length && rawValue[i.Value] != '}')
			{
				if (keys.Count > 0)
				{
					PopExpected(rawValue, length, i, ",");
					SkipWhitespace(rawValue, length, i);
				}

				Value key = ParseJsonThing(rawValue, length, i);
				if (key.type != stringTypeId) throw new JsonParserException();
				SkipWhitespace(rawValue, length, i);
				PopExpected(rawValue, length, i, ":");
				SkipWhitespace(rawValue, length, i);
				Value value = ParseJsonThing(rawValue, length, i);
				SkipWhitespace(rawValue, length, i);
                keys.Add((string)key.internalValue);
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

			return CrayonWrapper.v_buildDictionary(keys.ToArray(), values.ToArray());
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
	}
}
