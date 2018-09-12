using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Crayon
{
    internal class CrayonTopLevelParser : AbstractTopLevelParser
    {
        public CrayonTopLevelParser(ParserContext parser)
            : base(parser)
        { }

        protected override void ParseClassMember(
            TokenStream tokens,
            FileScope fileScope,
            ClassDefinition classDef,
            IList<FunctionDefinition> methodsOut,
            IList<FieldDefinition> fieldsOut)
        {
            AnnotationCollection annotations = this.parser.AnnotationParser.ParseAnnotations(tokens);
            
            if (tokens.IsNext(this.parser.Keywords.FUNCTION) ||
                tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.FUNCTION))
            {
                methodsOut.Add(this.ParseFunction(tokens, classDef, fileScope, annotations));
            }
            else if (tokens.IsNext(this.parser.Keywords.CONSTRUCTOR))
            {
                if (classDef.Constructor != null)
                {
                    throw this.parser.GenerateParseError(
                        ErrorMessages.CLASS_CANNOT_HAVE_MULTIPLE_CONSTRUCTORS,
                        tokens.Pop());
                }

                classDef.Constructor = this.ParseConstructor(tokens, classDef, annotations);
            }
            else if (tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.CONSTRUCTOR))
            {
                tokens.Pop(); // static token
                if (classDef.StaticConstructor != null)
                {
                    throw new ParserException(tokens.Pop(), "Multiple static constructors are not allowed.");
                }

                classDef.StaticConstructor = this.ParseConstructor(tokens, classDef, annotations);
            }
            else if (tokens.IsNext(this.parser.Keywords.FIELD) ||
                tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.FIELD))
            {
                fieldsOut.Add(this.ParseField(tokens, classDef, annotations));
            }
            else if (tokens.IsNext(this.parser.Keywords.CLASS))
            {
                throw new ParserException(tokens.Pop(), "Nested classes are not currently supported.");
            }
            else
            {
                tokens.PopExpected("}");
            }

            TODO.CheckForUnusedAnnotations();
        }

    }
}
