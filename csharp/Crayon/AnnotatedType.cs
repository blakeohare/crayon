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
		public Token Token { get; private set; }

		public AnnotatedType(Crayon.ParseTree.Annotation annotation)
		{
			if (annotation.Type != "type") throw new ParserException(annotation.FirstToken, "Expected type annotation.");
			this.Initialize(annotation.FirstToken, Tokenizer.Tokenize("proxy token stream", annotation.GetSingleArgAsString(null), -1, false));
		}

		public AnnotatedType(Token stringToken, TokenStream proxyTokenStream)
		{
			this.Initialize(stringToken, proxyTokenStream);
		}

		private void Initialize(Token annotationToken, TokenStream proxyTokenStream)
		{
			this.Token = annotationToken;

			Token token = proxyTokenStream.Pop();
			if (!Parser.IsValidIdentifier(token.Value)) throw new ParserException(annotationToken, "Invalid type");

			this.Name = token.Value;

			while (proxyTokenStream.PopIfPresent("."))
			{
				this.Name += ".";
				this.Name += proxyTokenStream.PopValue();
			}

			List<AnnotatedType> generics = new List<AnnotatedType>();
			if (proxyTokenStream.PopIfPresent("<"))
			{
				while (proxyTokenStream.HasMore && !proxyTokenStream.IsNext(">"))
				{
					if (generics.Count > 0 && !proxyTokenStream.PopIfPresent(","))
					{
						throw new ParserException(annotationToken, "Expected comma in generic list");
					}

					AnnotatedType genericItem = new AnnotatedType(annotationToken, proxyTokenStream);
					generics.Add(genericItem);
				}

				if (!proxyTokenStream.PopIfPresent(">"))
				{
					throw new ParserException(annotationToken, "Unclosed generic bracket.");
				}
			}

			this.Generics = generics.ToArray();
		}
	}
}
