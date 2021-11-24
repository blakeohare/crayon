using Common;
using Parser.Localization;
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

        protected override void ParseClassMember(
            TokenStream tokens,
            FileScope fileScope,
            ClassDefinition classDef,
            IList<FunctionDefinition> methodsOut,
            IList<FieldDefinition> fieldsOut,
            IList<PropertyDefinition> propertiesOut)
        {
            AnnotationCollection annotations = this.parser.AnnotationParser.ParseAnnotations(tokens);
            ModifierCollection modifiers = ModifierCollection.Parse(tokens);

            if (tokens.IsNext(this.parser.Keywords.CONSTRUCTOR))
            {
                if (modifiers.HasStatic)
                {
                    if (classDef.StaticConstructor != null)
                    {
                        throw new ParserException(tokens.Pop(), "Multiple static constructors are not allowed.");
                    }
                    classDef.StaticConstructor = this.ParseConstructor(tokens, classDef, modifiers, annotations);
                }
                else
                {
                    if (classDef.Constructor != null)
                    {
                        throw this.parser.GenerateParseError(
                            ErrorMessages.CLASS_CANNOT_HAVE_MULTIPLE_CONSTRUCTORS,
                            tokens.Pop());
                    }
                    classDef.Constructor = this.ParseConstructor(tokens, classDef, modifiers, annotations);
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
                        fieldsOut.Add(this.ParseField(tokens, classDef, modifiers, annotations));
                        break;
                    case "(":
                        methodsOut.Add(this.ParseFunction(tokens, classDef, fileScope, modifiers, annotations));
                        break;
                    case "{":
                        // TODO(acrylic): properties should be implemented in Acrylic, just not yet.
                        tokens.PopExpected("}"); // intentionally induce error
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
            ModifierCollection modifiers,
            AnnotationCollection annotations)
        {
            AType fieldType = this.parser.TypeParser.Parse(tokens);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            FieldDefinition fd = new FieldDefinition(modifiers.FirstToken, fieldType, nameToken, owner, modifiers, annotations);
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
            ModifierCollection modifiers,
            AnnotationCollection annotations)
        {
            AType returnType = this.parser.TypeParser.Parse(tokens);
            Token firstToken = modifiers.FirstToken ?? returnType.FirstToken;
            Token functionNameToken = tokens.Pop();
            this.parser.VerifyIdentifier(functionNameToken);

            FunctionDefinition fd = new FunctionDefinition(firstToken, returnType, nullableOwner, functionNameToken, modifiers, annotations, fileScope);

            tokens.PopExpected("(");
            List<AType> argTypes = new List<AType>();
            List<Token> argNames = new List<Token>();
            List<Expression> defaultValues = new List<Expression>();
            this.ParseArgumentListDeclaration(tokens, fd, argTypes, argNames, defaultValues);

            fd.ArgTypes = argTypes.ToArray();
            fd.ArgNames = argNames.ToArray();
            fd.DefaultValues = defaultValues.ToArray();
            fd.FinalizeArguments();

            IList<Executable> code = this.parser.ExecutableParser.ParseBlock(tokens, true, fd);

            fd.Code = code.ToArray();

            return fd;
        }

        protected PropertyDefinition ParseProperty(
            TokenStream tokens,
            ClassDefinition classDef,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations)
        {
            AType propertyType = this.parser.TypeParser.Parse(tokens);
            tokens.EnsureNotEof();
            Token propertyName = tokens.PopIfWord();
            if (propertyName == null) throw new ParserException(tokens.Peek(), "Expected property name.");
            tokens.PopExpected("{");
            PropertyMember getter = null;
            PropertyMember setter = null;
            PropertyDefinition property = new PropertyDefinition(propertyType.FirstToken, classDef, fileScope, modifiers);
            while (!tokens.PopIfPresent("}"))
            {
                PropertyMember member = this.ParsePropertyMember(tokens, property);
                if (member.IsGetter)
                {
                    if (getter != null) throw new ParserException(member.FirstToken, "Property has multiple getters");
                    getter = member;
                }
                else if (member.IsSetter)
                {
                    if (setter != null) throw new ParserException(member.FirstToken, "Property has multiple setters");
                    setter = member;
                }
            }

            property.Getter = getter;
            property.Setter = setter;

            return property;
        }

        private PropertyMember ParsePropertyMember(TokenStream tokens, PropertyDefinition property)
        {
            tokens.EnsureNotEof();
            ModifierCollection modifiers = ModifierCollection.Parse(tokens);
            tokens.EnsureNotEof();
            Token getOrSet = tokens.Peek();
            if (!tokens.PopIfPresent("get") && !tokens.PopIfPresent("set")) tokens.PopExpected("get");
            PropertyMember member = new PropertyMember(getOrSet, property, modifiers);
            if (!tokens.PopIfPresent(";"))
            {
                IList<Executable> code = this.parser.ExecutableParser.ParseBlock(tokens, true, member);
                member.Code = code.ToArray();
            }
            return member;
        }

        protected override ConstDefinition ParseConst(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations)
        {
            Token constToken = tokens.PopExpected(this.parser.Keywords.CONST);
            AType type = this.parser.TypeParser.Parse(tokens);
            Token nameToken = tokens.Pop();
            ConstDefinition constStatement = new ConstDefinition(constToken, type, nameToken, owner, fileScope, modifiers, annotations);
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
                return this.ParseFunction(tokens, owner as TopLevelEntity, fileScope, modifiers, annotations);
            }

            tokens.RestoreState(tss);
            return null;
        }
    }
}
