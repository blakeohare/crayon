using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal static class Util
	{
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
			List<string> output = new List<string>() { "\"" };
			foreach (char c in rawValue.ToCharArray())
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
			string[] foo = typeof(Util).Assembly.GetManifestResourceNames();
			System.IO.Stream stream = typeof(Util).Assembly.GetManifestResourceStream("Crayon." + path.Replace('/', '.'));
			List<string> output = new List<string>();
			int byteRead = -1;
			char c;
			do
			{
				byteRead = stream.ReadByte();
				if (byteRead >= 0 && byteRead < 65535)
				{
					c = (char)(byte)byteRead;
					output.Add("" + c);
				}
			} while (byteRead >= 0 && byteRead < 65535);

			string finalOutput = string.Join("", output);

			if (finalOutput.Length >= 3 && finalOutput[0] == 239 && finalOutput[1] == 187 && finalOutput[2] == 191)
			{
				finalOutput = finalOutput.Substring(3);
			}

			return finalOutput;
		}
	}
}
