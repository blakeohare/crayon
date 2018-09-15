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
            : base(parser, false)
        { }

        protected override void ParseClassMember(
            TokenStream tokens,
            FileScope fileScope,
            ClassDefinition classDef,
            IList<FunctionDefinition> methodsOut,
            IList<FieldDefinition> fieldsOut)
        {
            AnnotationCollection annotations = this.parser.AnnotationParser.ParseAnnotations(tokens);
            ModifierCollection modifiers = ModifierCollection.EMPTY;

            if (tokens.IsNext(this.parser.Keywords.FUNCTION) ||
                tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.FUNCTION))
            {
                methodsOut.Add(this.ParseFunction(tokens, classDef, fileScope, annotations, modifiers));
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
                fieldsOut.Add(this.ParseField(tokens, classDef, annotations, ModifierCollection.EMPTY));
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

        // TODO: don't manually parse static here, just get it from modifiers
        protected override FieldDefinition ParseField(
            TokenStream tokens,
            ClassDefinition owner,
            AnnotationCollection annotations,
            ModifierCollection modifiers)
        {
            bool isStatic = tokens.PopIfPresent(this.parser.Keywords.STATIC);
            Token fieldToken = tokens.PopExpected(this.parser.Keywords.FIELD);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            FieldDefinition fd = new FieldDefinition(fieldToken, null, nameToken, owner, isStatic, annotations);
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
            bool isStatic =
                nullableOwner != null &&
                nullableOwner is ClassDefinition &&
                tokens.PopIfPresent(this.parser.Keywords.STATIC);

            Token functionToken = tokens.PopExpected(this.parser.Keywords.FUNCTION);

            Token functionNameToken = tokens.Pop();
            this.parser.VerifyIdentifier(functionNameToken);

            FunctionDefinition fd = new FunctionDefinition(functionToken, nullableOwner, isStatic, functionNameToken, annotations, fileScope);

            tokens.PopExpected("(");
            List<Token> argNames = new List<Token>();
            List<Expression> defaultValues = new List<Expression>();
            List<AType> argTypesIgnored = new List<AType>();
            this.ParseArgumentListDeclaration(tokens, fd, argTypesIgnored, argNames, defaultValues);

            IList<Executable> code = this.parser.ExecutableParser.ParseBlock(tokens, true, fd);

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
            Token nameToken = tokens.Pop();
            ConstDefinition constStatement = new ConstDefinition(constToken, nameToken, owner, fileScope, annotations);
            this.parser.VerifyIdentifier(nameToken);
            tokens.PopExpected("=");
            constStatement.Expression = this.parser.ExpressionParser.Parse(tokens, constStatement);
            tokens.PopExpected(";");

            return constStatement;
        }

    }
}
