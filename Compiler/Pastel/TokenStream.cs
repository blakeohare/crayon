using System.Collections.Generic;
using System.Linq;

namespace Pastel
{
    internal class TokenStream
    {
        private Token[] tokens;
        private int index;
        private int length;

        public TokenStream(IList<Token> tokens)
        {
            this.index = 0;
            this.tokens = tokens.ToArray();
            this.length = this.tokens.Length;
        }

        public int SnapshotState()
        {
            return this.index;
        }

        public void RevertState(int index)
        {
            this.index = index;
        }

        public bool IsNext(string token)
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index].Value == token;
            }
            return false;
        }

        public Token Peek()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index];
            }
            return null;
        }

        public Token Pop()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index++];
            }
            throw new EofException();
        }

        public string PeekValue()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index].Value;
            }
            return null;
        }

        public string PeekAhead(int offset)
        {
            if (this.index + offset < this.length)
            {
                return this.tokens[this.index + offset].Value;
            }
            return null;
        }

        public bool PopIfPresent(string value)
        {
            if (this.index < this.length && this.tokens[this.index].Value == value)
            {
                this.index++;
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
                throw new ParserException(token, message);
            }
            return token;
        }

        public bool HasMore
        {
            get
            {
                return this.index < this.length;
            }
        }

        public Token PopBitShiftHackIfPresent()
        {
            string next = this.PeekValue();
            if (next == "<" || next == ">")
            {
                if (this.index + 1 < this.length)
                {
                    Token nextToken = this.tokens[this.index + 1];
                    if (nextToken.Value == next && !nextToken.HasWhitespacePrefix)
                    {
                        Token output = this.Pop();
                        this.Pop();
                        return new Token(
                            output.Value + output.Value,
                            output.FileName,
                            output.Line,
                            output.Col,
                            output.HasWhitespacePrefix);
                    }
                }
            }
            return null;
        }
    }
}
