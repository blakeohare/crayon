using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// This code is taken from SimpleJson: https://github.com/blakeohare/SimpleJson
namespace SimpleJson
{
    // Various flags to reduce the strictness of the JSON standard.
    public enum JsonOption
    {
        // This will make the parser return null instead of throwing an exception
        FAIL_SILENTLY,

        // This will allow /* slash asterisk */ style comments.
        ALLOW_COMMENTS,

        // This will allow [lists, and, dictionaries, with, trailing, commas, ]
        ALLOW_TRAILING_COMMA,

        // This will allow both "double" and 'single' quote strings. Strict standard only allows double quotes.
        ALLOW_SINGLE_QUOTES,

        // This will allow keys of objects to not require quotes around them.
        ALLOW_NON_QUOTED_KEYS,

        // If enabled, any JSON entity can be the root of the document.
        // For example, if you have a document of lists, you can use this parser for that, even
        // though it's not technically JSON.
        ALLOW_NON_OBJECT_AS_ROOT,

        // If enabled, floats with a decimal as the first character (rather than starting with "0.") will
        // be considered valid.
        ALLOW_DECIMAL_AS_FLOAT_FIRST_CHAR,

        // This will allow objects with duplicate keys to pass the parser.
        // Later keys will overwrite values for previous identical keys.
        OVERWRITE_DUPLICATE_KEYS,

        // This will recognize the Python primitives True, False, and None and convert them into
        // true, false, and null respectively.
        ALLOW_PYTHON_PRIMITIVES,
    }

    public class JsonParser
    {
        private string text;
        private HashSet<JsonOption> options = new HashSet<JsonOption>();

        public JsonParser(string text)
        {
            this.text = text;
        }

        public JsonParser AddOption(JsonOption option)
        {
            this.options.Add(option);
            return this;
        }

        public IDictionary<string, object> ParseAsDictionary()
        {
            if (this.options.Contains(JsonOption.ALLOW_NON_OBJECT_AS_ROOT))
            {
                throw new InvalidOperationException("Cannot use ParseAsDictionary if ALLOW_NON_OBJECT_AS_ROOT option is enabled.");
            }

            return (IDictionary<string, object>)this.Parse();
        }

        public object Parse()
        {
            try
            {
                TokenStream tokens = new TokenStream(this.text, this.options.Contains(JsonOption.ALLOW_COMMENTS));
                if (tokens.Length == 0)
                {
                    throw new JsonParserException("Content is blank.");
                }

                object value = this.ParseThing(tokens);
                if (!this.options.Contains(JsonOption.ALLOW_NON_OBJECT_AS_ROOT) &&
                    !(value is IDictionary<string, object>))
                {
                    tokens.Reset();
                    tokens.PopExpected("{");
                }
                return value;
            }
            catch (JsonParserException jpe)
            {
                if (this.options.Contains(JsonOption.FAIL_SILENTLY))
                {
                    return null;
                }
                throw jpe;
            }
        }

        private object ParseThing(TokenStream tokens)
        {
            string tokenValue = tokens.PeekValue();
            switch (tokenValue)
            {
                case "true":
                case "false":
                    tokens.Pop();
                    return tokenValue == "true";
                case "null":
                    tokens.Pop();
                    return null;

                default:
                    if (this.options.Contains(JsonOption.ALLOW_PYTHON_PRIMITIVES))
                    {
                        switch (tokenValue)
                        {
                            case "True":
                            case "False":
                                tokens.Pop();
                                return tokenValue == "True";
                            case "None":
                                tokens.Pop();
                                return null;
                        }
                    }

                    char c = tokenValue[0];
                    switch (c)
                    {
                        case '"':
                        case '\'':
                            return this.ParseString(tokens.Pop(), false);

                        case '{': return this.ParseDictionary(tokens);
                        case '[': return this.ParseList(tokens);

                        default: break;
                    }

                    if (c >= '0' && c <= '9')
                    {
                        if (tokenValue.Contains('.'))
                        {
                            return this.ParseFloat(tokens.Pop());
                        }
                        return this.ParseInteger(tokens.Pop());
                    }

                    if (c == '.')
                    {
                        return this.ParseFloat(tokens.Pop());
                    }

                    throw tokens.Pop().Throw("Unrecognized/unexpected value: '" + tokenValue + "'");
            }

            throw tokens.Peek().Throw("Unexpected token: '" + tokens.PeekValue() + "'.");
        }

        private static readonly IFormatProvider EN_US =
            System.Globalization.CultureInfo.GetCultureInfo("en-us");
        private static readonly System.Globalization.NumberStyles DOUBLE_FLAG =
            (System.Globalization.NumberStyles)(
            (int)System.Globalization.NumberStyles.AllowDecimalPoint |
            (int)System.Globalization.NumberStyles.AllowLeadingSign |
            (int)System.Globalization.NumberStyles.Float |
            (int)System.Globalization.NumberStyles.Integer);

        private int ParseInteger(Token token)
        {
            int value;
            if (int.TryParse(token.Value, out value))
            {
                return value;
            }
            throw token.Throw("Unrecognized integer value: '" + value + "'");
        }

        private double ParseFloat(Token token)
        {
            if (token.Value[0] == '.' && !this.options.Contains(JsonOption.ALLOW_DECIMAL_AS_FLOAT_FIRST_CHAR))
            {
                token.Throw("Invalid format for decimal. Use ALLOW_DECIMAL_AS_FLOAT_FIRST_CHAR or add '0' prefix before decimal.");
            }

            double value;
            if (double.TryParse(token.Value, DOUBLE_FLAG, EN_US, out value))
            {
                return value;
            }
            throw token.Throw("Unrecognized float value: '" + token.Value + "'");
        }

        private string ParseString(Token token, bool allowUnquoted)
        {
            string text = token.Value;
            char stringType = text[0];
            if (stringType == '"' || stringType == '\'')
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int length = text.Length - 1;
                char c;
                for (int i = 1; i < length; ++i)
                {
                    c = text[i];
                    if (c == '\\')
                    {
                        if (i + 1 < length)
                        {
                            switch (text[i + 1])
                            {
                                case '\\': sb.Append('\\'); break;
                                case 'r': sb.Append('\r'); break;
                                case 'n': sb.Append('\n'); break;
                                case '0': sb.Append('\0'); break;
                                case 't': sb.Append('\t'); break;
                                case '\'': sb.Append('\''); break;
                                case '"': sb.Append('"'); break;
                                case 'u': throw new NotImplementedException(); // TODO: implement unicode.
                                default:
                                    throw token.Throw("Unrecognized escape sequence: '\\" + text[i + 1] + "'.");
                            }
                            ++i; // used 2 chars, skip the next.
                        }
                        else
                        {
                            throw token.Throw("Trailing backslash in string.");
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }

            if (allowUnquoted)
            {
                return text;
            }

            throw token.Throw("Found unquoted string: " + text);
        }

        private object[] ParseList(TokenStream tokens)
        {
            List<object> output = new List<object>();
            tokens.PopExpected("[");

            bool isNextAllowed = true;
            Token lastComma = null;
            while (!tokens.PopIfPresent("]"))
            {
                if (!isNextAllowed) tokens.PopExpected(","); // will throw
                output.Add(this.ParseThing(tokens));
                lastComma = tokens.Peek();
                isNextAllowed = tokens.PopIfPresent(",");
            }

            if (isNextAllowed && output.Count > 0 && !this.options.Contains(JsonOption.ALLOW_TRAILING_COMMA))
            {
                throw lastComma.Throw("Unexpected comma at end of list. Remove or use ALLOW_TRAILING_COMMA option.");
            }

            return output.ToArray();
        }

        private ReadOnlyOrderedDictionary ParseDictionary(TokenStream tokens)
        {
            List<string> keys = new List<string>();
            Dictionary<string, object> lookup = new Dictionary<string, object>();
            tokens.PopExpected("{");
            bool isNextAllowed = true;
            bool allowUnquotedKeys = this.options.Contains(JsonOption.ALLOW_NON_QUOTED_KEYS);
            Token lastComma = null;
            while (!tokens.PopIfPresent("}"))
            {
                if (!isNextAllowed) tokens.PopExpected(","); // will throw
                Token next = tokens.Peek();
                string key = this.ParseString(tokens.Pop(), allowUnquotedKeys);
                tokens.PopExpected(":");
                object value = this.ParseThing(tokens);

                if (lookup.ContainsKey(key))
                {
                    if (this.options.Contains(JsonOption.OVERWRITE_DUPLICATE_KEYS))
                    {
                        lookup[key] = value;
                    }
                    else
                    {
                        next.Throw("Duplicate object key declaration: '" + key + "'.");
                    }
                }
                else
                {
                    keys.Add(key);
                    lookup[key] = value;
                }

                lastComma = tokens.Peek();
                isNextAllowed = tokens.PopIfPresent(",");
            }

            if (isNextAllowed && keys.Count > 0 && !this.options.Contains(JsonOption.ALLOW_TRAILING_COMMA))
            {
                throw lastComma.Throw("Unexpected comma at end of object. Remove or use ALLOW_TRAILING_COMMA option.");
            }

            return new ReadOnlyOrderedDictionary(lookup, keys);
        }

        private class Token
        {
            public string Value { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }

            public override string ToString()
            {
                return "TOKEN: " + this.Value;
            }

            public Token(string value, int line, int column)
            {
                this.Value = value;
                this.Line = line;
                this.Column = column;
            }

            // Throws an exception so that the calling site can use the throw statement to prevent e.g. no-return-statement errors.
            public Exception Throw(string message)
            {
                throw new JsonParserException("Line " + this.Line + ", Column " + this.Column + ": " + message);
            }
        }

        private class TokenStream
        {
            private int index;
            private int length;
            private Token[] tokens;

            public TokenStream(string text, bool allowComments)
            {
                this.tokens = Init(text, allowComments);
                this.index = 0;
                this.length = this.tokens.Length;
            }

            public int Length { get { return this.length; } }
            public void Reset() { this.index = 0; }
            public Token Last { get { return this.tokens[this.length - 1]; } }

            private enum TokenizerState
            {
                NONE,
                COMMENT,
                STRING,
                WORD,
            }

            private static Token[] Init(string text, bool allowComments)
            {
                List<Token> tokens = new List<Token>();

                // This is a hack to prevent the need to "close" token/state types.
                // If the state is not NONE at the end, then either a comment or string was left open.
                text += "\n";

                int length = text.Length;
                TokenizerState state = TokenizerState.NONE;
                List<string> tokenBuilder = new List<string>();
                int tokenStart = 0;

                int[] lines = new int[length];
                int[] columns = new int[length];
                int line = 1;
                int column = 1;
                for (int i = 0; i < length; ++i)
                {
                    lines[i] = line;
                    columns[i] = column++;
                    if (text[i] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                }

                char stringType = '\0';
                char c;
                for (int i = 0; i < length; ++i)
                {
                    c = text[i];
                    switch (state)
                    {
                        case TokenizerState.NONE:
                            switch (c)
                            {
                                case ' ':
                                case '\r':
                                case '\n':
                                case '\t':
                                    // skip whitespace
                                    break;

                                case '"':
                                case '\'':
                                    stringType = c;
                                    tokenStart = i;
                                    state = TokenizerState.STRING;
                                    break;

                                case '/':
                                    if (allowComments && i + 1 < length && text[i + 1] == '*')
                                    {
                                        state = TokenizerState.COMMENT;
                                        i++; // do not allow /*/ as a self-closing comment.
                                    }
                                    else
                                    {
                                        // Go ahead and add as a token and let the parser throw the exception.
                                        tokens.Add(new Token("/", lines[i], columns[i]));
                                    }
                                    break;

                                default:
                                    // numbers, unquoted strings, nulls, booleans
                                    if ((c >= '0' && c <= '9') ||
                                        (c >= 'a' && c <= 'z') ||
                                        (c >= 'A' && c <= 'Z') ||
                                        c == '_' ||
                                        c == '.' ||
                                        c == '-')
                                    {
                                        state = TokenizerState.WORD;
                                        tokenStart = i;
                                    }
                                    else
                                    {
                                        // Either a JSON syntax character (like a bracket, comma, or colon) or an error.
                                        tokens.Add(new Token(c.ToString(), lines[i], columns[i]));
                                    }
                                    break;
                            }
                            break;

                        case TokenizerState.COMMENT:
                            if (c == '*')
                            {
                                if (i + 1 < length && text[i + 1] == '/')
                                {
                                    ++i; // skip the slash
                                    state = TokenizerState.NONE;
                                }
                            }
                            break;

                        case TokenizerState.STRING:
                            if (c == stringType)
                            {
                                tokens.Add(new Token(text.Substring(tokenStart, i - tokenStart + 1), lines[tokenStart], columns[tokenStart]));
                                state = TokenizerState.NONE;
                            }
                            else if (c == '\\')
                            {
                                i++;
                            }
                            break;

                        case TokenizerState.WORD:
                            if ((c >= '0' && c <= '9') ||
                                (c >= 'a' && c <= 'z') ||
                                (c >= 'A' && c <= 'Z') ||
                                c == '_' ||
                                c == '.' ||
                                c == '-')
                            {
                                // continue the word. Make sure this expression always matches the one above it in the NONE state.
                            }
                            else
                            {
                                tokens.Add(new Token(text.Substring(tokenStart, i - tokenStart), lines[tokenStart], columns[tokenStart]));
                                --i;
                                state = TokenizerState.NONE;
                            }
                            break;
                    }
                }

                if (state != TokenizerState.NONE)
                {
                    if (state == TokenizerState.COMMENT)
                    {
                        throw new JsonParserException("Unexpected EOF detected. A comment seems to be left unclosed.");
                    }
                    throw new JsonParserException("Unexpected EOF detected. A string seems to be left unclosed.");
                }

                return tokens.ToArray();
            }

            public Token Pop()
            {
                if (this.index == this.length) throw new InvalidOperationException(); // this should not happen.
                return this.tokens[this.index++];
            }

            public Token Peek()
            {
                if (this.index < this.length)
                {
                    return this.tokens[this.index];
                }

                throw this.Last.Throw("Unexpected EOF");
            }

            public string PeekValue()
            {
                if (this.index < this.length)
                {
                    return this.tokens[this.index].Value;
                }

                throw this.Last.Throw("Unexpected EOF");
            }

            public void PopExpected(string value)
            {
                if (this.index < this.length)
                {
                    Token next = this.tokens[this.index];
                    if (next.Value != value)
                    {
                        next.Throw("Unexpected token. Expected '" + value + "' but found '" + next.Value + "' instead.");
                    }
                    this.index++;
                }
                else
                {
                    this.Last.Throw("Unexpected EOF. Expected '" + value + "'.");
                }
            }

            public bool PopIfPresent(string value)
            {
                if (this.index < this.length)
                {
                    if (this.tokens[this.index].Value == value)
                    {
                        this.index++;
                        return true;
                    }
                }
                return false;
            }
        }

        public class JsonParserException : Exception
        {
            public JsonParserException(string message) : base(message) { }
        }

        private class ReadOnlyOrderedDictionary : IDictionary<string, object>
        {
            private Dictionary<string, object> lookup = new Dictionary<string, object>();
            private string[] keyOrder;

            public ReadOnlyOrderedDictionary(Dictionary<string, object> lookup, List<string> keyOrder)
            {
                this.lookup = lookup;
                this.keyOrder = keyOrder.ToArray();
            }

            public object this[string key]
            {
                get { return this.lookup[key]; }
                set { throw new NotImplementedException(); }
            }

            public int Count { get { return this.keyOrder.Length; } }
            public bool IsReadOnly { get { return true; } }
            public ICollection<string> Keys { get { return new List<string>(this.keyOrder); } }
            public ICollection<object> Values { get { return new List<object>(this.keyOrder.Select(key => this.lookup[key])); } }
            public bool Contains(KeyValuePair<string, object> item) { return this.lookup.Contains(item); }
            public bool ContainsKey(string key) { return this.lookup.ContainsKey(key); }
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { return this.lookup.GetEnumerator(); }
            public bool TryGetValue(string key, out object value) { return this.lookup.TryGetValue(key, out value); }
            IEnumerator IEnumerable.GetEnumerator() { return this.lookup.GetEnumerator(); }
            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                Dictionary<string, KeyValuePair<string, object>> orderedKeyValuePairs = new Dictionary<string, KeyValuePair<string, object>>();
                foreach (KeyValuePair<string, object> kvp in this.lookup)
                {
                    orderedKeyValuePairs.Add(kvp.Key, kvp);
                }
                foreach (string key in this.keyOrder)
                {
                    array[arrayIndex++] = orderedKeyValuePairs[key];
                }
            }

            public void Add(KeyValuePair<string, object> item) { throw new NotImplementedException(); }
            public void Add(string key, object value) { throw new NotImplementedException(); }
            public void Clear() { throw new NotImplementedException(); }
            public bool Remove(KeyValuePair<string, object> item) { throw new NotImplementedException(); }
            public bool Remove(string key) { throw new NotImplementedException(); }
        }
    }

    public class JsonLookup
    {
        private IDictionary<string, object> root;
        public JsonLookup(IDictionary<string, object> root)
        {
            this.root = root;
        }

        public string GetAsString(string path, string defaultValue) { return GetAsString(path) ?? defaultValue; }
        public string GetAsString(string path) { return this.Get(path) as string; }
        public int GetAsInteger(string path, int defaultValue) { object value = this.Get(path); return value == null ? defaultValue : (int)value; }
        public double GetAsInteger(string path) { return GetAsInteger(path, 0); }
        public double GetAsDouble(string path, double defaultValue) { object value = this.Get(path); return value == null ? defaultValue : (double)value; }
        public double GetAsDouble(string path) { return GetAsDouble(path, 0); }
        public bool GetAsBoolean(string path, bool defaultValue) { object value = this.Get(path); return value == null ? defaultValue : (bool)value; }
        public bool GetAsBoolean(string path) { return GetAsBoolean(path, false); }
        public object[] GetAsList(string path) { return (Get(path) as object[]) ?? new object[0]; }
        public IDictionary<string, object> GetAsLookup(string path) { return (Get(path) as IDictionary<string, object>) ?? new Dictionary<string, object>(); }

        public object Get(string path)
        {
            // TODO: add support for list indexing as well
            string[] steps = path.Split('.');
            IDictionary<string, object> current = this.root;
            for (int i = 0; i < steps.Length; ++i)
            {
                if (current.ContainsKey(steps[i]))
                {
                    if (i == steps.Length - 1)
                    {
                        return current[steps[i]];
                    }
                    else if (current[steps[i]] is IDictionary<string, object>)
                    {
                        current = (IDictionary<string, object>)current[steps[i]];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
    }
}
