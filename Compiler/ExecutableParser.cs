using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;
using Common;

namespace Crayon
{
    internal class ExecutableParser
    {
        private Parser parser;
        public ExecutableParser(Parser parser)
        {
            this.parser = parser;
        }

        private static readonly HashSet<string> ASSIGNMENT_OPS = new HashSet<string>(
            "= += -= *= /= %= |= &= ^= <<= >>=".Split(' '));

        public TopLevelConstruct ParseTopLevel(
            TokenStream tokens,
            TopLevelConstruct owner)
        {
            string value = tokens.PeekValue();

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
                        throw new ParserException(staticToken, "Only classes, methods, and fields may be marked as static");
                    }
                    else
                    {
                        throw new ParserException(finalToken, "Only classes may be marked as final.");
                    }
                }

                if (staticToken != null && finalToken != null)
                {
                    throw new ParserException(staticToken, "Classes cannot be both static and final.");
                }
            }
            
            if (value == parser.Keywords.IMPORT)
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

                return new ImportStatement(importToken, importPath, parser.CurrentLibrary);
            }

            if (value == this.parser.Keywords.ENUM)
            {
                return this.ParseEnumDefinition(tokens, owner);
            }

            if (value == this.parser.Keywords.NAMESPACE)
            {
                return this.ParseNamespace(tokens, owner);
            }

            if (value == this.parser.Keywords.CONST) return this.ParseConst(tokens, owner);
            if (value == this.parser.Keywords.FUNCTION) return this.ParseFunction(tokens, owner);
            if (value == this.parser.Keywords.CLASS) return this.ParseClassDefinition(tokens, owner, staticToken, finalToken);
            if (value == this.parser.Keywords.ENUM) return this.ParseEnumDefinition(tokens, owner);
            if (value == this.parser.Keywords.CONSTRUCTOR) return this.ParseConstructor(tokens, owner);

            throw new ParserException(tokens.Peek(), "Unrecognized token.");
        }

        public Executable Parse(
            TokenStream tokens,
            bool simpleOnly,
            bool semicolonPresent,
            TopLevelConstruct owner)
        {
            string value = tokens.PeekValue();

            if (!simpleOnly)
            {
                if (value == this.parser.Keywords.FUNCTION || value == this.parser.Keywords.CLASS)
                {
                    throw new ParserException(
                        tokens.Peek(),
                        (value == this.parser.Keywords.FUNCTION ? "Function" : "Class") +
                        " definition cannot be nested in another construct.");
                }
                
                if (value == parser.Keywords.IMPORT)
                {
                    throw new ParserException(tokens.Peek(), "Imports can only be made from the root of a file and cannot be nested inside other constructs.");
                }

                if (value == this.parser.Keywords.ENUM)
                {
                    throw new ParserException(tokens.Peek(), "Enums can only be defined from the root of a file and cannot be nested inside functions/loops/etc.");
                }

                if (value == this.parser.Keywords.NAMESPACE)
                {
                    throw new ParserException(tokens.Peek(), "Namespace declarations cannot be nested in other constructs.");
                }

                if (value == this.parser.Keywords.CONST)
                {
                    throw new ParserException(tokens.Peek(), "Constant declarations cannot be nested in other constructs.");
                }
                
                if (value == this.parser.Keywords.FOR) return this.ParseFor(tokens, owner);
                if (value == this.parser.Keywords.WHILE) return this.ParseWhile(tokens, owner);
                if (value == this.parser.Keywords.DO) return this.ParseDoWhile(tokens, owner);
                if (value == this.parser.Keywords.SWITCH) return this.ParseSwitch(tokens, owner);
                if (value == this.parser.Keywords.IF) return this.ParseIf(tokens, owner);
                if (value == this.parser.Keywords.TRY) return this.ParseTry(tokens, owner);
                if (value == this.parser.Keywords.RETURN) return this.ParseReturn(tokens, owner);
                if (value == this.parser.Keywords.BREAK) return this.ParseBreak(tokens, owner);
                if (value == this.parser.Keywords.CONTINUE) return this.ParseContinue(tokens, owner);
                if (value == this.parser.Keywords.THROW) return this.ParseThrow(tokens, owner);
            }

            Expression expr = this.parser.ExpressionParser.Parse(tokens, owner);
            value = tokens.PeekValue();
            if (ASSIGNMENT_OPS.Contains(value))
            {
                Token assignment = tokens.Pop();
                Expression assignmentValue = this.parser.ExpressionParser.Parse(tokens, owner);
                if (semicolonPresent) tokens.PopExpected(";");
                return new Assignment(expr, assignment, assignment.Value, assignmentValue, owner);
            }

            if (semicolonPresent)
            {
                tokens.PopExpected(";");
            }

            return new ExpressionAsExecutable(expr, owner);
        }
        
        private Executable ParseThrow(TokenStream tokens, TopLevelConstruct owner)
        {
            Token throwToken = tokens.PopExpected(this.parser.Keywords.THROW);
            Expression throwExpression = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(";");
            return new ThrowStatement(throwToken, throwExpression, owner);
        }

        private ConstructorDefinition ParseConstructor(TokenStream tokens, TopLevelConstruct owner)
        {
            Token constructorToken = tokens.PopExpected(this.parser.Keywords.CONSTRUCTOR);
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
                    defaultValue = this.parser.ExpressionParser.Parse(tokens, owner);
                    optionalArgFound = true;
                }
                else if (optionalArgFound)
                {
                    throw new ParserException(argName, "All optional arguments must come at the end of the argument list.");
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

                    baseArgs.Add(this.parser.ExpressionParser.Parse(tokens, owner));
                }
            }

            IList<Executable> code = Parser.ParseBlock(parser, tokens, true, owner);

            return new ConstructorDefinition(constructorToken, argNames, argValues, baseArgs, code, baseToken, owner);
        }

        private ConstStatement ParseConst(TokenStream tokens, TopLevelConstruct owner)
        {
            Token constToken = tokens.PopExpected(this.parser.Keywords.CONST);
            Token nameToken = tokens.Pop();
            ConstStatement constStatement = new ConstStatement(constToken, nameToken, parser.CurrentNamespace, owner, parser.CurrentLibrary);
            this.parser.VerifyIdentifier(nameToken);
            tokens.PopExpected("=");
            constStatement.Expression = this.parser.ExpressionParser.Parse(tokens, constStatement);
            tokens.PopExpected(";");

            return constStatement;
        }

        private EnumDefinition ParseEnumDefinition(TokenStream tokens, TopLevelConstruct owner)
        {
            Token enumToken = tokens.PopExpected(this.parser.Keywords.ENUM);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            string name = nameToken.Value;
            EnumDefinition ed = new EnumDefinition(enumToken, nameToken, parser.CurrentNamespace, owner, parser.CurrentLibrary);

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

        private ClassDefinition ParseClassDefinition(TokenStream tokens, TopLevelConstruct owner, Token staticToken, Token finalToken)
        {
            Token classToken = tokens.PopExpected(this.parser.Keywords.CLASS);
            Token classNameToken = tokens.Pop();
            this.parser.VerifyIdentifier(classNameToken);
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
                parser.CurrentNamespace,
                owner,
                parser.CurrentLibrary,
                staticToken,
                finalToken);

            tokens.PopExpected("{");
            List<FunctionDefinition> methods = new List<FunctionDefinition>();
            List<FieldDeclaration> fields = new List<FieldDeclaration>();
            ConstructorDefinition constructorDef = null;
            ConstructorDefinition staticConstructorDef = null;

            while (!tokens.PopIfPresent("}"))
            {
                Dictionary<string, List<Annotation>> annotations = null;

                while (tokens.IsNext("@"))
                {
                    annotations = annotations ?? new Dictionary<string, List<Annotation>>();
                    Annotation annotation = this.parser.AnnotationParser.ParseAnnotation(tokens);
                    if (!annotations.ContainsKey(annotation.Type))
                    {
                        annotations[annotation.Type] = new List<Annotation>();
                    }

                    annotations[annotation.Type].Add(annotation);
                }

                if (tokens.IsNext(this.parser.Keywords.FUNCTION) ||
                    tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.FUNCTION))
                {
                    methods.Add((FunctionDefinition)this.parser.ExecutableParser.ParseFunction(tokens, cd));
                }
                else if (tokens.IsNext(this.parser.Keywords.CONSTRUCTOR))
                {
                    if (constructorDef != null)
                    {
                        throw new ParserException(tokens.Pop(), "Multiple constructors are not allowed. Use optional arguments.");
                    }

                    constructorDef = (ConstructorDefinition)this.parser.ExecutableParser.ParseConstructor(tokens, cd);

                    if (annotations != null && annotations.ContainsKey(this.parser.Keywords.PRIVATE))
                    {
                        constructorDef.PrivateAnnotation = annotations[this.parser.Keywords.PRIVATE][0];
                        annotations[this.parser.Keywords.PRIVATE].RemoveAt(0);
                    }
                }
                else if (tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.CONSTRUCTOR))
                {
                    tokens.Pop(); // static token
                    if (staticConstructorDef != null)
                    {
                        throw new ParserException(tokens.Pop(), "Multiple static constructors are not allowed.");
                    }

                    staticConstructorDef = (ConstructorDefinition)this.parser.ExecutableParser.ParseConstructor(tokens, cd);
                }
                else if (tokens.IsNext(this.parser.Keywords.FIELD) ||
                    tokens.AreNext(this.parser.Keywords.STATIC, this.parser.Keywords.FIELD))
                {
                    fields.Add(this.parser.ExecutableParser.ParseField(tokens, cd));
                }
                else
                {
                    tokens.PopExpected("}");
                }

                if (annotations != null)
                {
                    foreach (List<Annotation> annotationsOfType in annotations.Values)
                    {
                        if (annotationsOfType.Count > 0)
                        {
                            throw new ParserException(annotationsOfType[0].FirstToken, "Unused or extra annotation.");
                        }
                    }
                }
            }

            cd.Methods = methods.ToArray();
            cd.Constructor = constructorDef;
            cd.StaticConstructor = staticConstructorDef;
            cd.Fields = fields.ToArray();

            return cd;
        }

        private FieldDeclaration ParseField(TokenStream tokens, ClassDefinition owner)
        {
            bool isStatic = tokens.PopIfPresent(this.parser.Keywords.STATIC);
            Token fieldToken = tokens.PopExpected(this.parser.Keywords.FIELD);
            Token nameToken = tokens.Pop();
            this.parser.VerifyIdentifier(nameToken);
            FieldDeclaration fd = new FieldDeclaration(fieldToken, nameToken, owner, isStatic);
            if (tokens.PopIfPresent("="))
            {
                fd.DefaultValue = this.parser.ExpressionParser.Parse(tokens, owner);
            }
            tokens.PopExpected(";");
            return fd;
        }

        private Namespace ParseNamespace(TokenStream tokens, TopLevelConstruct owner)
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
            parser.PushNamespacePrefix(name);

            Namespace namespaceInstance = new Namespace(namespaceToken, name, owner, parser.CurrentLibrary);

            tokens.PopExpected("{");
            List<TopLevelConstruct> namespaceMembers = new List<TopLevelConstruct>();
            while (!tokens.PopIfPresent("}"))
            {
                TopLevelConstruct executable = this.parser.ExecutableParser.ParseTopLevel(tokens, namespaceInstance);
                if (executable is FunctionDefinition ||
                    executable is ClassDefinition ||
                    executable is EnumDefinition ||
                    executable is ConstStatement ||
                    executable is Namespace)
                {
                    namespaceMembers.Add(executable);
                }
                else
                {
                    throw new ParserException(executable.FirstToken, "Only function, class, and nested namespace declarations may exist as direct members of a namespace.");
                }
            }

            namespaceInstance.Code = namespaceMembers.ToArray();

            parser.PopNamespacePrefix();

            return namespaceInstance;
        }

        private FunctionDefinition ParseFunction(TokenStream tokens, TopLevelConstruct nullableOwner)
        {
            bool isStatic =
                nullableOwner != null &&
                nullableOwner is ClassDefinition &&
                tokens.PopIfPresent(this.parser.Keywords.STATIC);

            Token functionToken = tokens.PopExpected(this.parser.Keywords.FUNCTION);

            List<Annotation> functionAnnotations = new List<Annotation>();

            while (tokens.IsNext("@"))
            {
                functionAnnotations.Add(this.parser.AnnotationParser.ParseAnnotation(tokens));
            }

            Token functionNameToken = tokens.Pop();
            this.parser.VerifyIdentifier(functionNameToken);

            FunctionDefinition fd = new FunctionDefinition(functionToken, parser.CurrentLibrary, nullableOwner, isStatic, functionNameToken, functionAnnotations, parser.CurrentNamespace);

            tokens.PopExpected("(");
            List<Token> argNames = new List<Token>();
            List<Expression> defaultValues = new List<Expression>();
            List<Annotation> argAnnotations = new List<Annotation>();
            bool optionalArgFound = false;
            while (!tokens.PopIfPresent(")"))
            {
                if (argNames.Count > 0) tokens.PopExpected(",");

                Annotation annotation = tokens.IsNext("@") ? this.parser.AnnotationParser.ParseAnnotation(tokens) : null;
                Token argName = tokens.Pop();
                Expression defaultValue = null;
                this.parser.VerifyIdentifier(argName);
                if (tokens.PopIfPresent("="))
                {
                    optionalArgFound = true;
                    defaultValue = this.parser.ExpressionParser.Parse(tokens, fd);
                }
                else if (optionalArgFound)
                {
                    throw new ParserException(argName, "All optional arguments must come at the end of the argument list.");
                }
                argAnnotations.Add(annotation);
                argNames.Add(argName);
                defaultValues.Add(defaultValue);
            }

            IList<Executable> code = Parser.ParseBlock(parser, tokens, true, fd);

            fd.ArgNames = argNames.ToArray();
            fd.DefaultValues = defaultValues.ToArray();
            fd.ArgAnnotations = argAnnotations.ToArray();
            fd.Code = code.ToArray();

            return fd;
        }

        private Executable ParseFor(TokenStream tokens, TopLevelConstruct owner)
        {
            Token forToken = tokens.PopExpected(this.parser.Keywords.FOR);
            tokens.PopExpected("(");
            if (!tokens.HasMore) tokens.ThrowEofException();

            if (this.parser.IsValidIdentifier(tokens.PeekValue()) && tokens.PeekValue(1) == ":")
            {
                Token iteratorToken = tokens.Pop();
                if (this.parser.IsReservedKeyword(iteratorToken.Value))
                {
                    throw new ParserException(iteratorToken, "Cannot use this name for an iterator.");
                }
                tokens.PopExpected(":");
                Expression iterationExpression = this.parser.ExpressionParser.Parse(tokens, owner);
                tokens.PopExpected(")");
                IList<Executable> body = Parser.ParseBlock(parser, tokens, false, owner);

                return new ForEachLoop(forToken, iteratorToken, iterationExpression, body, owner);
            }
            else
            {
                List<Executable> init = new List<Executable>();
                while (!tokens.PopIfPresent(";"))
                {
                    if (init.Count > 0) tokens.PopExpected(",");
                    init.Add(this.Parse(tokens, true, false, owner));
                }
                Expression condition = null;
                if (!tokens.PopIfPresent(";"))
                {
                    condition = this.parser.ExpressionParser.Parse(tokens, owner);
                    tokens.PopExpected(";");
                }
                List<Executable> step = new List<Executable>();
                while (!tokens.PopIfPresent(")"))
                {
                    if (step.Count > 0) tokens.PopExpected(",");
                    step.Add(this.Parse(tokens, true, false, owner));
                }

                IList<Executable> body = Parser.ParseBlock(parser, tokens, false, owner);

                return new ForLoop(forToken, init, condition, step, body, owner);
            }
        }

        private Executable ParseWhile(TokenStream tokens, TopLevelConstruct owner)
        {
            Token whileToken = tokens.PopExpected(this.parser.Keywords.WHILE);
            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            IList<Executable> body = Parser.ParseBlock(parser, tokens, false, owner);
            return new WhileLoop(whileToken, condition, body, owner);
        }

        private Executable ParseDoWhile(TokenStream tokens, TopLevelConstruct owner)
        {
            Token doToken = tokens.PopExpected(this.parser.Keywords.DO);
            IList<Executable> body = Parser.ParseBlock(parser, tokens, true, owner);
            tokens.PopExpected(this.parser.Keywords.DO_WHILE_END);
            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            tokens.PopExpected(";");
            return new DoWhileLoop(doToken, body, condition, owner);
        }

        private Executable ParseSwitch(TokenStream tokens, TopLevelConstruct owner)
        {
            Token switchToken = tokens.PopExpected(this.parser.Keywords.SWITCH);

            Expression explicitMax = null;
            Token explicitMaxToken = null;
            if (tokens.IsNext("{"))
            {
                explicitMaxToken = tokens.Pop();
                explicitMax = this.parser.ExpressionParser.Parse(tokens, owner);
                tokens.PopExpected("}");
            }

            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            tokens.PopExpected("{");
            List<List<Expression>> cases = new List<List<Expression>>();
            List<Token> firstTokens = new List<Token>();
            List<List<Executable>> code = new List<List<Executable>>();
            char state = '?'; // ? - first, O - code, A - case
            bool defaultEncountered = false;
            while (!tokens.PopIfPresent("}"))
            {
                if (tokens.IsNext(this.parser.Keywords.CASE))
                {
                    if (defaultEncountered)
                    {
                        throw new ParserException(tokens.Peek(), "default condition in a switch statement must be the last condition.");
                    }

                    Token caseToken = tokens.PopExpected(this.parser.Keywords.CASE);
                    if (state != 'A')
                    {
                        cases.Add(new List<Expression>());
                        firstTokens.Add(caseToken);
                        code.Add(null);
                        state = 'A';
                    }
                    cases[cases.Count - 1].Add(this.parser.ExpressionParser.Parse(tokens, owner));
                    tokens.PopExpected(":");
                }
                else if (tokens.IsNext(this.parser.Keywords.DEFAULT))
                {
                    Token defaultToken = tokens.PopExpected(this.parser.Keywords.DEFAULT);
                    if (state != 'A')
                    {
                        cases.Add(new List<Expression>());
                        firstTokens.Add(defaultToken);
                        code.Add(null);
                        state = 'A';
                    }
                    cases[cases.Count - 1].Add(null);
                    tokens.PopExpected(":");
                    defaultEncountered = true;
                }
                else
                {
                    if (state != 'O')
                    {
                        cases.Add(null);
                        firstTokens.Add(null);
                        code.Add(new List<Executable>());
                        state = 'O';
                    }
                    code[code.Count - 1].Add(this.parser.ExecutableParser.Parse(tokens, false, true, owner));
                }
            }

            return new SwitchStatement(switchToken, condition, firstTokens, cases, code, explicitMax, explicitMaxToken, owner);
        }

        private Executable ParseIf(TokenStream tokens, TopLevelConstruct owner)
        {
            Token ifToken = tokens.PopExpected(this.parser.Keywords.IF);
            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            IList<Executable> body = Parser.ParseBlock(parser, tokens, false, owner);
            IList<Executable> elseBody;
            if (tokens.PopIfPresent(this.parser.Keywords.ELSE))
            {
                elseBody = Parser.ParseBlock(parser, tokens, false, owner);
            }
            else
            {
                elseBody = new Executable[0];
            }
            return new IfStatement(ifToken, condition, body, elseBody, owner);
        }

        private Executable ParseTry(TokenStream tokens, TopLevelConstruct owner)
        {
            Token tryToken = tokens.PopExpected(this.parser.Keywords.TRY);
            IList<Executable> tryBlock = Parser.ParseBlock(parser, tokens, true, owner);

            List<Token> catchTokens = new List<Token>();
            List<string[]> exceptionTypes = new List<string[]>();
            List<Token[]> exceptionTypeTokens = new List<Token[]>();
            List<Token> exceptionVariables = new List<Token>();
            List<Executable[]> catchBlocks = new List<Executable[]>();

            Token finallyToken = null;
            IList<Executable> finallyBlock = null;

            while (tokens.IsNext(this.parser.Keywords.CATCH))
            {
                /*
                    Parse patterns:
                        All exceptions:
                            1a: catch { ... }
                            1b: catch (e) { ... }

                        A certain exception:
                            2a: catch (ExceptionName) { ... }
                            2b: catch (ExceptionName e) { ... }

                        Certain exceptions:
                            3a: catch (ExceptionName1 | ExceptionName2) { ... }
                            3b: catch (ExceptionName1 | ExceptionName2 e) { ... }

                    Non-Context-Free alert:
                        Note that if the exception variable does not contain a '.' character, 1b and 2a are
                        ambiguous at parse time. Treat them both as 1b and then if the classname resolution
                        fails, treat this as a variable.

                        This is actually kind of bad because a typo in the classname will not be known.
                        e.g "catch (Excpetion) {" will compile as a variable called "Excpetion"

                        End-user workarounds:
                        - always use a variable name OR
                        - always fully qualify exception types e.g. Core.Exception
                        Long-term plan:
                        - add warning support and emit warnings for:
                            - unused variables
                            - style-breaking uppercase variables.
                */

                Token catchToken = tokens.PopExpected(this.parser.Keywords.CATCH);

                List<string> classNames = new List<string>();
                List<Token> classTokens = new List<Token>();
                Token variableToken = null;

                if (tokens.PopIfPresent("("))
                {
                    // This first one might actually be a variable. Assume class for now and sort it out later.
                    // (and by "later" I mean the ResolveNames phase)
                    Token classFirstToken = tokens.Pop();
                    string className = this.parser.PopClassNameWithFirstTokenAlreadyPopped(tokens, classFirstToken);
                    classNames.Add(className);
                    classTokens.Add(classFirstToken);

                    while (tokens.PopIfPresent("|"))
                    {
                        classFirstToken = tokens.Pop();
                        className = this.parser.PopClassNameWithFirstTokenAlreadyPopped(tokens, classFirstToken);
                        classNames.Add(className);
                        classTokens.Add(classFirstToken);
                    }

                    if (!tokens.IsNext(")"))
                    {
                        variableToken = tokens.Pop();
                        this.parser.VerifyIdentifier(variableToken);
                    }

                    tokens.PopExpected(")");
                }
                else
                {
                    classNames.Add(null);
                    classTokens.Add(null);
                }

                Executable[] catchBlockCode = Parser.ParseBlock(parser, tokens, true, owner).ToArray();


                catchTokens.Add(catchToken);
                exceptionTypes.Add(classNames.ToArray());
                exceptionTypeTokens.Add(classTokens.ToArray());
                exceptionVariables.Add(variableToken);
                catchBlocks.Add(catchBlockCode);
            }

            if (tokens.IsNext(this.parser.Keywords.FINALLY))
            {
                finallyToken = tokens.Pop();
                finallyBlock = Parser.ParseBlock(parser, tokens, true, owner);
            }

            return new TryStatement(tryToken, tryBlock, catchTokens, exceptionVariables, exceptionTypeTokens, exceptionTypes, catchBlocks, finallyToken, finallyBlock, owner);
        }

        private Executable ParseBreak(TokenStream tokens, TopLevelConstruct owner)
        {
            Token breakToken = tokens.PopExpected(this.parser.Keywords.BREAK);
            tokens.PopExpected(";");
            return new BreakStatement(breakToken, owner);
        }

        private Executable ParseContinue(TokenStream tokens, TopLevelConstruct owner)
        {
            Token continueToken = tokens.PopExpected(this.parser.Keywords.CONTINUE);
            tokens.PopExpected(";");
            return new ContinueStatement(continueToken, owner);
        }

        private Executable ParseReturn(TokenStream tokens, TopLevelConstruct owner)
        {
            Token returnToken = tokens.PopExpected(this.parser.Keywords.RETURN);
            Expression expr = null;
            if (!tokens.PopIfPresent(";"))
            {
                expr = this.parser.ExpressionParser.Parse(tokens, owner);
                tokens.PopExpected(";");
            }

            return new ReturnStatement(returnToken, expr, owner);
        }
    }
}
