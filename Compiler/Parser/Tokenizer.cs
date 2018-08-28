using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public static class Tokenizer
    {
        private static readonly HashSet<string> TWO_CHAR_TOKENS = new HashSet<string>() {
            "==", "!=", "<=", ">=",
            "&&", "||",
            "++", "--",
            "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
            "<<", ">>",
            "**",
            "??",
            "=>",
        };
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

        public static Token[] Tokenize(FileScope file)
        {
            bool useMultiCharTokens = true;
            Localization.Locale locale = file.CompilationScope.Locale;
            string code = file.Content;
            code += '\n';
            code += '\0';

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

            string commentType = null;
            string stringType = null;
            StringBuilder stringToken = null;
            string normalToken = null;
            int stringStart = 0;
            int normalStart = 0;

            bool previousIsWhitespace = false;
            bool tokenStartHasPreviousWhitespace = false;

            string c2;
            int length = code.Length;
            for (int i = 0; i < length; ++i)
            {
                c = code[i];
                c2 = (i >= (length - 1)) ? "" : ("" + c + code[i + 1]);

                if (c == '\0' && i == length - 1)
                {
                    // Indicates the end of the stream. Throw an exception in cases where you left something lingering.
                    if (commentType == "*")
                    {
                        ParserException.ThrowEofExceptionWithSuggestion(file.Name, "This file contains an unclosed comment somewhere.");
                    }

                    if (stringType != null)
                    {
                        Token suspiciousToken = null;
                        foreach (Token suspiciousCheck in tokens)
                        {
                            c = suspiciousCheck.Value[0];
                            if (c == '"' || c == '\'')
                            {
                                if (suspiciousCheck.Value.Contains("\n"))
                                {
                                    suspiciousToken = suspiciousCheck;
                                    break;
                                }
                            }
                        }

                        string unclosedStringError = "There is an unclosed string somewhere in this file.";
                        if (suspiciousToken != null)
                        {
                            unclosedStringError += " Line " + (suspiciousToken.Line + 1) + " is suspicious.";
                        }
                        else if (stringStart != 0)
                        {
                            unclosedStringError += " Line " + (lineByIndex[stringStart] + 1) + " is suspicious.";
                        }
                        ParserException.ThrowEofExceptionWithSuggestion(file.Name, unclosedStringError);
                    }
                }

                if (commentType == "/")
                {
                    if (c == '\n')
                    {
                        commentType = null;
                    }
                    previousIsWhitespace = true;
                }
                else if (commentType == "*")
                {
                    if (c2 == "*/")
                    {
                        commentType = null;
                        ++i;
                    }
                    previousIsWhitespace = true;
                }
                else if (stringType != null)
                {
                    if (c == '\\')
                    {
                        stringToken.Append(c2);
                        ++i;
                    }
                    else if (c == stringType[0])
                    {
                        stringToken.Append(c);
                        stringType = null;
                        tokens.Add(new Token(stringToken.ToString(), TokenType.STRING, file, lineByIndex[stringStart], colByIndex[stringStart], tokenStartHasPreviousWhitespace));
                    }
                    else
                    {
                        stringToken.Append(c);
                    }
                    previousIsWhitespace = false;
                }
                else if (normalToken != null)
                {
                    if (IsIdentifierChar(c))
                    {
                        normalToken += c;
                    }
                    else
                    {
                        TokenType type = TokenType.WORD;
                        if (!locale.Keywords.IsValidVariable(normalToken))
                        {
                            type = TokenType.KEYWORD;
                        }
                        tokens.Add(new Token(normalToken, type, file, lineByIndex[normalStart], colByIndex[normalStart], tokenStartHasPreviousWhitespace));
                        --i;
                        normalToken = null;
                    }
                    previousIsWhitespace = false;
                }
                else if (useMultiCharTokens && TWO_CHAR_TOKENS.Contains(c2))
                {
                    tokens.Add(new Token(c2, TokenType.PUNCTUATION, file, lineByIndex[i], colByIndex[i], previousIsWhitespace));
                    ++i;
                    previousIsWhitespace = false;
                }
                else if (WHITESPACE.Contains(c))
                {
                    previousIsWhitespace = true;
                }
                else if (c == '"')
                {
                    stringType = "\"";
                    stringToken = new StringBuilder("" + stringType);
                    stringStart = i;
                    tokenStartHasPreviousWhitespace = previousIsWhitespace;
                    previousIsWhitespace = false;
                }
                else if (c == '\'')
                {
                    stringType = "'";
                    stringToken = new StringBuilder("" + stringType);
                    stringStart = i;
                    tokenStartHasPreviousWhitespace = previousIsWhitespace;
                    previousIsWhitespace = false;
                }
                else if (IsIdentifierChar(c))
                {
                    normalToken = "" + c;
                    normalStart = i;
                    tokenStartHasPreviousWhitespace = previousIsWhitespace;
                    previousIsWhitespace = false;
                }
                else if (c2 == "//")
                {
                    commentType = "/";
                    i += 1;
                }
                else if (c2 == "/*")
                {
                    commentType = "*";
                    i += 1;
                }
                else
                {
                    tokens.Add(new Token("" + c, TokenType.PUNCTUATION, file, lineByIndex[i], colByIndex[i], previousIsWhitespace));
                    previousIsWhitespace = false;
                }
            }
            tokens.RemoveAt(tokens.Count - 1);

            return tokens.ToArray();
        }
    }
}
