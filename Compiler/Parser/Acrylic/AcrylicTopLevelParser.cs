using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser.Acrylic
{
    internal class AcrylicTopLevelParser : AbstractTopLevelParser
    {
        public AcrylicTopLevelParser(ParserContext parser)
            : base(parser)
        { }

        private static readonly HashSet<string> MODIFIER_STRING_VALUES = new HashSet<string>()
        {
            "public",
            "private",
            "internal",
            "protected",
            "abstract",
            "static",
            "final",
            "override",
        };

        internal override ModifierCollection ParseModifiers(TokenStream tokens)
        {
            List<Token> modifierTokens = new List<Token>();
            while (MODIFIER_STRING_VALUES.Contains(tokens.PeekValue()))
            {
                modifierTokens.Add(tokens.Pop());
            }
            return new ModifierCollection(modifierTokens);
        }

        protected override void ParseClassMember(TokenStream tokens, FileScope fileScope, ClassDefinition classDef, IList<FunctionDefinition> methodsOut, IList<FieldDefinition> fieldsOut)
        {
            AnnotationCollection annotations = this.parser.AnnotationParser.ParseAnnotations(tokens);
            ModifierCollection modifiers_IGNORED = this.ParseModifiers(tokens);

            if (tokens.IsNext(this.parser.Keywords.CONSTRUCTOR))
            {
                if (modifiers_IGNORED.HasStatic)
                {
                    if (classDef.StaticConstructor != null)
                    {
                        throw new ParserException(tokens.Pop(), "Multiple static constructors are not allowed.");
                    }
                    classDef.StaticConstructor = this.ParseConstructor(tokens, classDef, annotations);
                }
                else
                {
                    if (classDef.Constructor != null)
                    {
                        throw this.parser.GenerateParseError(
                            ErrorMessages.CLASS_CANNOT_HAVE_MULTIPLE_CONSTRUCTORS,
                            tokens.Pop());
                    }
                    classDef.Constructor = this.ParseConstructor(tokens, classDef, annotations);
                }
            }
            else if (tokens.IsNext(this.parser.Keywords.CLASS))
            {
                throw new ParserException(tokens.Pop(), "Nested classes are not currently supported.");
            }
            else
            {
                // Parsing the type and then throwing it away is a little wasteful, but feels less weird than parsing
                // the type here and passing it into ParseFunction()/ParseField(). ParseX() should ParseX from the start.
                TokenStream.StreamState fieldOrFunctionStart = tokens.RecordState();
                AType fieldOrFunctionType = this.parser.TypeParser.TryParse(tokens);
                Token tokenAfterName = fieldOrFunctionType != null ? tokens.PeekAhead(1) : null;
                tokens.RestoreState(fieldOrFunctionStart);

                if (tokenAfterName == null) tokens.PopExpected("}"); // intentionally induce error

                switch (tokenAfterName.Value)
                {
                    case "=":
                    case ";":
                        fieldsOut.Add(this.ParseField(tokens, classDef, annotations));
                        break;
                    case "(":
                        methodsOut.Add(this.ParseFunction(tokens, classDef, fileScope, annotations));
                        break;
                    default:
                        tokens.PopExpected("}"); // intentionally induce error
                        break;
                }
            }

            TODO.CheckForUnusedAnnotations();
        }
    }
}
