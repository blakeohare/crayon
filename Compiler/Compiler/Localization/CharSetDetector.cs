using System.Collections.Generic;

namespace Parser.Localization
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

        private static readonly string DIACRITIC_CHARS =
            // This is not an exhaustive list and should not be defined this way.
            // TODO: Find the correct way to identify these characters programmatically.
            "āēīūčģķļņšžўõáíóúýáéíóúýöáâãàçéêíóôõúâêîôûŵŷäëïöüẅÿàèìòùẁỳáéíóúẃý";
        private static readonly HashSet<char> DIACRITICS = new HashSet<char>(
            (DIACRITIC_CHARS + DIACRITIC_CHARS.ToUpperInvariant()).ToCharArray());

        private static CharType GetTypeImpl(char c)
        {
            if (c >= '0' && c <= '9') return CharType.NUMBER;
            if (c >= 'a' && c <= 'z') return CharType.LETTER;
            if (c >= 'A' && c <= 'Z') return CharType.LETTER;

            int code = (int)c;
            if (code >= 0x3040 && code <= 0x309f) return CharType.HIRAGANA;
            if (code >= 0x30a0 && code <= 0x30ff) return CharType.KATAKANA;
            if (code >= 0x4e00 && code <= 0x9fbf) return CharType.KANJI;

            if (DIACRITICS.Contains(c))
            {
                return CharType.ACCENTED;
            }

            return CharType.UNKNOWN;
        }
    }
}
