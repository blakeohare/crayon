using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon
{
    internal class AnnotationParser
    {
        private Parser parser;
        public AnnotationParser(Parser parser)
        {
            this.parser = parser;
        }

        public Annotation ParseAnnotation(TokenStream tokens)
        {
            Token annotationToken = tokens.PopExpected("@");
            Token typeToken = tokens.Pop();
            parser.VerifyIdentifier(typeToken);
            List<Expression> args = new List<Expression>();
            if (tokens.PopIfPresent("("))
            {
                while (!tokens.PopIfPresent(")"))
                {
                    if (args.Count > 0)
                    {
                        tokens.PopExpected(",");
                    }

                    args.Add(this.parser.ExpressionParser.Parse(tokens, null));
                }
            }
            return new Annotation(annotationToken, typeToken, args);
        }
    }
}
