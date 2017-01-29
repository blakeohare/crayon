using System.Collections.Generic;
using System.Text;

namespace Pastel
{
    internal static class Tokenizer
    {
        private static readonly HashSet<string> TWO_CHAR_TOKENS = new HashSet<string>(
            "++ -- == != <= >= && || += -= *= /= %= &= |= ^=".Split(' '));
        private static readonly HashSet<char> IDENTIFIER_CHARS = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_".ToCharArray());
        private static readonly HashSet<char> WHITESPACE = new HashSet<char>(" \r\n\t".ToCharArray());

        public static Token[] Tokenize(string filename, string code)
        {
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
                    if (commentType == "*") throw new ParserException(new Token("EOF", filename, lineByIndex[lineByIndex.Length - 1], colByIndex[colByIndex.Length - 1], false), "This file contains an unclosed comment somewhere.");
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

                        string unclosedStringError = "This file contains an unclosed string somewhere.";
                        if (suspiciousToken != null)
                        {
                            unclosedStringError += " Line " + (suspiciousToken.Line + 1) + " is suspicious.";
                        }
                        throw new ParserException(new Token("EOF", filename, lineByIndex[lineByIndex.Length - 1], colByIndex[colByIndex.Length - 1], false), unclosedStringError);
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
                        tokens.Add(new Token(stringToken.ToString(), filename, lineByIndex[stringStart], colByIndex[stringStart], tokenStartHasPreviousWhitespace));
                    }
                    else
                    {
                        stringToken.Append(c);
                    }
                    previousIsWhitespace = false;
                }
                else if (normalToken != null)
                {
                    if (IDENTIFIER_CHARS.Contains(c))
                    {
                        normalToken += c;
                    }
                    else
                    {
                        tokens.Add(new Token(normalToken, filename, lineByIndex[normalStart], colByIndex[normalStart], tokenStartHasPreviousWhitespace));
                        --i;
                        normalToken = null;
                    }
                    previousIsWhitespace = false;
                }
                else if (TWO_CHAR_TOKENS.Contains(c2))
                {
                    tokens.Add(new Token(c2, filename, lineByIndex[i], colByIndex[i], previousIsWhitespace));
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
                else if (IDENTIFIER_CHARS.Contains(c))
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
                    tokens.Add(new Token("" + c, filename, lineByIndex[i], colByIndex[i], previousIsWhitespace));
                    previousIsWhitespace = false;
                }
            }
            tokens.RemoveAt(tokens.Count - 1);

            return tokens.ToArray();
        }
    }
}
