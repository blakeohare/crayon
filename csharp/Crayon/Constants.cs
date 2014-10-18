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

		public static string DoReplacements(string text)
		{
			foreach (Types t in Enum.GetValues(typeof(Types)).Cast<Types>())
			{
				text = text.Replace("%%%TYPE_" + t.ToString() + "%%%", ((int)t).ToString());
			}

			foreach (string key in values.Keys)
			{
				text = text.Replace("%%%" + key + "%%%", values[key].ToString());
			}
			return text;
		}
	}
}
