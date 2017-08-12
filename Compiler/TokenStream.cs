using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    internal class TokenStream
    {
        private readonly List<Token[]> tokenLayers = new List<Token[]>();
        private readonly List<int> indexes = new List<int>();
        private Token[] topTokens;
        private int topIndex;
        private int topLength;
        private bool empty;

        private string fileName;

        public TokenStream(IList<Token> tokens, string fileName)
        {
            this.topIndex = 0;
            this.topTokens = tokens.ToArray();
            this.topLength = this.topTokens.Length;

            this.indexes.Add(0);
            this.tokenLayers.Add(this.topTokens);
            this.empty = false;

            this.fileName = fileName;
        }

        // returns an exception so that you can throw this function call in situations where you want
        // the compiler to think the codepath terminates
        public System.Exception ThrowEofException()
        {
            return ParserException.ThrowEofException(this.fileName);
        }

        private Token SafePeek()
        {
            if (this.empty) throw new EofException();

            if (this.topIndex < this.topLength)
            {
                return this.topTokens[this.topIndex];
            }

            this.SafePopLayer();

            return this.SafePeek();
        }

        private Token SafePop()
        {
            Token token = this.SafePeek();
            this.topIndex++;
            return token;
        }

        private bool SafeHasMore()
        {
            if (this.empty) return false;
            if (this.topIndex < this.topLength) return true;
            this.SafePopLayer();
            return this.SafeHasMore();
        }

        private void SafePopLayer()
        {
            this.indexes.RemoveAt(this.indexes.Count - 1);
            this.tokenLayers.RemoveAt(this.indexes.Count);

            int i = this.indexes.Count - 1;
            if (i == -1)
            {
                this.empty = true;
            }
            else
            {
                this.topIndex = this.indexes[i];
                this.topTokens = this.tokenLayers[i];
                this.topLength = this.topTokens.Length;
            }
        }

        public bool IsNext(string token)
        {
            return this.PeekValue() == token;
        }

        public bool AreNext(string token1, string token2)
        {
            if (this.empty) return false;

            if (!this.IsNext(token1)) return false;

            // only check the top stack as multi-stream results are never contextually relevant.
            if (this.topIndex + 1 < this.topLength)
            {
                return this.topTokens[this.topIndex + 1].Value == token2;
            }

            return false;
        }

        public Token Peek()
        {
            return this.SafeHasMore() ? this.SafePeek() : null;
        }

        public string FlatPeekAhead(int i)
        {
            if (this.topIndex + i < this.topLength)
            {
                return this.topTokens[this.topIndex + i].Value;
            }
            return null;
        }

        public Token Pop()
        {
            return this.SafePop();
        }

        public string PeekValue()
        {
            return this.SafeHasMore() ? this.SafePeek().Value : null;
        }

        public string PeekValue(int offset)
        {
            // only check the top stack as multi-stream results are never contextually relevant.
            int index = this.topIndex + offset;
            if (index < this.topLength)
            {
                return this.topTokens[index].Value;
            }
            return null;
        }

        public string PopValue()
        {
            return this.Pop().Value;
        }

        public bool PopIfPresent(string value)
        {
            if (this.PeekValue() == value)
            {
                this.Pop();
                return true;
            }
            return false;
        }

        // This is a hack for generic types that may have multiple '>' characters in a row.
        // e.g. List<List<int>>
        // The last >> will get counted as a single bit shift token.
        // Calling this function will pop the >> token off, modify the value to > and add a single-token stream of just > to the token list stack.
        public Token PopExpectedOrPartial(string value)
        {
            if (this.IsNext(">"))
            {
                return this.Pop();
            }

            if (this.IsNext(">>"))
            {
                Token token = this.Pop();
                token = new Token(">", token.FileID, token.FileName, token.Line, token.Col, token.HasWhitespacePrefix);
                Token otherGt = new Token(">", token.FileID, token.FileName, token.Line, token.Col + 1, false);
                this.InsertTokens(new Token[] { otherGt });
                return token;
            }

            return this.PopExpected(">");
        }

        public Token PopExpected(string value)
        {
            Token token = this.Pop();
            if (token.Value != value)
            {
                string message = "Unexpected token. Expected: '" + value + "' but found '" + token.Value + "'.";
                if (value == ";" && this.topIndex > 0 && this.topTokens[this.topIndex - 2].Line < token.Line)
                {
                    message += " Is this previous line missing a semicolon at the end?";
                }
                throw new ParserException(token, message);
            }
            return token;
        }

        public bool HasMore
        {
            get
            {
                return this.SafeHasMore();
            }
        }

        public bool NextHasNoWhitespacePrefix
        {
            get
            {
                return this.SafeHasMore() && this.SafePeek().HasWhitespacePrefix;
            }
        }

        // Inline imports, implicit core import statements...
        public void InsertTokens(IList<Token> tokens)
        {
            this.indexes[this.indexes.Count - 1] = this.topIndex;
            this.topTokens = tokens.ToArray();
            this.topIndex = 0;
            this.topLength = this.topTokens.Length;
            this.tokenLayers.Add(this.topTokens);
            this.indexes.Add(0);
        }
    }
}
