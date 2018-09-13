using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    public class TokenStream
    {
        public class StreamState
        {
            internal int Value { get; set; }
        }

        private readonly FileScope file;
        private readonly InnerTokenStream innerStream;

        public TokenStream(FileScope file)
        {
            this.file = file;
            this.innerStream = new InnerTokenStream(Tokenizer.Tokenize(file));
        }

        public StreamState RecordState()
        {
            return new StreamState() { Value = this.innerStream.Index };
        }

        public void RestoreState(StreamState state)
        {
            this.innerStream.Index = state.Value;
        }

        // This stream consolidates specific sequences of punctuation into a single token.
        private class InnerTokenStream
        {
            private static readonly HashSet<string> MULTI_CHAR_TOKENS = new HashSet<string>() {
                "==", "!=", "<=", ">=",
                "&&", "||",
                "++", "--",
                "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "<<<=",
                "<<", ">>", "<<<",
                "**",
                "??",
                "=>",
            };
            private static readonly Dictionary<char, char[][]> LIST_OF_MULTICHAR_TOKENS_BY_INITIAL_CHAR;

            static InnerTokenStream()
            {
                Dictionary<char, List<string>> mCharLookup = new Dictionary<char, List<string>>();
                foreach (string token in MULTI_CHAR_TOKENS)
                {
                    char c = token[0];
                    if (!mCharLookup.ContainsKey(c))
                    {
                        mCharLookup[c] = new List<string>();
                    }
                    mCharLookup[c].Add(token);
                }
                LIST_OF_MULTICHAR_TOKENS_BY_INITIAL_CHAR = new Dictionary<char, char[][]>();
                foreach (char c in mCharLookup.Keys)
                {
                    LIST_OF_MULTICHAR_TOKENS_BY_INITIAL_CHAR[c] = mCharLookup[c]
                        .OrderBy(s => -s.Length) // e.g. Get >>> to trigger before >>
                        .Select(s => s.Substring(1)) // since this list is found from the first character, we only want to test the characters after
                        .Select(s => s.ToCharArray()) // easier to check
                        .ToArray();
                }
            }

            private int index = 0;
            public int Index
            {
                get { return this.index; }
                set { this.index = value; this.nextToken = null; }
            }
            public int Length { get; set; }
            public Token[] Tokens { get; set; }
            private Token nextToken = null;
            private bool useMulticharTokens = true;

            public InnerTokenStream(Token[] tokens)
            {
                this.Length = tokens.Length;
                this.Tokens = tokens;
            }

            public void SetMulticharTokenMode(bool value)
            {
                this.useMulticharTokens = value;
                this.nextToken = null;
            }

            public Token Peek()
            {
                if (this.nextToken == null && this.Index < this.Length)
                {
                    // TODO: Put the number consolidation logic currently in Tokenizer into this section.
                    // Hypothetically if I want to use a double-dot construct like 0..10 to indicate some sort
                    // of range, it would be easier to detect that here, whereas .10 would get tokenized as a number currently.

                    Token next = this.Tokens[this.Index];
                    if (!this.useMulticharTokens || next.Type != TokenType.PUNCTUATION)
                    {
                        this.nextToken = next;
                        return next;
                    }

                    if (next.Value.Length != 1) throw new System.Exception(); // this should not happen
                    char c = next.Value[0];
                    char[][] mcharTokens;

                    if (LIST_OF_MULTICHAR_TOKENS_BY_INITIAL_CHAR.TryGetValue(c, out mcharTokens))
                    {
                        foreach (char[] mtoken in mcharTokens)
                        {
                            bool isMatch = true;
                            for (int i = 0; i < mtoken.Length; ++i)
                            {
                                int index = this.Index + i + 1;
                                if (index < this.Length)
                                {
                                    Token tokenCheck = this.Tokens[index];
                                    if (tokenCheck.Type != TokenType.PUNCTUATION ||
                                        tokenCheck.Value[0] != mtoken[i])
                                    {
                                        isMatch = false;
                                        break;
                                    }
                                }
                            }
                            if (isMatch)
                            {
                                this.nextToken = new Token(c + string.Join("", mtoken), TokenType.PUNCTUATION, next.File, next.Line, next.Col);
                                this.nextToken.AggregateTokenCount = 1 + mtoken.Length;
                                return this.nextToken;
                            }
                        }
                    }

                    this.nextToken = next;
                }
                return this.nextToken;
            }

            public Token Pop()
            {
                Token token = this.Peek();
                this.Index += token.AggregateTokenCount;
                return token;
            }
        }

        public void EnsureNotEof()
        {
            if (this.innerStream.Index >= this.innerStream.Length)
            {
                this.ThrowEofException();
            }
        }

        // returns an exception so that you can throw this function call in situations where you want
        // the compiler to think the codepath terminates
        public System.Exception ThrowEofException()
        {
            throw ParserException.ThrowEofException(this.file.Name);
        }

        public bool IsNext(string token)
        {
            Token next = this.innerStream.Peek();
            return next != null && next.Value == token;
        }

        public bool AreNext(string token1, string token2)
        {
            if (!this.HasMore) return false;
            if (this.PeekValue() != token1) return false;
            int index = this.innerStream.Index;
            this.innerStream.Pop();
            bool isNext = this.HasMore && this.innerStream.Peek().Value == token2;
            this.innerStream.Index = index;
            return isNext;
        }

        public Token Peek()
        {
            Token token = this.innerStream.Peek();
            if (token == null) return null;
            return token;
        }

        public Token Pop()
        {
            Token token = this.innerStream.Pop();
            if (token == null) throw ThrowEofException();
            return token;
        }

        public string PeekValue()
        {
            Token token = this.innerStream.Peek();
            if (token == null) return null;
            return token.Value;
        }

        public Token PeekAhead(int offset)
        {
            int index = this.innerStream.Index;
            Token last = null;
            while (offset-- >= 0)
            {
                last = this.innerStream.Pop();
                if (last == null) throw ThrowEofException();
            }
            this.innerStream.Index = index;
            return last;
        }

        public string PeekValue(int offset)
        {
            return this.PeekAhead(offset).Value;
        }

        public string PopValue()
        {
            return this.Pop().Value;
        }

        public bool PopIfPresent(string value)
        {
            Token token = this.innerStream.Peek();
            if (token != null && token.Value == value)
            {
                this.innerStream.Pop();
                return true;
            }
            return false;
        }

        public Token PopIfWord()
        {
            Token token = this.innerStream.Peek();
            if (token != null && token.Type == TokenType.WORD)
            {
                return this.innerStream.Pop();
            }
            return null;
        }

        public Token PopExpected(string value)
        {
            Token token = this.innerStream.Peek();
            if (token == null)
            {
                this.ThrowEofException();
            }
            else if (token.Value != value)
            {
                string message = "Unexpected token. Expected: '" + value + "' but found '" + token.Value + "'.";
                if (value == ";" && this.innerStream.Index > 1 && this.innerStream.Tokens[this.innerStream.Index - 1].Line < token.Line)
                {
                    message += " Is this previous line missing a semicolon at the end?";
                }
                throw new ParserException(token, message);
            }
            return this.innerStream.Pop();
        }

        public bool HasMore
        {
            get
            {
                return this.innerStream.Index < this.innerStream.Length;
            }
        }
    }
}
