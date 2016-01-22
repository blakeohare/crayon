using System;
using System.Collections.Generic;
using System.Text;

namespace Crayon
{
	internal static class ByteCodeEncoder
	{
		// the encoded byte code begins with the word CRAYON followed by a @ char.
		// The word crayon serves as a version stamp. For example, in a later version of byte code, CRAYON can be
		// swapped out with CRAYON2.
		// The first @ will always be the terminator of the version ID.

		// Following the @ will be an encoded integer indicating how many rows are in the byte code.
		// After that is each encoded row.
		public static String Encode(ByteBuffer buffer)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("CRAYON@");

			int[][] code = buffer.ToIntList().ToArray();
			string[] stringArgs = buffer.ToStringList().ToArray();
			int size = code.Length;

			sb.Append(EncodeInteger(size));

			for (int i = 0; i < size; ++i)
			{
				int[] row = code[i];
				string stringArg = stringArgs[i];
				EncodeRow(sb, row, stringArg);
			}
			return sb.ToString();
		}

		// If a row begins with a !, &, or * that means there are 1, 2, or 3 integer args (respectively) followed by a string arg
		// Otherwise...
		// - The first part is an encoded number indicating how many integers there are
		// - This is followed by that many encoded integers
		// - If there was a ~ prefix before the arg, then that means there is a string arg after the integer args.
		private static void EncodeRow(StringBuilder sb, int[] row, string stringArg)
		{
			int argCount = row.Length;

			if (stringArg == null)
			{
				sb.Append(EncodeInteger(argCount));
			}
			else
			{
				if (argCount >= 1 && argCount <= 3)
				{
					if (argCount == 1) sb.Append("!");
					else if (argCount == 2) sb.Append("&");
					else sb.Append("*");
				}
				else
				{
					sb.Append("~");
					sb.Append(EncodeInteger(argCount));
				}
			}

			for (int j = 0; j < argCount; ++j)
			{
				sb.Append(EncodeInteger(row[j]));
			}

			if (stringArg != null)
			{
				EncodeString(sb, stringArg);
			}
		}

		// Each character is encoded
		// A character can be followed by a @ followed by an encoded integer 
		// indicating how many times to repeat the last character.
		// If a % appears, that indicates the end of the string.
		private static void EncodeString(StringBuilder sb, string value)
		{
			List<char> characters = new List<char>();
			List<int> counts = new List<int>();
			int size = 0;
			foreach (char c in value)
			{
				if (size > 0 && characters[size - 1] == c)
				{
					counts[size - 1]++;
				}
				else
				{
					characters.Add(c);
					counts.Add(1);
					size++;
				}
			}

			for (int i = 0; i < size; ++i)
			{
				string encodedChar = EncodeChar(characters[i]);
				int count = counts[i];
				sb.Append(encodedChar);

				if (count > 1)
				{
					if (encodedChar.Length == 1 && count == 2)
					{
						sb.Append(encodedChar);
					}
					else
					{
						sb.Append("@");
						sb.Append(EncodeInteger(count - 1));
					}
				}
			}
			sb.Append("%");
		}

		private static readonly HashSet<char> SAFE_CHARS = new HashSet<char>((
			// notable exclusions: interesting whitespace, backslashes, both quote types, and the @, `, %, and ~ chars.
			"0123456789" +
			"abcdefghijklmnopqrstuvwxyz" +
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
			"_ !#$^&*()_+=-:;[]{}|<>,./?").ToCharArray());

		// an encoded character is just the character if it's on the list above. If it isn't and can fit in a two digit base-62 number,
		// it will appear as a ~ followed by the two digits.
		// If it is larger, it appears as a ` followed by an encoded integer.
		private static string EncodeChar(char c)
		{
			if (SAFE_CHARS.Contains(c)) return "" + c;

			if (c >= 0 && c < 62 * 62)
			{
				return "~" + BASE62[c / 62] + BASE62[c % 62];
			}

			return "`" + EncodeInteger((int)c);
		}

		// if the first char is a %, then there will be some characters followed by another %. This is a simple toString representation of the number.
		// if the first char is @, then there are 2 base 62 digits
		// if the first char is a #, then there are 3 base 62 digits
		// if the first char is a ^, then there is 1 base 62 digit number and it is negative
		// if the first char is a number or letter, that is the base 62 number
		private static readonly char[] BASE62 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
		private static string EncodeInteger(int n)
		{
			if (n >= 0)
			{
				if (n < 62)
				{
					return "" + BASE62[n];
				}
				else if (n < 62 * 62)
				{
					int b = n % 62;
					int a = n / 62;
					return "@" + BASE62[a] + BASE62[b];
				}
				else if (n < 62 * 62 * 62)
				{
					int c = n % 62;
					int b = (n / 62 % 62);
					int a = n / 62 / 62;
					return "#" + BASE62[a] + BASE62[b] + BASE62[c];
				}
			}
			else
			{
				if (-n < 62)
				{
					return "^" + BASE62[-n];
				}
			}

			return "%" + n + "%";
		}
	}
}
