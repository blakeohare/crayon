using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localization
{
    public enum CharType
    {
        UNKNOWN,

        BASIC_SET,
        LETTER,
        ACCENTED,
        NUMBER,
        HIRAGANA,
        KATAKANA,
        KANJI,
    }

    public class CharSetDetector
    {
        private static Dictionary<char, CharType> types = new Dictionary<char, CharType>();

        public static CharType GetCharType(char c)
        {
            CharType ct;
            if (!types.TryGetValue(c, out ct))
            {
                ct = GetTypeImpl(c);
                types[c] = ct;
            }
            return ct;
        }

        private static CharType GetTypeImpl(char c)
        {
            if (c >= '0' && c <= '9') return CharType.NUMBER;
            if (c >= 'a' && c <= 'z') return CharType.LETTER;
            if (c >= 'A' && c <= 'Z') return CharType.LETTER;

            int code = (int)c;
            if (code >= 0x3040 && code <= 0x309f) return CharType.HIRAGANA;
            if (code >= 0x30a0 && code <= 0x30ff) return CharType.KATAKANA;
            if (code >= 0x4e00 && code <= 0x9fbf) return CharType.KANJI;

            // TODO: diacritic characters

            return CharType.UNKNOWN;
        }
    }
}
