using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal class Constants
	{
		private static readonly Dictionary<string, string> CONSTANT_REPLACEMENTS;

		static Constants()
		{
			Dictionary<string, string> constants = new Dictionary<string, string>()
			{
				{ "TYPE_NATIVE_OBJECT_SCREEN", "1" },
				{ "TYPE_NATIVE_OBJECT_IMAGE", "2" },
			};

			foreach (Types t in Enum.GetValues(typeof(Types)).Cast<Types>())
			{
				constants["TYPE_" + t.ToString()] = ((int)t).ToString();
			}

			foreach (IOErrors errorType in Enum.GetValues(typeof(IOErrors)).Cast<IOErrors>())
			{
				constants["TYPE_" + errorType.ToString()] = ((int)errorType).ToString();
			}

			foreach (AsyncMessageType type in Enum.GetValues(typeof(AsyncMessageType)).Cast<AsyncMessageType>())
			{
				constants["ASYNC_MESSAGE_TYPE_" + type] = ((int)type).ToString();
			}

			CONSTANT_REPLACEMENTS = constants;
		}

		public static string DoReplacements(string text, Dictionary<string, string> replacements)
		{
			if (text.Contains("%%%"))
			{
				string[] parts = text.Split(new string[] { "%%%" }, StringSplitOptions.None);
				bool lastWasReplacement = false;

				List<string> replaced = new List<string>() { parts[0] };
				int i = 1;
				for (; i < parts.Length - 1; ++i)
				{
					string key = parts[i];
					if (CONSTANT_REPLACEMENTS.ContainsKey(key))
					{
						replaced.Add(CONSTANT_REPLACEMENTS[key]);
						replaced.Add(parts[++i]);
						lastWasReplacement = true;
					}
					else if (replacements.ContainsKey(key))
					{
						replaced.Add(replacements[key]);
						replaced.Add(parts[++i]);
						lastWasReplacement = true;
					}
					else
					{
						replaced.Add("%%%");
						replaced.Add(key);
						lastWasReplacement = false;
					}
				}

				if (!lastWasReplacement)
				{
					replaced.Add("%%%");
					replaced.Add(parts[parts.Length - 1]);
				}

				return string.Join("", replaced);
			}
			return text;
		}
	}
}
