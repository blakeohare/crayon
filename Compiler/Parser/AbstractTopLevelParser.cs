﻿using Common;
using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    internal abstract class AbstractTopLevelParser
    {
        protected ParserContext parser;

        public AbstractTopLevelParser(ParserContext parser)
        {
            this.parser = parser;
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

        internal virtual ModifierCollection ParseModifiers(TokenStream tokens)
        {
            return ModifierCollection.EMPTY;
        }

        internal virtual TopLevelEntity Parse(
            TokenStream tokens,
            TopLevelEntity owner,
            FileScope fileScope)
        {
            AnnotationCollection annotations = annotations = this.parser.AnnotationParser.ParseAnnotations(tokens);

            ModifierCollection modifiers_IGNORED = this.ParseModifiers(tokens);

            string value = tokens.PeekValue();

            if (value == this.parser.Keywords.IMPORT)
            {
                throw this.parser.GenerateParseError(
                    ErrorMessages.ALL_IMPORTS_MUST_OCCUR_AT_BEGINNING_OF_FILE,
                    tokens.Pop());
            }

            // The returns are inline, so you'll have to refactor or put the check inside each parse call.
            // Or maybe a try/finally.
            TODO.CheckForUnusedAnnotations();

            Token staticToken = null;
            Token finalToken = null;
            while (value == this.parser.Keywords.STATIC || value == this.parser.Keywords.FINAL)
            {
                if (value == this.parser.Keywords.STATIC && staticToken == null)
                {
                    staticToken = tokens.Pop();
                    value = tokens.PeekValue();
                }
                if (value == this.parser.Keywords.FINAL && finalToken == null)
                {
                    finalToken = tokens.Pop();
                    value = tokens.PeekValue();
                }
            }

            if (staticToken != null || finalToken != null)
            {
                if (value != this.parser.Keywords.CLASS)
                {
                    if (staticToken != null)
                    {
                        throw ParserException.ThrowException(this.parser.CurrentLocale, ErrorMessages.ONLY_CLASSES_METHODS_FIELDS_MAY_BE_STATIC, staticToken);
                    }
                    else
                    {
                        throw ParserException.ThrowException(this.parser.CurrentLocale, ErrorMessages.ONLY_CLASSES_MAY_BE_FINAL, finalToken);
                    }
                }

                if (staticToken != null && finalToken != null)
                {
                    throw ParserException.ThrowException(this.parser.CurrentLocale, ErrorMessages.CLASSES_CANNOT_BE_STATIC_AND_FINAL_SIMULTANEOUSLY, staticToken);
                }
            }

            if (value == this.parser.Keywords.NAMESPACE) return this.ParseNamespace(tokens, owner, fileScope, annotations);
            if (value == this.parser.Keywords.CONST) return this.ParseConst(tokens, owner, fileScope, annotations);
            if (value == this.parser.Keywords.FUNCTION) return this.ParseFunction(tokens, owner, fileScope, annotations, modifiers_IGNORED);
            if (value == this.parser.Keywords.CLASS) return this.ParseClassDefinition(tokens, owner, staticToken, finalToken, fileScope, annotations);
            if (value == this.parser.Keywords.ENUM) return this.ParseEnumDefinition(tokens, owner, fileScope, annotations);
            if (value == this.parser.Keywords.CONSTRUCTOR && owner is ClassDefinition) return this.ParseConstructor(tokens, (ClassDefinition)owner, annotations);

            FunctionDefinition nullableFunctionDef = this.MaybeParseFunctionDefinition(tokens, owner, fileScope, annotations, modifiers_IGNORED);
            if (nullableFunctionDef != null)
            {
                return nullableFunctionDef;
            }

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
            AnnotationCollection annotations)
        {
            Token constructorToken = tokens.PopExpected(this.parser.Keywords.CONSTRUCTOR);
            ConstructorDefinition ctor = new ConstructorDefinition(constructorToken, annotations, owner);
            tokens.PopExpected("(");

            List<Token> argNames = new List<Token>();
            List<Expression> argValues = new List<Expression>();
            bool optionalArgFound = false;
            while (!tokens.PopIfPresent(")"))
            {
                if (argNames.Count > 0)
                {
                    tokens.PopExpected(",");
                }

                Token argName = tokens.Pop();
                this.parser.VerifyIdentifier(argName);
                Expression defaultValue = null;
                if (tokens.PopIfPresent("="))
                {
                    defaultValue = this.parser.ExpressionParser.Parse(tokens, ctor);
                    optionalArgFound = true;
                }
                else if (optionalArgFound)
                {
                    throw this.parser.GenerateParseError(
                        ErrorMessages.OPTIONAL_ARGUMENT_WAS_NOT_AT_END_OF_ARGUMENT_LIST,
                        argName);
                }

                argNames.Add(argName);
                argValues.Add(defaultValue);
            }

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

            ctor.SetArgs(argNames, argValues);
            ctor.SetBaseArgs(baseArgs);
            ctor.SetCode(code);
            ctor.BaseToken = baseToken;
            return ctor;
        }

        protected abstract ConstDefinition ParseConst(
            TokenStream tokens,
            Node owner,
            FileScope fileScope,
            AnnotationCollection annotations);

        protected virtual EnumDefinition ParseEnumDefinition(TokenStream tokens, Node owner, FileScope fileScope, AnnotationCollection annotations)
        {
            Token enumToken = tokens.PopExpected(this.parser.Keywords.ENUM);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            string name = nameToken.Value;
            EnumDefinition ed = new EnumDefinition(enumToken, nameToken, owner, fileScope, annotations);

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

        protected virtual ClassDefinition ParseClassDefinition(TokenStream tokens, Node owner, Token staticToken, Token finalToken, FileScope fileScope, AnnotationCollection classAnnotations)
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
                staticToken,
                finalToken,
                fileScope,
                classAnnotations);

            tokens.PopExpected("{");
            List<FunctionDefinition> methods = new List<FunctionDefinition>();
            List<FieldDefinition> fields = new List<FieldDefinition>();

            while (!tokens.PopIfPresent("}"))
            {
                this.ParseClassMember(tokens, fileScope, cd, methods, fields);
            }

            cd.Methods = methods.ToArray();
            cd.Fields = fields.ToArray();

            return cd;
        }

        protected abstract void ParseClassMember(
            TokenStream tokens,
            FileScope fileScope,
            ClassDefinition classDef,
            IList<FunctionDefinition> methodsOut,
            IList<FieldDefinition> fieldsOut);

        protected abstract FieldDefinition ParseField(
            TokenStream tokens,
            ClassDefinition owner,
            AnnotationCollection annotations,
            ModifierCollection modifiers);

        protected virtual Namespace ParseNamespace(TokenStream tokens, Node owner, FileScope fileScope, AnnotationCollection annotations)
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

            Namespace namespaceInstance = new Namespace(namespaceToken, name, owner, fileScope, annotations);

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
            AnnotationCollection annotations,
            ModifierCollection modifiers);
    }
}
