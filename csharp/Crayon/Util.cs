using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal static class Util
	{
		public static void EnsureFolderExists(string path)
		{
			string folder = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(folder))
			{
				Util.EnsureFolderExists(folder);
				System.IO.Directory.CreateDirectory(folder);
			}
		}

		public static string FloatToString(double value)
		{
			string output = value.ToString();
			if (!output.Contains('.'))
			{
				output += ".0";
			}
			return output;
		}

		public static string ConvertStringTokenToValue(string tokenValue)
		{
			List<string> output = new List<string>();
			for (int i = 1; i < tokenValue.Length - 1; ++i)
			{
				char c = tokenValue[i];
				if (c == '\\')
				{
					c = tokenValue[++i];
					switch (c)
					{
						case '\\': output.Add("\\"); break;
						case 'n': output.Add("\n"); break;
						case 'r': output.Add("\r"); break;
						case 't': output.Add("\t"); break;
						case '\'': output.Add("'"); break;
						case '"': output.Add("\""); break;
						case '0': output.Add("\0"); break;
						default: return null;
					}
				}
				else
				{
					output.Add("" + c);
				}
			}
			return string.Join("", output);
		}

		public static string ConvertStringValueToCode(string rawValue)
		{
			return ConvertStringValueToCode(rawValue, false);
		}

		private const char ASCII_MAX = (char)127;
		private static readonly string[] HEX_CHARS = "0 1 2 3 4 5 6 7 8 9 a b c d e f".Split(' ');

		public static string ConvertStringValueToCode(string rawValue, bool includeUnicodeEscape)
		{
			int uValue, d1, d2, d3, d4;
			List<string> output = new List<string>() { "\"" };
			foreach (char c in rawValue.ToCharArray())
			{
				if (includeUnicodeEscape && c > ASCII_MAX)
				{
					uValue = c;
					output.Add("\\u");
					d1 = uValue & 15;
					d2 = (uValue >> 4) & 15;
					d3 = (uValue >> 8) & 15;
					d4 = (uValue >> 12) & 15;
					output.Add(HEX_CHARS[d4]);
					output.Add(HEX_CHARS[d3]);
					output.Add(HEX_CHARS[d2]);
					output.Add(HEX_CHARS[d1]);
				}
				else
				{
					switch (c)
					{
						case '"': output.Add("\\\""); break;
						case '\n': output.Add("\\n"); break;
						case '\r': output.Add("\\r"); break;
						case '\0': output.Add("\\0"); break;
						case '\t': output.Add("\\t"); break;
						case '\\': output.Add("\\\\"); break;
						default: output.Add("" + c); break;
					}
				}
			}
			output.Add("\"");

			return string.Join("", output);
		}

		public static string ReadFileExternally(string path, bool canonicalizeNewlines)
		{
			string contents = System.IO.File.ReadAllText(path);
			if (contents.Length >= 3 && contents[0] == 237 && contents[1] == 187 && contents[2] == 191)
			{
				contents = contents.Substring(3);
			}

			if (canonicalizeNewlines)
			{
				contents = contents.Replace("\r\n", "\n").Replace('\r', '\n');
			}

			return contents;
		}

		public static string ReadFileInternally(string path)
		{
			return TrimBomIfPresent(
				string.Join("", Util.ReadBytesInternally(path).Select<byte, char>(b => (char)b)));
		}

		private static string TrimBomIfPresent(string text)
		{
			return (text.Length >= 3 && text[0] == 239 && text[1] == 187 && text[2] == 191)
				? text.Substring(3)
				: text;
		}

		private static readonly byte[] BUFFER = new byte[1000];

		public static byte[] ReadBytesInternally(string path)
		{
			System.IO.Stream stream = typeof(Util).Assembly.GetManifestResourceStream("Crayon." + path.Replace('/', '.'));
			List<byte> output = new List<byte>();
			int bytesRead = 1;
			while (bytesRead > 0)
			{
				bytesRead = stream.Read(BUFFER, 0, BUFFER.Length);
				if (bytesRead == BUFFER.Length)
				{
					output.AddRange(BUFFER);
				}
				else
				{
					for (int i = 0; i < bytesRead; ++i)
					{
						output.Add(BUFFER[i]);
					}
					bytesRead = 0;
				}
			}

			return output.ToArray();
		}

		public static string MassReplacements(string text, Dictionary<string, string> replacements)
		{
			if (text.Contains("%%%"))
			{
				text = Constants.DoReplacements(text);

				foreach (string key in replacements.Keys)
				{
					text = text.Replace("%%%" + key + "%%%", replacements[key]);
				}
			}
			return text;
		}
	}
}
