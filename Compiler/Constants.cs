using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	public class Constants
	{
		private static readonly Dictionary<string, string> CONSTANT_REPLACEMENTS;

		private static readonly Dictionary<string, int> PRIMITIVE_FIELD_IDS;

		static Constants()
		{
			Dictionary<string, string> constants = new Dictionary<string, string>()
			{
				{ "OPEN_GL_ELLIPSE_POINT_COUNT", "60" },
			};

			Dictionary<string, Type> enumReplacementsByPrefix = new Dictionary<string, Type>()
			{
				{ "ASYNC_MESSAGE_TYPE", typeof(AsyncMessageType) },
				{ "FF", typeof(FrameworkFunction) },
				{ "IO_ERROR", typeof(IOErrors) },
				{ "PRIMITIVE_METHOD", typeof(PrimitiveMethods) },
				{ "SUBTYPE_ID", typeof(SubTypes) },
				{ "TYPE_ID", typeof(Types) },
				{ "TYPE_NATIVE_OBJECT", typeof(NativeObjectTypes) },
			};

			foreach (string key in enumReplacementsByPrefix.Keys)
			{
				foreach (object enumValue in Enum.GetValues(enumReplacementsByPrefix[key]))
				{
					constants.Add(key + "_" + enumValue.ToString(), ((int)enumValue).ToString());
				}
			}

			Dictionary<string, int> primitiveFieldIds = new Dictionary<string, int>();
			List<string> fields = new List<string>() { "length" };

			foreach (PrimitiveMethods primitiveMethod in PrimitiveMethodsUtil.GetAllMethods())
			{
				string name = PrimitiveMethodsUtil.GetMethodName(primitiveMethod);
				if (name != null)
				{
					fields.Add(name);
				}
			}

			fields.AddRange(new string[] {
				// events
				"button", "down", "key", "type", "x", "y", "player", "name", "value", "device", "is_significant", "descriptor", 
				"is_key", "is_mouse", "is_quit", "is_gamepad", "is_hardware",

				// gamepad
				"id", "name", "has_configuration", 

				// http request
				"completed", "content", "code",

				// image
				"width", "height",
			});

			constants.Add("PREDEFINED_FIELD_COUNT", "" + (fields.Count + 1));

			foreach (string field in fields)
			{
				if (field == "count") throw new Exception(); // name collision with %%%PREDEFINED_FIELD_COUNT%%%

				if (!primitiveFieldIds.ContainsKey(field))
				{
					int id = primitiveFieldIds.Count + 1;
					primitiveFieldIds.Add(field, id);
					constants.Add("PREDEFINED_FIELD_" + field.ToUpperInvariant(), "" + id);
				}
			}

			PRIMITIVE_FIELD_IDS = primitiveFieldIds;

			CONSTANT_REPLACEMENTS = constants;
		}

		internal static int GetPrimitiveFieldId(string name)
		{
			int id = 0;
			PRIMITIVE_FIELD_IDS.TryGetValue(name, out id);
			return id;
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
