using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal class TokenStream
	{
		private int index;
		private int length;
		private Token[] tokens;

		public TokenStream(IList<Token> tokens)
		{
			this.index = 0;
			this.tokens = tokens.ToArray();
			this.length = tokens.Count;
		}

		public void Reset()
		{
			this.index = 0;
		}

		public bool IsNext(string token)
		{
			return this.PeekValue() == token;
		}

		public bool AreNext(string token1, string token2)
		{
			if (this.IsNext(token1))
			{
				if (this.index + 1 < this.length)
				{
					return this.tokens[this.index + 1].Value == token2;
				}
			}
			return false;
		}

		public Token Peek()
		{
			if (this.index < this.length)
			{
				return this.tokens[this.index];
			}

			throw new EofException();
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
			return this.Peek().Value;
		}

		public string PeekValue(int skipAhead)
		{
			int index = this.index + skipAhead;
			if (index < this.length)
			{
				return this.tokens[index].Value;
			}
			return null;
		}

		public string PopValue()
		{
			return this.Pop().Value;
		}

		public bool PopIfPresent(string value)
		{
			if (this.index < this.length && this.tokens[this.index].Value == value)
			{
				this.index += 1;
				return true;
			}
			return false;
		}

		public Token PopExpected(string value)
		{
			Token token = this.Pop();
			if (token.Value != value)
			{
				throw new ParserException(token, " Unexpected token. Expected: '" + value + "' but found '" + token.Value + "'");
			}
			return token;
		}

		public bool HasMore
		{
			get { return this.index < this.length; }
		}

		public bool NextHasNoWhitespacePrefix
		{
			get
			{
				if (this.index < this.length)
				{
					Token token = this.tokens[this.index];
					return token.HasWhitespacePrefix;
				}
				return false;
			}
		}

		// For inline imports
		public void InsertTokens(TokenStream tokens)
		{
			Token[] otherTokens = tokens.tokens;
			List<Token> theseTokens = new List<Token>(this.tokens);
			theseTokens.InsertRange(this.index, otherTokens);
			this.tokens = theseTokens.ToArray();
			this.length = this.tokens.Length;
		}
	}
}
