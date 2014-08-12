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
		};

		public static string DoReplacements(string text)
		{
			Dictionary<string, object> replacements = new Dictionary<string, object>();

			foreach (Types t in Enum.GetValues(typeof(Types)).Cast<Types>())
			{
				replacements.Add("%%%TYPE_" + t.ToString() + "%%%", (int)t);
			}

			foreach (string key in replacements.Keys)
			{
				text = text.Replace(key, replacements[key].ToString());
			}

			foreach (string key in values.Keys)
			{
				text = text.Replace("%%%" + key + "%%%", values[key].ToString());
			}
			return text;
		}
	}
}
