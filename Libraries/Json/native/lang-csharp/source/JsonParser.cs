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

		public static Value ParseJsonIntoValue(VmGlobals globals, string rawValue)
		{
			try
			{
				return ParseJsonIntoValue(globals, rawValue.ToCharArray(), rawValue.Length, new Index() { Value = 0 });
			}
			catch (JsonParserException)
			{
				return null;
			}
		}

		private static Value ParseJsonIntoValue(VmGlobals globals, char[] rawValue, int length, Index i)
		{
			Value output = ParseJsonThing(globals, rawValue, length, i);
			SkipWhitespace(rawValue, length, i);
			if (i.Value < length) throw new JsonParserException();
			return output;
		}

		private static Value ParseJsonThing(VmGlobals globals, char[] rawValue, int length, Index i)
		{
			SkipWhitespace(rawValue, length, i);
			Value value = null;
			char c = rawValue[i.Value];
			if (c == '{')
			{
				value = ParseJsonDictionary(globals, rawValue, length, i);
			}
			else if (c == '[')
			{
				value = ParseJsonList(globals, rawValue, length, i);
			}
			else if (c == '"')
			{
				value = ParseJsonString(globals, rawValue, length, i);
			}
			else if (c == '.' || c == '-' || (c >= '0' && c <= '9'))
			{
				value = ParseJsonNumber(globals, rawValue, length, i);
			}
			else if (PopIfPresent(rawValue, length, i, "true"))
			{
				value = globals.boolTrue;
			}
			else if (PopIfPresent(rawValue, length, i, "false"))
			{
				value = globals.boolFalse;
			}
			else if (PopIfPresent(rawValue, length, i, "null"))
			{
				value = globals.valueNull;
			}
			else
			{
				throw new JsonParserException();
			}
			return value;
		}

		private static Value ParseJsonNumber(VmGlobals globals, char[] rawValue, int length, Index i)
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
					return CrayonWrapper.buildFloat(globals, value * sign);
				}
			}
			else
			{
				int value;
				if (int.TryParse(stringValue, out value))
				{
					return CrayonWrapper.buildInteger(globals, value * sign);
				}
			}

			throw new JsonParserException();
		}

		private static Value ParseJsonString(VmGlobals globals, char[] rawValue, int length, Index i)
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
						case '\\': sb.Append('\\'); break;
						case '"': sb.Append('"'); break;
						case '\'': sb.Append('\''); break;
						case 'n': sb.Append('\n'); break;
						case 't': sb.Append('\t'); break;
						case 'r': sb.Append('\r'); break;
						case '0': sb.Append('\0'); break;
						case 'u':
							if (i.Value + 4 < length)
							{
								char u1 = rawValue[i.Value];
								char u2 = rawValue[i.Value + 1];
								char u3 = rawValue[i.Value + 2];
								char u4 = rawValue[i.Value + 3];
								i.Value += 4;
								string strValue = UnicodeToString("" + u1 + u2 + u3 + u4);
								sb.Append(strValue);
							}
							else
							{
								throw new JsonParserException();
							}
							break;
						default:
							throw new JsonParserException();
					}
				}
				else
				{
					sb.Append(c);
				}
			}

			if (i.Value >= length) throw new JsonParserException();
			i.Value++; // closing quote
			return CrayonWrapper.buildString(globals, sb.ToString());
		}

		private static byte[] buffer = new byte[4];
		private static string UnicodeToString(string code)
		{
			int value;
			if (!int.TryParse(code, System.Globalization.NumberStyles.HexNumber, null, out value))
			{
				throw new JsonParserException();
			}
			buffer[3] = (byte)((value >> 24) & 255);
			buffer[2] = (byte)((value >> 16) & 255);
			buffer[1] = (byte)((value >> 8) & 255);
			buffer[0] = (byte)((value >> 0) & 255);
			return System.Text.Encoding.UTF32.GetString(buffer);
		}

		private static Value ParseJsonList(VmGlobals globals, char[] rawValue, int length, Index i)
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

				Value item = ParseJsonThing(globals, rawValue, length, i);
				SkipWhitespace(rawValue, length, i);
				items.Add(item);
			}

			if (i.Value < length) PopExpected(rawValue, length, i, "]");

			return CrayonWrapper.buildList(items);
		}

		private static Value ParseJsonDictionary(VmGlobals globals, char[] rawValue, int length, Index i)
		{
			int stringTypeId = globals.stringEmpty.type;

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

				Value key = ParseJsonThing(globals, rawValue, length, i);
				if (key.type != stringTypeId) throw new JsonParserException();
				SkipWhitespace(rawValue, length, i);
				PopExpected(rawValue, length, i, ":");
				SkipWhitespace(rawValue, length, i);
				Value value = ParseJsonThing(globals, rawValue, length, i);
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

			return CrayonWrapper.buildStringDictionary(globals, keys.ToArray(), values.ToArray());
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
