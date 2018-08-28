namespace Parser
{
    public class TokenStream
    {
        private readonly FileScope file;
        private readonly Token[] tokens;
        private int index;
        private readonly int length;

        public TokenStream(FileScope file)
        {
            this.file = file;
            this.index = 0;
            this.tokens = Tokenizer.Tokenize(file);
            this.length = this.tokens.Length;
        }

        public void EnsureNotEof()
        {
            if (!this.HasMore) this.ThrowEofException();
        }

        // returns an exception so that you can throw this function call in situations where you want
        // the compiler to think the codepath terminates
        public System.Exception ThrowEofException()
        {
            throw ParserException.ThrowEofException(this.file.Name);
        }

        private bool SafeHasMore()
        {
            return this.index < this.length;
        }

        public bool IsNext(string token)
        {
            return this.PeekValue() == token;
        }

        public bool AreNext(string token1, string token2)
        {
            return
                this.index + 1 < this.length &&
                this.tokens[this.index].Value == token1 &&
                this.tokens[this.index + 1].Value == token2;
        }

        public Token Peek()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index];
            }
            throw ThrowEofException();
        }

        public Token Pop()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index++];
            }
            throw ThrowEofException();
        }

        public string PeekValue()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index].Value;
            }
            throw ThrowEofException();
        }

        public string PeekValue(int offset)
        {
            if (this.index + offset < this.length)
            {
                return this.tokens[this.index + offset].Value;
            }
            throw ThrowEofException();
        }

        public string PopValue()
        {
            return this.Pop().Value;
        }

        public bool PopIfPresent(string value)
        {
            if (this.index < this.length && this.tokens[this.index].Value == value)
            {
                this.Pop();
                return true;
            }
            return false;
        }

        public Token PopExpected(string value)
        {
            Token token = this.Pop();
            if (token.Value != value)
            {
                string message = "Unexpected token. Expected: '" + value + "' but found '" + token.Value + "'.";
                if (value == ";" && this.index > 1 && this.tokens[this.index - 2].Line < this.tokens[this.index - 1].Line)
                {
                    message += " Is this previous line missing a semicolon at the end?";
                }
                throw new ParserException(token, message);
            }
            return token;
        }

        public bool HasMore { get { return this.index < this.length; } }

        public bool NextHasNoWhitespacePrefix
        {
            get
            {
                return this.index < this.length && this.Peek().HasWhitespacePrefix;
            }
        }
    }
}
