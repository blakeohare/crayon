using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
	internal class AnnotatedType
	{
		public string Name { get; private set; }
		public AnnotatedType[] Generics { get; private set; }
		public Token StringToken { get; private set; }

		public AnnotatedType(Token stringToken, TokenStream proxyTokenStream)
		{
			this.StringToken = stringToken;

			Token token = proxyTokenStream.Pop();
			if (!Parser.IsValidIdentifier(token.Value)) throw new ParserException(stringToken, "Invalid type");

			this.Name = token.Value;

			List<AnnotatedType> generics = new List<AnnotatedType>();
			if (proxyTokenStream.PopIfPresent("<"))
			{
				while (proxyTokenStream.HasMore && !proxyTokenStream.IsNext(">"))
				{
					if (generics.Count > 0 && !proxyTokenStream.PopIfPresent(","))
					{
						throw new ParserException(stringToken, "Expected comma in generic list");
					}

					AnnotatedType genericItem = new AnnotatedType(stringToken, proxyTokenStream);
					generics.Add(genericItem);
				}

				if (!proxyTokenStream.PopIfPresent(">"))
				{
					throw new ParserException(stringToken, "Unclosed generic bracket.");
				}
			}

			this.Generics = generics.ToArray();
		}
	}
}
