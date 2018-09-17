using Localization;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    // This class (and AbstractExpressionParser) ideally should be split into an AbstractCurlyBraceLanguageParser
    internal abstract class AbstractExecutableParser
    {
        private static readonly HashSet<string> ASSIGNMENT_OPS = new HashSet<string>(
            "= += -= *= /= %= |= &= ^= <<= >>=".Split(' '));

        protected ParserContext parser;

        public AbstractExecutableParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal virtual Executable Parse(
            TokenStream tokens,
            bool simpleOnly,
            bool semicolonPresent,
            Node owner)
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
                    throw this.parser.GenerateParseError(ErrorMessages.ALL_IMPORTS_MUST_OCCUR_AT_BEGINNING_OF_FILE, tokens.Peek());
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

            Assignment assignmentExec = this.MaybeParseTypedVariableDeclaration(tokens, owner);
            if (assignmentExec != null)
            {
                if (semicolonPresent) tokens.PopExpected(";");
                return assignmentExec;
            }

            Expression expr = this.parser.ExpressionParser.Parse(tokens, owner);
            value = tokens.PeekValue();
            if (ASSIGNMENT_OPS.Contains(value))
            {
                Token assignment = tokens.Pop();
                Expression assignmentValue = this.parser.ExpressionParser.Parse(tokens, owner);
                if (semicolonPresent) tokens.PopExpected(";");
                return new Assignment(expr, null, assignment, assignmentValue, owner);
            }

            if (semicolonPresent)
            {
                tokens.PopExpected(";");
            }

            return new ExpressionAsExecutable(expr, owner);
        }

        protected virtual Assignment MaybeParseTypedVariableDeclaration(TokenStream tokens, Node owner)
        {
            return null;
        }

        internal virtual IList<Executable> ParseBlock(TokenStream tokens, bool bracketsRequired, Node owner)
        {
            List<Executable> output = new List<Executable>();

            if (tokens.PopIfPresent("{"))
            {
                while (!tokens.PopIfPresent("}"))
                {
                    output.Add(this.parser.ExecutableParser.Parse(tokens, false, true, owner));
                }
            }
            else
            {
                if (bracketsRequired)
                {
                    tokens.PopExpected("{"); // throws with reasonable exception message.
                }

                if (tokens.PopIfPresent(";"))
                {
                    return output;
                }

                output.Add(this.parser.ExecutableParser.Parse(tokens, false, true, owner));
            }
            return output;
        }

        private Executable ParseThrow(TokenStream tokens, Node owner)
        {
            Token throwToken = tokens.PopExpected(this.parser.Keywords.THROW);
            Expression throwExpression = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(";");
            return new ThrowStatement(throwToken, throwExpression, owner);
        }

        protected Executable ParseFor(TokenStream tokens, Node owner)
        {
            Token forToken = tokens.PopExpected(this.parser.Keywords.FOR);
            tokens.PopExpected("(");
            tokens.EnsureNotEof();

            if (this.IsForEachLoopParenthesisContents(tokens))
            {
                System.Tuple<AType, Token> iteratorVariable = this.ParseForEachLoopIteratorVariable(tokens, owner);
                AType iteratorVariableType = iteratorVariable.Item1;
                Token iteratorToken = iteratorVariable.Item2;
                tokens.PopExpected(":");
                Expression iterationExpression = this.parser.ExpressionParser.Parse(tokens, owner);
                tokens.PopExpected(")");
                IList<Executable> body = this.ParseBlock(tokens, false, owner);
                return new ForEachLoop(forToken, iteratorVariableType, iteratorVariable.Item2, iterationExpression, body, owner);
            }
            else
            {
                System.Tuple<IList<Executable>, Expression, IList<Executable>> parts = this.ParseForLoopComponents(tokens, owner);
                IList<Executable> body = this.ParseBlock(tokens, false, owner);
                return new ForLoop(forToken, parts.Item1, parts.Item2, parts.Item3, body, owner);
            }
        }

        protected abstract System.Tuple<AType, Token> ParseForEachLoopIteratorVariable(TokenStream tokens, Node owner);

        protected abstract bool IsForEachLoopParenthesisContents(TokenStream tokens);

        protected System.Tuple<IList<Executable>, Expression, IList<Executable>> ParseForLoopComponents(TokenStream tokens, Node owner)
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

            return new System.Tuple<IList<Executable>, Expression, IList<Executable>>(init, condition, step);
        }

        private Executable ParseWhile(TokenStream tokens, Node owner)
        {
            Token whileToken = tokens.PopExpected(this.parser.Keywords.WHILE);
            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            IList<Executable> body = this.ParseBlock(tokens, false, owner);
            return new WhileLoop(whileToken, condition, body, owner);
        }

        private Executable ParseDoWhile(TokenStream tokens, Node owner)
        {
            Token doToken = tokens.PopExpected(this.parser.Keywords.DO);
            IList<Executable> body = this.ParseBlock(tokens, true, owner);
            tokens.PopExpected(this.parser.Keywords.DO_WHILE_END);
            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            tokens.PopExpected(";");
            return new DoWhileLoop(doToken, body, condition, owner);
        }

        private Executable ParseSwitch(TokenStream tokens, Node owner)
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

        private Executable ParseIf(TokenStream tokens, Node owner)
        {
            Token ifToken = tokens.PopExpected(this.parser.Keywords.IF);
            tokens.PopExpected("(");
            Expression condition = this.parser.ExpressionParser.Parse(tokens, owner);
            tokens.PopExpected(")");
            IList<Executable> body = this.ParseBlock(tokens, false, owner);
            IList<Executable> elseBody;
            if (tokens.PopIfPresent(this.parser.Keywords.ELSE))
            {
                elseBody = this.ParseBlock(tokens, false, owner);
            }
            else
            {
                elseBody = new Executable[0];
            }
            return new IfStatement(ifToken, condition, body, elseBody, owner);
        }

        private Executable ParseTry(TokenStream tokens, Node owner)
        {
            Token tryToken = tokens.PopExpected(this.parser.Keywords.TRY);
            IList<Executable> tryBlock = this.ParseBlock(tokens, true, owner);

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

                Executable[] catchBlockCode = this.ParseBlock(tokens, true, owner).ToArray();

                catchTokens.Add(catchToken);
                exceptionTypes.Add(classNames.ToArray());
                exceptionTypeTokens.Add(classTokens.ToArray());
                exceptionVariables.Add(variableToken);
                catchBlocks.Add(catchBlockCode);
            }

            if (tokens.IsNext(this.parser.Keywords.FINALLY))
            {
                finallyToken = tokens.Pop();
                finallyBlock = this.ParseBlock(tokens, true, owner);
            }

            return new TryStatement(tryToken, tryBlock, catchTokens, exceptionVariables, exceptionTypeTokens, exceptionTypes, catchBlocks, finallyToken, finallyBlock, owner);
        }

        private Executable ParseBreak(TokenStream tokens, Node owner)
        {
            Token breakToken = tokens.PopExpected(this.parser.Keywords.BREAK);
            tokens.PopExpected(";");
            return new BreakStatement(breakToken, owner);
        }

        private Executable ParseContinue(TokenStream tokens, Node owner)
        {
            Token continueToken = tokens.PopExpected(this.parser.Keywords.CONTINUE);
            tokens.PopExpected(";");
            return new ContinueStatement(continueToken, owner);
        }

        private Executable ParseReturn(TokenStream tokens, Node owner)
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
