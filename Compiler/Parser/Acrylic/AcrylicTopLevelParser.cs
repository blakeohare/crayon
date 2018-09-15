using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Acrylic
{
    internal class AcrylicTopLevelParser : AbstractTopLevelParser
    {
        public AcrylicTopLevelParser(ParserContext parser)
            : base(parser, true)
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
                        fieldsOut.Add(this.ParseField(tokens, classDef, annotations, modifiers_IGNORED));
                        break;
                    case "(":
                        methodsOut.Add(this.ParseFunction(tokens, classDef, fileScope, annotations, modifiers_IGNORED));
                        break;
                    default:
                        tokens.PopExpected("}"); // intentionally induce error
                        break;
                }
            }

            TODO.CheckForUnusedAnnotations();
        }

        protected override FieldDefinition ParseField(
            TokenStream tokens,
            ClassDefinition owner,
            AnnotationCollection annotations,
            ModifierCollection modifiers)
        {
            AType fieldType = this.parser.TypeParser.Parse(tokens);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            FieldDefinition fd = new FieldDefinition(modifiers.FirstToken, fieldType, nameToken, owner, modifiers.HasStatic, annotations);
            if (tokens.PopIfPresent("="))
            {
                fd.DefaultValue = this.parser.ExpressionParser.Parse(tokens, fd);
            }
            tokens.PopExpected(";");
            return fd;
        }

        protected override FunctionDefinition ParseFunction(
            TokenStream tokens,
            TopLevelEntity nullableOwner,
            FileScope fileScope,
            AnnotationCollection annotations,
            ModifierCollection modifiers)
        {
            AType returnType = this.parser.TypeParser.Parse(tokens);
            Token firstToken = modifiers.FirstToken ?? returnType.FirstToken;
            Token functionNameToken = tokens.Pop();
            this.parser.VerifyIdentifier(functionNameToken);

            FunctionDefinition fd = new FunctionDefinition(firstToken, nullableOwner, modifiers.HasStatic, functionNameToken, annotations, fileScope);

            tokens.PopExpected("(");
            List<AType> argTypes = new List<AType>();
            List<Token> argNames = new List<Token>();
            List<Expression> defaultValues = new List<Expression>();
            this.ParseArgumentListDeclaration(tokens, fd, argTypes, argNames, defaultValues);

            IList<Executable> code = this.parser.ExecutableParser.ParseBlock(tokens, true, fd);

            fd.ArgTypes = argTypes.ToArray();
            fd.ArgNames = argNames.ToArray();
            fd.DefaultValues = defaultValues.ToArray();
            fd.Code = code.ToArray();

            return fd;
        }

        protected override ConstDefinition ParseConst(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            AnnotationCollection annotations)
        {
            Token constToken = tokens.PopExpected(this.parser.Keywords.CONST);
            AType type = this.parser.TypeParser.Parse(tokens);
            Token nameToken = tokens.Pop();
            ConstDefinition constStatement = new ConstDefinition(constToken, nameToken, owner, fileScope, annotations);
            this.parser.VerifyIdentifier(nameToken);
            tokens.PopExpected("=");
            constStatement.Expression = this.parser.ExpressionParser.Parse(tokens, constStatement);
            tokens.PopExpected(";");

            return constStatement;
        }

        protected override FunctionDefinition MaybeParseFunctionDefinition(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            AnnotationCollection annotations,
            ModifierCollection modifiers)
        {
            TokenStream.StreamState tss = tokens.RecordState();
            AType returnType = this.parser.TypeParser.TryParse(tokens);
            if (returnType == null) return null;
            Token functionName = tokens.PopIfWord();
            if (functionName == null)
            {
                tokens.RestoreState(tss);
                return null;
            }

            if (tokens.IsNext("("))
            {
                tokens.RestoreState(tss);
                return this.ParseFunction(tokens, owner as TopLevelEntity, fileScope, annotations, modifiers);
            }

            tokens.RestoreState(tss);
            return null;
        }
    }
}
