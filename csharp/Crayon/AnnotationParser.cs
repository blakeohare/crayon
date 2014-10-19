using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
	internal class AnnotationParser
	{
		public static Annotation ParseAnnotation(TokenStream tokens)
		{
			Token annotationToken = tokens.PopExpected("@");
			Token typeToken = tokens.Pop();
			Parser.VerifyIdentifier(typeToken);
			List<Expression> args = new List<Expression>();
			if (tokens.PopIfPresent("("))
			{
				while (!tokens.PopIfPresent(")"))
				{
					if (args.Count > 0)
					{
						tokens.PopExpected(",");
					}

					args.Add(ExpressionParser.Parse(tokens));
				}
			}
			return new Annotation(annotationToken, typeToken, args);
		}
	}
}
