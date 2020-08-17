using System;
using System.Collections.Generic;

namespace Common
{
    public static class KeyValuePairParser
    {
        // Returns key value pairs given a string of the following format:
        // key1=value1, key2=value2,key3="values 3, 4, and 5" etc.
        // returns null upon error.
        // supports both single and double-quoted strings.
        public static Dictionary<string, string> Parse(string raw)
        {
            SimpleTokenStream stream = new SimpleTokenStream(raw);
            Dictionary<string, string> output = new Dictionary<string, string>();
            try
            {
                while (stream.HasMore)
                {
                    if (output.Count > 0)
                    {
                        stream.SkipWhitespace();
                        stream.PopExpected(',');
                    }

                    stream.SkipWhitespace();
                    string key = stream.PopPossiblyQuotedToken('=');
                    stream.SkipWhitespace();
                    stream.PopExpected('=');
                    stream.SkipWhitespace();
                    string value = stream.PopPossiblyQuotedToken(',');
                    output[key] = value;

                    // skip whitespace before .HasMore check.
                    stream.SkipWhitespace();
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return output;
        }

        private class SimpleTokenStream
        {
            private int index;
            private int length;
            private char[] chars;

            public SimpleTokenStream(string value)
            {
                this.chars = value.ToCharArray();
                this.index = 0;
                this.length = this.chars.Length;
            }

            public void SkipWhitespace()
            {
                while (this.index < this.length)
                {
                    switch (this.chars[this.index])
                    {
                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                            this.index++;
                            break;
                        default:
                            return;
                    }
                }
            }

            public void PopExpected(char c)
            {
                if (this.index < this.length && this.chars[this.index] == c)
                {
                    this.index++;
                }
                else
                {
                    throw new InvalidOperationException("Expected '" + c + "'");
                }
            }

            public string PopPossiblyQuotedToken(char specialEndChar)
            {
                this.SkipWhitespace();
                if (this.index < this.length)
                {
                    char c = this.chars[this.index++];
                    if (c == '"' || c == '\\')
                    {
                        string value = this.ReadTill(c, false).Trim();
                        this.index++;
                        return value;
                    }
                    else
                    {
                        this.index--;
                    }
                    if (c == specialEndChar)
                    {
                        return "";
                    }
                    return this.ReadTill(specialEndChar, true).Trim();
                }
                throw new InvalidOperationException("Unexpected EOF");
            }

            public bool HasMore { get { return this.index < this.length; } }

            private string ReadTill(char endChar, bool eofOk)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                char c;
                while (this.index < this.length)
                {
                    c = this.chars[this.index];
                    if (c == endChar)
                    {
                        return sb.ToString();
                    }
                    sb.Append(c);
                    this.index++;
                }
                if (eofOk) return sb.ToString();
                throw new InvalidOperationException("Unexpected EOF");
            }
        }
    }
}
