using System;
using System.Linq;

namespace Crayon
{
	internal static class PrimitiveMethodsUtil
	{
		public static PrimitiveMethods[] GetAllMethods()
		{
			return Enum.GetValues(typeof(PrimitiveMethods)).Cast<PrimitiveMethods>().ToArray();
		}

		public static string GetMethodName(PrimitiveMethods method)
		{
			switch (method)
			{
				case PrimitiveMethods.DICTIONARY_CLEAR: return "clear";
				case PrimitiveMethods.DICTIONARY_CLONE: return "clone";
				case PrimitiveMethods.DICTIONARY_CONTAINS: return "contains";
				case PrimitiveMethods.DICTIONARY_GET: return "get";
				case PrimitiveMethods.DICTIONARY_KEYS: return "keys";
				case PrimitiveMethods.DICTIONARY_REMOVE: return "remove";
				case PrimitiveMethods.DICTIONARY_VALUES: return "values";
				case PrimitiveMethods.LIST_ADD: return "add";
				case PrimitiveMethods.LIST_CHOICE: return "choice";
				case PrimitiveMethods.LIST_CLEAR: return "clear";
				case PrimitiveMethods.LIST_CLONE: return "clone";
				case PrimitiveMethods.LIST_CONCAT: return "concat";
				case PrimitiveMethods.LIST_CONTAINS: return "contains";
				case PrimitiveMethods.LIST_INSERT: return "insert";
				case PrimitiveMethods.LIST_JOIN: return "join";
				case PrimitiveMethods.LIST_POP: return "pop";
				case PrimitiveMethods.LIST_REMOVE: return "remove";
				case PrimitiveMethods.LIST_REVERSE: return "reverse";
				case PrimitiveMethods.LIST_SHUFFLE: return "shuffle";
				case PrimitiveMethods.LIST_SORT: return "sort";
				case PrimitiveMethods.STRING_CONTAINS: return "contains";
				case PrimitiveMethods.STRING_ENDSWITH: return "endsWith";
				case PrimitiveMethods.STRING_INDEXOF: return "indexOf";
				case PrimitiveMethods.STRING_LOWER: return "lower";
				case PrimitiveMethods.STRING_REPLACE: return "replace";
				case PrimitiveMethods.STRING_REVERSE: return "reverse";
				case PrimitiveMethods.STRING_SPLIT: return "split";
				case PrimitiveMethods.STRING_STARTSWITH: return "startsWith";
				case PrimitiveMethods.STRING_TRIM: return "trim";
				case PrimitiveMethods.STRING_UPPER: return "upper";
				case PrimitiveMethods.MAX_PRIMITIVE_VALUE: return null;
				default: throw new NotImplementedException();
			}
		}
	}
}
