using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal class Constants
	{
		private static readonly Dictionary<string, int> values = new Dictionary<string, int>()
		{
			{ "TYPE_NATIVE_OBJECT_SCREEN", 1 },
			{ "TYPE_NATIVE_OBJECT_IMAGE", 2 },
		};

		public static string DoReplacements(string text, Dictionary<string, string> replacements)
		{
			foreach (Types t in Enum.GetValues(typeof(Types)).Cast<Types>())
			{
				text = text.Replace("%%%TYPE_" + t.ToString() + "%%%", ((int)t).ToString());
			}

			foreach (string key in values.Keys)
			{
				text = text.Replace("%%%" + key + "%%%", values[key].ToString());
			}

			foreach (IOErrors errorType in Enum.GetValues(typeof(IOErrors)).Cast<IOErrors>())
			{
				text = text.Replace("%%%IO_ERROR_" + errorType.ToString() + "%%%", "" + (int)errorType);
			}

			foreach (string key in replacements.Keys)
			{
				text = text.Replace("%%%" + key + "%%%", replacements[key]);
			}

			foreach (AsyncMessageType type in Enum.GetValues(typeof(AsyncMessageType)).Cast<AsyncMessageType>())
			{
				text = text.Replace("%%%ASYNC_MESSAGE_TYPE_" + type + "%%%", ((int)type).ToString());
			}
			return text;
		}
	}
}
