using Builder.Localization;
using Builder.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Builder
{
    internal abstract class AbstractTopLevelParser
    {
        protected ParserContext parser;

        public bool HasTypes { get; protected set; }

        public AbstractTopLevelParser(ParserContext parser, bool hasTypes)
        {
            this.parser = parser;
            this.HasTypes = hasTypes;
        }

        internal virtual ImportStatement ParseImport(TokenStream tokens, FileScope fileScope)
        {
            Token importToken = tokens.PopExpected(parser.Keywords.IMPORT);

            List<string> importPathBuilder = new List<string>();
            while (!tokens.PopIfPresent(";"))
            {
                if (importPathBuilder.Count > 0)
                {
                    tokens.PopExpected(".");
                }

                Token pathToken = tokens.Pop();
                parser.VerifyIdentifier(pathToken);
                importPathBuilder.Add(pathToken.Value);
            }
            string importPath = string.Join(".", importPathBuilder);

            return new ImportStatement(importToken, importPath, fileScope);
        }

        internal virtual TopLevelEntity Parse(
            TokenStream tokens,
            TopLevelEntity owner,
            FileScope fileScope)
        {
            AnnotationCollection annotations = this.parser.AnnotationParser.ParseAnnotations(tokens);

            ModifierCollection modifiers = ModifierCollection.Parse(tokens);

            string value = tokens.PeekValue();

            if (value == this.parser.Keywords.IMPORT)
            {
                throw this.parser.GenerateParseError(
                    ErrorMessages.ALL_IMPORTS_MUST_OCCUR_AT_BEGINNING_OF_FILE,
                    tokens.Pop());
            }

            // TODO: check for annotations that aren't used.
            // https://github.com/blakeohare/crayon/issues/305

            if (value == this.parser.Keywords.NAMESPACE) return this.ParseNamespace(tokens, owner, fileScope, annotations);
            if (value == this.parser.Keywords.CONST) return this.ParseConst(tokens, owner, fileScope, modifiers, annotations);
            if (value == this.parser.Keywords.FUNCTION) return this.ParseFunction(tokens, owner, fileScope, modifiers, annotations);
            if (value == this.parser.Keywords.CLASS) return this.ParseClassDefinition(tokens, owner, fileScope, modifiers, annotations);
            if (value == this.parser.Keywords.ENUM) return this.ParseEnumDefinition(tokens, owner, fileScope, modifiers, annotations);
            if (value == this.parser.Keywords.CONSTRUCTOR && owner is ClassDefinition) return this.ParseConstructor(tokens, (ClassDefinition)owner, modifiers, annotations);

            FunctionDefinition nullableFunctionDef = this.MaybeParseFunctionDefinition(tokens, owner, fileScope, annotations, modifiers);
            if (nullableFunctionDef != null)
            {
                return nullableFunctionDef;
            }

            tokens.EnsureNotEof();

            Token token = tokens.Peek();
            throw ParserException.ThrowException(
                this.parser.CurrentLocale,
                ErrorMessages.UNEXPECTED_TOKEN_NO_SPECIFIC_EXPECTATIONS,
                token,
                token.Value);
        }

        protected virtual FunctionDefinition MaybeParseFunctionDefinition(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            AnnotationCollection annotations,
            ModifierCollection modifiers)
        {
            return null;
        }

        protected virtual ConstructorDefinition ParseConstructor(
            TokenStream tokens,
            ClassDefinition owner,
            ModifierCollection modifiers,
            AnnotationCollection annotations)
        {
            Token constructorToken = tokens.PopExpected(this.parser.Keywords.CONSTRUCTOR);
            ConstructorDefinition ctor = new ConstructorDefinition(constructorToken, modifiers, annotations, owner);
            tokens.PopExpected("(");

            List<AType> argTypes = new List<AType>();
            List<Token> argNames = new List<Token>();
            List<Expression> argValues = new List<Expression>();

            this.ParseArgumentListDeclaration(tokens, ctor, argTypes, argNames, argValues);

            List<Expression> baseArgs = new List<Expression>();
            Token baseToken = null;
            if (tokens.PopIfPresent(":"))
            {
                baseToken = tokens.PopExpected(this.parser.Keywords.BASE);
                tokens.PopExpected("(");
                while (!tokens.PopIfPresent(")"))
                {
                    if (baseArgs.Count > 0)
                    {
                        tokens.PopExpected(",");
                    }

                    baseArgs.Add(this.parser.ExpressionParser.Parse(tokens, ctor));
                }
            }

            IList<Executable> code = this.parser.ExecutableParser.ParseBlock(tokens, true, ctor);

            ctor.SetArgs(argNames, argValues, argTypes);
            ctor.SetBaseArgs(baseArgs);
            ctor.SetCode(code);
            ctor.BaseToken = baseToken;
            return ctor;
        }

        protected void ParseArgumentListDeclaration(
            TokenStream tokens,
            Node owner,
            IList<AType> argTypesOut,
            IList<Token> argNamesOut,
            IList<Expression> argDefaultValuesOut)
        {
            bool optionalArgFound = false;
            while (!tokens.PopIfPresent(")"))
            {
                if (argNamesOut.Count > 0)
                {
                    tokens.PopExpected(",");
                }

                AType argType = this.HasTypes
                    ? this.parser.TypeParser.Parse(tokens)
                    : AType.Any(tokens.Peek());

                Token argName = tokens.Pop();
                this.parser.VerifyIdentifier(argName);
                Expression defaultValue = null;
                if (tokens.PopIfPresent("="))
                {
                    defaultValue = this.parser.ExpressionParser.Parse(tokens, owner);
                    optionalArgFound = true;
                }
                else if (optionalArgFound)
                {
                    throw this.parser.GenerateParseError(
                        ErrorMessages.OPTIONAL_ARGUMENT_WAS_NOT_AT_END_OF_ARGUMENT_LIST,
                        argName);
                }

                argTypesOut.Add(argType);
                argNamesOut.Add(argName);
                argDefaultValuesOut.Add(defaultValue);
            }
        }

        protected abstract ConstDefinition ParseConst(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations);

        protected virtual EnumDefinition ParseEnumDefinition(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations)
        {
            Token enumToken = tokens.PopExpected(this.parser.Keywords.ENUM);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            string name = nameToken.Value;
            EnumDefinition ed = new EnumDefinition(enumToken, nameToken, owner, fileScope, modifiers, annotations);

            tokens.PopExpected("{");
            bool nextForbidden = false;
            List<Token> items = new List<Token>();
            List<Expression> values = new List<Expression>();
            while (!tokens.PopIfPresent("}"))
            {
                if (nextForbidden) tokens.PopExpected("}"); // crash

                Token enumItem = tokens.Pop();
                this.parser.VerifyIdentifier(enumItem);
                if (tokens.PopIfPresent("="))
                {
                    values.Add(this.parser.ExpressionParser.Parse(tokens, ed));
                }
                else
                {
                    values.Add(null);
                }
                nextForbidden = !tokens.PopIfPresent(",");
                items.Add(enumItem);
            }

            ed.SetItems(items, values);
            return ed;
        }

        protected virtual ClassDefinition ParseClassDefinition(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection classAnnotations)
        {
            Token classToken = tokens.PopExpected(this.parser.Keywords.CLASS);
            Token classNameToken = tokens.Pop();
            if (classNameToken.Type != TokenType.WORD)
            {
                throw new ParserException(classNameToken, "This is not a valid class name.");
            }
            List<Token> baseClassTokens = new List<Token>();
            List<string> baseClassStrings = new List<string>();
            if (tokens.PopIfPresent(":"))
            {
                if (baseClassTokens.Count > 0)
                {
                    tokens.PopExpected(",");
                }

                Token baseClassToken = tokens.Pop();
                string baseClassName = baseClassToken.Value;

                this.parser.VerifyIdentifier(baseClassToken);
                while (tokens.PopIfPresent("."))
                {
                    Token baseClassTokenNext = tokens.Pop();
                    this.parser.VerifyIdentifier(baseClassTokenNext);
                    baseClassName += "." + baseClassTokenNext.Value;
                }

                baseClassTokens.Add(baseClassToken);
                baseClassStrings.Add(baseClassName);
            }

            ClassDefinition cd = new ClassDefinition(
                classToken,
                classNameToken,
                baseClassTokens,
                baseClassStrings,
                owner,
                fileScope,
                modifiers,
                classAnnotations,
                this.parser);

            tokens.PopExpected("{");
            List<FunctionDefinition> methods = new List<FunctionDefinition>();
            List<FieldDefinition> fields = new List<FieldDefinition>();
            List<PropertyDefinition> properties = new List<PropertyDefinition>();

            while (!tokens.PopIfPresent("}"))
            {
                this.ParseClassMember(tokens, fileScope, cd, methods, fields, properties);
            }

            cd.Methods = methods.ToArray();
            cd.Fields = fields.ToArray();

            if (cd.Constructor == null)
            {
                // This should be empty if there is no base class, or just pass along the base class' args if there is.
                cd.Constructor = new ConstructorDefinition(
                    cd,
                    ModifierCollection.EMPTY,
                    new AnnotationCollection(parser));
                if (cd.BaseClassTokens.Length > 0)
                {
                    cd.Constructor.BaseToken = cd.FirstToken;
                    cd.Constructor.SetBaseArgs(new Expression[0]);
                }
            }

            return cd;
        }

        protected abstract void ParseClassMember(
            TokenStream tokens,
            FileScope fileScope,
            ClassDefinition classDef,
            IList<FunctionDefinition> methodsOut,
            IList<FieldDefinition> fieldsOut,
            IList<PropertyDefinition> propertiesOut);

        protected abstract FieldDefinition ParseField(
            TokenStream tokens,
            ClassDefinition owner,
            ModifierCollection modifiers,
            AnnotationCollection annotations);

        protected virtual Namespace ParseNamespace(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            AnnotationCollection annotations)
        {
            Token namespaceToken = tokens.PopExpected(this.parser.Keywords.NAMESPACE);
            Token first = tokens.Pop();
            this.parser.VerifyIdentifier(first);
            List<Token> namespacePieces = new List<Token>() { first };
            string namespaceBuilder = first.Value;
            parser.RegisterNamespace(namespaceBuilder);
            while (tokens.PopIfPresent("."))
            {
                Token nsToken = tokens.Pop();
                this.parser.VerifyIdentifier(nsToken);
                namespacePieces.Add(nsToken);
                namespaceBuilder += "." + nsToken.Value;
                parser.RegisterNamespace(namespaceBuilder);
            }

            string name = string.Join(".", namespacePieces.Select<Token, string>(t => t.Value));

            Namespace namespaceInstance = new Namespace(namespaceToken, name, owner, fileScope, ModifierCollection.EMPTY, annotations);

            tokens.PopExpected("{");
            List<TopLevelEntity> namespaceMembers = new List<TopLevelEntity>();
            while (!tokens.PopIfPresent("}"))
            {
                TopLevelEntity executable = this.Parse(tokens, namespaceInstance, fileScope);
                if (executable is FunctionDefinition ||
                    executable is ClassDefinition ||
                    executable is EnumDefinition ||
                    executable is ConstDefinition ||
                    executable is Namespace)
                {
                    namespaceMembers.Add(executable);
                }
                else
                {
                    throw new ParserException(executable, "Only function, class, and nested namespace declarations may exist as direct members of a namespace.");
                }
            }

            namespaceInstance.Code = namespaceMembers.ToArray();

            return namespaceInstance;
        }

        protected abstract FunctionDefinition ParseFunction(
            TokenStream tokens,
            TopLevelEntity nullableOwner,
            FileScope fileScope,
            ModifierCollection modifiers,
            AnnotationCollection annotations);
    }
}
