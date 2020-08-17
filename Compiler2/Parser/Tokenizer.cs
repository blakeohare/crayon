using System.Collections.Generic;

namespace Parser
{
    internal static class Tokenizer
    {
        private static readonly HashSet<char> WHITESPACE = new HashSet<char>() { ' ', '\r', '\n', '\t' };

        private static Dictionary<char, bool> IDENTIFIER_CHARS_CACHE = new Dictionary<char, bool>();

        private static bool IsIdentifierCharImpl(char c)
        {
            switch (c)
            {
                case '$':
                case '_':
                    return true;

                default:
                    switch (Localization.CharSetDetector.GetCharType(c))
                    {
                        case Localization.CharType.ACCENTED:
                        case Localization.CharType.LETTER:
                        case Localization.CharType.NUMBER:
                        case Localization.CharType.KANJI:
                        case Localization.CharType.HIRAGANA:
                        case Localization.CharType.KATAKANA:
                            return true;
                        default:
                            return false;
                    }
            }
        }

        public static bool IsIdentifierChar(char c)
        {
            bool result;
            if (!IDENTIFIER_CHARS_CACHE.TryGetValue(c, out result))
            {
                result = IsIdentifierCharImpl(c);
                IDENTIFIER_CHARS_CACHE[c] = result;
            }
            return result;
        }


        private enum TokenMode
        {
            NORMAL,
            COMMENT,
            WORD,
            STRING,
        }

        public static Token[] Tokenize(FileScope file)
        {
            Localization.Locale locale = file.CompilationScope.Locale;
            string code = file.Content;

            // Add a newline and a dummy character at the end.
            // Set the length equal to the code with the newline but without the null terminator.
            // This makes dereferencing the index + 1 code simpler and all makes the check for the end
            // of word tokens and single-line comments easy.
            code += "\n\0";
            int length = code.Length - 1;

            int[] lineByIndex = new int[code.Length];
            int[] colByIndex = new int[code.Length];
            char c;
            int line = 0;
            int col = 0;
            for (int i = 0; i < code.Length; ++i)
            {
                c = code[i];
                lineByIndex[i] = line;
                colByIndex[i] = col;
                if (c == '\n')
                {
                    ++line;
                    col = -1;
                }

                ++col;
            }

            List<Token> tokens = new List<Token>();

            TokenMode mode = TokenMode.NORMAL;
            char modeSubtype = ' ';
            int tokenStart = 0;
            string tokenValue;
            char c2;
            bool isTokenEnd = false;
            bool stringIsRaw = false;

            for (int i = 0; i < length; ++i)
            {
                c = code[i];

                switch (mode)
                {
                    case TokenMode.COMMENT:
                        if (modeSubtype == '*')
                        {
                            if (c == '*' && code[i + 1] == '/')
                            {
                                ++i;
                                mode = TokenMode.NORMAL;
                            }
                        }
                        else
                        {
                            if (c == '\n')
                            {
                                mode = TokenMode.NORMAL;
                            }
                        }
                        break;

                    case TokenMode.NORMAL:
                        if (WHITESPACE.Contains(c))
                        {
                            // do nothing
                        }
                        else if (c == '/' && (code[i + 1] == '/' || code[i + 1] == '*'))
                        {
                            mode = TokenMode.COMMENT;
                            modeSubtype = code[++i];
                        }
                        else if (IsIdentifierChar(c))
                        {
                            tokenStart = i;
                            mode = TokenMode.WORD;
                        }
                        else if (c == '"' | c == '\'')
                        {
                            tokenStart = i;
                            mode = TokenMode.STRING;
                            modeSubtype = c;
                            stringIsRaw = tokens.Count > 0 && tokens[tokens.Count - 1].Value == "@";
                        }
                        else
                        {
                            if (c == '.')
                            {
                                c2 = code[i + 1];
                                if (c2 >= '0' && c2 <= '9')
                                {
                                    mode = TokenMode.WORD;
                                    tokenStart = i++;
                                }
                            }

                            if (mode == TokenMode.NORMAL)
                            {
                                tokens.Add(new Token(c.ToString(), TokenType.PUNCTUATION, file, lineByIndex[i], colByIndex[i]));
                            }
                        }
                        break;

                    case TokenMode.STRING:
                        if (c == modeSubtype)
                        {
                            tokenValue = code.Substring(tokenStart, i - tokenStart + 1);
                            tokens.Add(new Token(tokenValue, TokenType.STRING, file, lineByIndex[i], colByIndex[i]));
                            mode = TokenMode.NORMAL;
                        }
                        else if (!stringIsRaw && c == '\\')
                        {
                            ++i;
                        }
                        break;

                    case TokenMode.WORD:
                        isTokenEnd = false;
                        if (IsIdentifierChar(c))
                        {
                            // do nothing
                        }
                        else if (c == '.')
                        {
                            if (code[tokenStart] >= '0' && code[tokenStart] <= '9')
                            {
                                // do nothing
                            }
                            else
                            {
                                isTokenEnd = true;
                            }
                        }
                        else
                        {
                            isTokenEnd = true;
                        }

                        if (isTokenEnd)
                        {
                            tokenValue = code.Substring(tokenStart, i - tokenStart);
                            c = tokenValue[0];
                            TokenType type = TokenType.WORD;
                            if ((c >= '0' && c <= '9') || c == '.')
                            {
                                type = TokenType.NUMBER;
                            }
                            else if (!locale.Keywords.IsValidVariable(tokenValue))
                            {
                                type = TokenType.KEYWORD;
                            }
                            tokens.Add(new Token(tokenValue, type, file, lineByIndex[tokenStart], colByIndex[tokenStart]));
                            mode = TokenMode.NORMAL;
                            --i;
                        }
                        break;
                }
            }

            switch (mode)
            {
                case TokenMode.COMMENT:
                    throw new ParserException(file, "There is an unclosed comment in this file.");
                case TokenMode.STRING:
                    throw new ParserException(file, "There is an unclosed string in this file.");
                case TokenMode.WORD:
                    throw new System.InvalidOperationException();
                default:
                    break;
            }

            return tokens.ToArray();
        }
    }
}
