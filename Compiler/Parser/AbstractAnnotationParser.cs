using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    internal abstract class AbstractAnnotationParser
    {
        protected ParserContext parser;

        public AbstractAnnotationParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal virtual AnnotationCollection ParseAnnotations(TokenStream tokens)
        {
            AnnotationCollection annotationCollection = new AnnotationCollection(this.parser);
            while (tokens.IsNext("@"))
            {
                annotationCollection.Add(this.ParseAnnotation(tokens));
            }

            annotationCollection.Validate();

            return annotationCollection;
        }

        private Annotation ParseAnnotation(TokenStream tokens)
        {
            Token annotationToken = tokens.PopExpected("@");
            Token typeToken = tokens.Pop();

            // TODO: refactor this. All built-in annotations should be exempt from the VerifyIdentifier check in an extensible way.
            if (typeToken.Value != this.parser.Keywords.PRIVATE)
            {
                parser.VerifyIdentifier(typeToken);
            }

            Annotation annotation = new Annotation(annotationToken, typeToken);
            List<Expression> args = new List<Expression>();
            if (tokens.PopIfPresent("("))
            {
                while (!tokens.PopIfPresent(")"))
                {
                    if (args.Count > 0)
                    {
                        tokens.PopExpected(",");
                    }

                    args.Add(this.parser.ExpressionParser.Parse(tokens, annotation));
                }
            }
            annotation.SetArgs(args);
            return annotation;
        }
    }
}
