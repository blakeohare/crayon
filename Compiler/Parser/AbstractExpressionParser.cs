using Parser.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    internal abstract class AbstractExpressionParser
    {
        protected ParserContext parser;

        public AbstractExpressionParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal virtual Expression Parse(TokenStream tokens, Node owner)
        {
            return ParseTernary(tokens, owner);
        }

        private Expression ParseTernary(TokenStream tokens, Node owner)
        {
            Expression root = ParseNullCoalescing(tokens, owner);
            if (tokens.PopIfPresent("?"))
            {
                Expression trueExpr = ParseTernary(tokens, owner);
                tokens.PopExpected(":");
                Expression falseExpr = ParseTernary(tokens, owner);

                return new Ternary(root, trueExpr, falseExpr, owner);
            }
            return root;
        }

        private Expression ParseNullCoalescing(TokenStream tokens, Node owner)
        {
            Expression root = ParseBooleanCombination(tokens, owner);
            if (tokens.PopIfPresent("??"))
            {
                Expression secondaryExpression = ParseNullCoalescing(tokens, owner);
                return new NullCoalescer(root, secondaryExpression, owner);
            }
            return root;
        }

        private Expression ParseBooleanCombination(TokenStream tokens, Node owner)
        {
            Expression expr = ParseBitwiseOp(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "||" || next == "&&")
            {
                List<Expression> expressions = new List<Expression>() { expr };
                List<Token> ops = new List<Token>();
                while (next == "||" || next == "&&")
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseBitwiseOp(tokens, owner));
                    next = tokens.PeekValue();
                }
                return new BooleanCombination(expressions, ops, owner);
            }
            return expr;
        }

        private Expression ParseBitwiseOp(TokenStream tokens, Node owner)
        {
            Expression expr = ParseEqualityComparison(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "|" || next == "&" || next == "^")
            {
                Token bitwiseToken = tokens.Pop();
                Expression rightExpr = ParseBitwiseOp(tokens, owner);
                return new OpChain(expr, bitwiseToken, rightExpr, owner);
            }
            return expr;
        }

        private Expression ParseEqualityComparison(TokenStream tokens, Node owner)
        {
            Expression expr = ParseInequalityComparison(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "==" || next == "!=")
            {
                Token equalityToken = tokens.Pop();
                Expression rightExpr = ParseEqualityComparison(tokens, owner);
                return new OpChain(expr, equalityToken, rightExpr, owner);
            }
            return expr;
        }

        private Expression ParseInequalityComparison(TokenStream tokens, Node owner)
        {
            Expression expr = ParseBitShift(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "<" || next == ">" || next == "<=" || next == ">=")
            {
                // Don't allow chaining of inqeualities
                Token opToken = tokens.Pop();
                Expression rightExpr = ParseBitShift(tokens, owner);
                return new OpChain(expr, opToken, rightExpr, owner);
            }
            return expr;
        }

        private Expression ParseBitShift(TokenStream tokens, Node owner)
        {
            Expression expr = ParseAddition(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "<<" || next == ">>")
            {
                Token opToken = tokens.Pop();
                Expression rightExpr = ParseBitShift(tokens, owner);
                return new OpChain(expr, opToken, rightExpr, owner);
            }
            return expr;
        }

        private static readonly HashSet<string> ADDITION_OPS = new HashSet<string>("+ -".Split(' '));
        private static readonly HashSet<string> MULTIPLICATION_OPS = new HashSet<string>("* / %".Split(' '));
        private static readonly HashSet<string> NEGATE_OPS = new HashSet<string>("! -".Split(' '));

        private Expression ParseAddition(TokenStream tokens, Node owner)
        {
            Expression expr = ParseMultiplication(tokens, owner);
            string next = tokens.PeekValue();
            while (ADDITION_OPS.Contains(next))
            {
                Token op = tokens.Pop();
                Expression right = ParseMultiplication(tokens, owner);
                expr = new OpChain(expr, op, right, owner);
                next = tokens.PeekValue();
            }
            return expr;
        }

        private Expression ParseMultiplication(TokenStream tokens, Node owner)
        {
            Expression expr = ParseNegate(tokens, owner);
            string next = tokens.PeekValue();
            while (MULTIPLICATION_OPS.Contains(next))
            {
                Token op = tokens.Pop();
                Expression right = ParseNegate(tokens, owner);
                expr = new OpChain(expr, op, right, owner);
                next = tokens.PeekValue();
            }
            return expr;
        }

        private Expression ParseNegate(TokenStream tokens, Node owner)
        {
            string next = tokens.PeekValue();
            if (NEGATE_OPS.Contains(next))
            {
                Token negateOp = tokens.Pop();
                Expression root = ParseNegate(tokens, owner);
                if (negateOp.Value == "!") return new BooleanNot(negateOp, root, owner);
                if (negateOp.Value == "-") return new NegativeSign(negateOp, root, owner);
                throw new Exception("This shouldn't happen.");
            }

            return ParseExponents(tokens, owner);
        }

        private Expression ParseExponents(TokenStream tokens, Node owner)
        {
            Expression expr = ParseIncrement(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "**")
            {
                Token op = tokens.Pop();
                Expression right = ParseNegate(tokens, owner);
                expr = new OpChain(expr, op, right, owner);
            }
            return expr;
        }

        private Expression ParseIncrement(TokenStream tokens, Node owner)
        {
            Expression root;
            if (tokens.IsNext("++") || tokens.IsNext("--"))
            {
                Token incrementToken = tokens.Pop();
                root = this.ParseEntity(tokens, owner);
                return new Increment(incrementToken, incrementToken, incrementToken.Value == "++", true, root, owner);
            }

            root = this.ParseEntity(tokens, owner);
            if (tokens.IsNext("++") || tokens.IsNext("--"))
            {
                Token incrementToken = tokens.Pop();
                return new Increment(root.FirstToken, incrementToken, incrementToken.Value == "++", false, root, owner);
            }

            return root;
        }

        protected abstract AType ParseTypeForInstantiation(TokenStream tokens);

        protected abstract Expression ParseInstantiate(TokenStream tokens, Node owner);

        protected IList<Expression> ParseArgumentList(TokenStream tokens, Node owner)
        {
            List<Expression> args = new List<Expression>();
            tokens.PopExpected("(");
            while (!tokens.PopIfPresent(")"))
            {
                if (args.Count > 0)
                {
                    tokens.PopExpected(",");
                }
                args.Add(Parse(tokens, owner));
            }
            return args;
        }

        private Expression ParseLambda(
            TokenStream tokens,
            Token firstToken,
            IList<AType> argTypes,
            IList<Token> args,
            Node owner)
        {
            tokens.PopExpected("=>");
            Lambda lambda = new Lambda(firstToken, owner, args, argTypes);
            IList<Executable> lambdaCode;
            if (tokens.IsNext("{"))
            {
                lambdaCode = this.parser.ExecutableParser.ParseBlock(tokens, false, lambda, false);
            }
            else
            {
                Expression lambdaBodyExpression = this.parser.ExpressionParser.ParseTernary(tokens, lambda);
                lambdaCode = new Executable[] { new ExpressionAsExecutable(lambdaBodyExpression, lambda) };
            }

            if (lambdaCode.Count == 1 && lambdaCode[0] is ExpressionAsExecutable)
            {
                // If a lambda contains a single expression as its code body, then this is an implicit return statement.
                ExpressionAsExecutable eae = (ExpressionAsExecutable)lambdaCode[0];
                lambdaCode[0] = new ReturnStatement(eae.FirstToken, eae.Expression, lambda);
            }
            lambda.Code = lambdaCode.ToArray();
            return lambda;
        }

        protected abstract AType MaybeParseCastPrefix(TokenStream tokens);

        private Expression ParseEntity(TokenStream tokens, Node owner)
        {
            Expression root;
            Token firstToken = tokens.Peek();
            AType castPrefix = firstToken.Value == "(" ? this.MaybeParseCastPrefix(tokens) : null;

            if (castPrefix != null)
            {
                root = this.ParseEntity(tokens, owner);
                return new Cast(firstToken, castPrefix, root, owner, true);
            }

            Token maybeOpenParen = tokens.Peek();
            if (tokens.PopIfPresent("("))
            {
                if (tokens.PopIfPresent(")"))
                {
                    root = this.ParseLambda(tokens, firstToken, new AType[0], new Token[0], owner);
                }
                else
                {
                    if (this.parser.CurrentScope.IsStaticallyTyped)
                    {
                        TokenStream.StreamState state = tokens.RecordState();
                        AType lambdaArgType = this.parser.TypeParser.TryParse(tokens);
                        Token lambdaArg = null;
                        if (lambdaArgType != null)
                        {
                            lambdaArg = tokens.PopIfWord();
                        }

                        if (lambdaArg != null)
                        {
                            List<AType> lambdaArgTypes = new List<AType>() { lambdaArgType };
                            List<Token> lambdaArgs = new List<Token>() { lambdaArg };
                            while (tokens.PopIfPresent(","))
                            {
                                lambdaArgTypes.Add(this.parser.TypeParser.Parse(tokens));
                                lambdaArgs.Add(tokens.PopWord());
                            }
                            tokens.PopExpected(")");
                            return this.ParseLambda(tokens, maybeOpenParen, lambdaArgTypes, lambdaArgs, owner);
                        }
                        else
                        {
                            tokens.RestoreState(state);
                        }
                    }

                    root = this.Parse(tokens, owner);
                    if (root is Variable)
                    {
                        if (tokens.PopIfPresent(")"))
                        {
                            if (tokens.IsNext("=>"))
                            {

                                root = this.ParseLambda(tokens, firstToken, new AType[] { AType.Any(root.FirstToken) }, new Token[] { root.FirstToken }, owner);
                            }
                        }
                        else if (tokens.IsNext(","))
                        {
                            List<Token> lambdaArgs = new List<Token>() { root.FirstToken };
                            List<AType> lambdaArgTypes = new List<AType>() { AType.Any(root.FirstToken) };
                            Token comma = tokens.Peek();
                            while (tokens.PopIfPresent(","))
                            {
                                Token nextArg = tokens.Pop();
                                if (nextArg.Type != TokenType.WORD)
                                {
                                    throw new ParserException(comma, "Unexpected comma.");
                                }
                                lambdaArgTypes.Add(AType.Any(nextArg));
                                lambdaArgs.Add(nextArg);
                                comma = tokens.Peek();
                            }
                            tokens.PopExpected(")");

                            root = this.ParseLambda(tokens, firstToken, lambdaArgTypes, lambdaArgs, owner);
                        }
                        else
                        {
                            // This will purposely cause an unexpected token error
                            // since it's none of the above conditions.
                            tokens.PopExpected(")");
                        }
                    }
                    else
                    {
                        tokens.PopExpected(")");
                    }
                }
            }
            else
            {
                root = ParseEntityWithoutSuffixChain(tokens, owner);
            }
            bool anySuffixes = true;
            bool isPreviousADot = false;
            while (anySuffixes)
            {
                if (tokens.IsNext("."))
                {
                    isPreviousADot = true;
                    Token dotToken = tokens.Pop();
                    Token fieldToken = tokens.Pop();
                    // HACK alert: "class" is a valid field on a class.
                    // ParserVerifyIdentifier is invoked downstream for non-resolved fields.
                    if (fieldToken.Value != this.parser.Keywords.CLASS)
                    {
                        this.parser.VerifyIdentifier(fieldToken);
                    }
                    root = new DotField(root, dotToken, fieldToken, owner);
                }
                else if (tokens.IsNext("["))
                {
                    Token openBracket = tokens.Pop();
                    List<Expression> sliceComponents = new List<Expression>();
                    if (tokens.IsNext(":"))
                    {
                        sliceComponents.Add(null);
                    }
                    else
                    {
                        sliceComponents.Add(Parse(tokens, owner));
                    }

                    for (int i = 0; i < 2; ++i)
                    {
                        if (tokens.PopIfPresent(":"))
                        {
                            if (tokens.IsNext(":") || tokens.IsNext("]"))
                            {
                                sliceComponents.Add(null);
                            }
                            else
                            {
                                sliceComponents.Add(Parse(tokens, owner));
                            }
                        }
                    }

                    tokens.PopExpected("]");

                    if (sliceComponents.Count == 1)
                    {
                        Expression index = sliceComponents[0];
                        root = new BracketIndex(root, openBracket, index, owner);
                    }
                    else
                    {
                        root = new ListSlice(root, sliceComponents, openBracket, owner);
                    }
                }
                else if (tokens.IsNext("("))
                {
                    Token openParen = tokens.Pop();
                    List<Expression> args = new List<Expression>();
                    while (!tokens.PopIfPresent(")"))
                    {
                        if (args.Count > 0)
                        {
                            tokens.PopExpected(",");
                        }

                        args.Add(Parse(tokens, owner));
                    }
                    root = new FunctionCall(root, openParen, args, owner);
                }
                else if (tokens.IsNext(this.parser.Keywords.IS))
                {
                    Token isToken = tokens.Pop();
                    Token classToken = tokens.Pop();
                    string className = this.parser.PopClassNameWithFirstTokenAlreadyPopped(tokens, classToken);
                    root = new IsComparison(root, isToken, classToken, className, owner);
                }
                else if (isPreviousADot && this.parser.IsCSharpCompat && tokens.IsNext("<"))
                {
                    TokenStream.StreamState s = tokens.RecordState();
                    Token openBracket = tokens.Pop();
                    AType funcType = this.parser.TypeParser.TryParse(tokens);

                    List<AType> types = new List<AType>() { funcType };
                    if (funcType != null)
                    {
                        while (tokens.PopIfPresent(","))
                        {
                            types.Add(this.parser.TypeParser.Parse(tokens));
                        }
                    }

                    if (funcType == null)
                    {
                        anySuffixes = false;
                        tokens.RestoreState(s);
                    }
                    else if (!tokens.PopIfPresent(">") || !tokens.IsNext("("))
                    {
                        anySuffixes = false;
                        tokens.RestoreState(s);
                    }
                    else
                    {
                        // TODO(acrylic-conversion): do something with this types list.
                    }
                }
                else
                {
                    anySuffixes = false;
                }
            }
            return root;
        }

        private Expression ParseEntityWithoutSuffixChain(TokenStream tokens, Node owner)
        {
            tokens.EnsureNotEof();

            Token nextToken = tokens.Peek();
            string next = nextToken.Value;

            if (next == this.parser.Keywords.NULL) return new NullConstant(tokens.Pop(), owner);
            if (next == this.parser.Keywords.TRUE) return new BooleanConstant(tokens.Pop(), true, owner);
            if (next == this.parser.Keywords.FALSE) return new BooleanConstant(tokens.Pop(), false, owner);
            if (next == this.parser.Keywords.THIS) return new ThisKeyword(tokens.Pop(), owner);
            if (next == this.parser.Keywords.BASE) return new BaseKeyword(tokens.Pop(), owner);

            Token peekToken = tokens.Peek();
            if (next.StartsWith("'")) return new StringConstant(tokens.Pop(), StringConstant.ParseOutRawValue(peekToken), owner);
            if (next.StartsWith("\"")) return new StringConstant(tokens.Pop(), StringConstant.ParseOutRawValue(peekToken), owner);
            if (next == "@") // Raw strings (no escape sequences, a backslash is a literal backslash)
            {
                Token atToken = tokens.Pop();
                Token stringToken = tokens.Pop();
                char stringTokenChar = stringToken.Value[0];
                if (stringTokenChar != '"' && stringTokenChar != '\'') throw new ParserException(atToken, "Unexpected token: '@'");
                string stringValue = stringToken.Value.Substring(1, stringToken.Value.Length - 2);
                return new StringConstant(atToken, stringValue, owner);
            }
            if (next == this.parser.Keywords.NEW) return this.ParseInstantiate(tokens, owner);

            char firstChar = next[0];
            if (nextToken.Type == TokenType.WORD)
            {
                Token varToken = tokens.Pop();
                if (tokens.IsNext("=>"))
                {
                    return this.ParseLambda(
                        tokens,
                        varToken,
                        new AType[] { AType.Any() },
                        new Token[] { varToken },
                        owner);
                }
                else
                {
                    return new Variable(varToken, varToken.Value, owner);
                }
            }

            if (firstChar == '[' && nextToken.File.CompilationScope.IsCrayon)
            {
                Token bracketToken = tokens.PopExpected("[");
                List<Expression> elements = new List<Expression>();
                bool previousHasCommaOrFirst = true;
                while (!tokens.PopIfPresent("]"))
                {
                    if (!previousHasCommaOrFirst) tokens.PopExpected("]"); // throws appropriate error
                    elements.Add(Parse(tokens, owner));
                    previousHasCommaOrFirst = tokens.PopIfPresent(",");
                }
                return new ListDefinition(bracketToken, elements, AType.Any(), owner, false, null);
            }

            if (firstChar == '{' && nextToken.File.CompilationScope.IsCrayon)
            {
                Token braceToken = tokens.PopExpected("{");
                List<Expression> keys = new List<Expression>();
                List<Expression> values = new List<Expression>();
                bool previousHasCommaOrFirst = true;
                while (!tokens.PopIfPresent("}"))
                {
                    if (!previousHasCommaOrFirst) tokens.PopExpected("}"); // throws appropriate error
                    keys.Add(Parse(tokens, owner));
                    tokens.PopExpected(":");
                    values.Add(Parse(tokens, owner));
                    previousHasCommaOrFirst = tokens.PopIfPresent(",");
                }
                return new DictionaryDefinition(braceToken, AType.Any(), AType.Any(), keys, values, owner);
            }

            if (nextToken.Type == TokenType.NUMBER)
            {
                if (next.Contains("."))
                {
                    double floatValue;
                    if (double.TryParse(next, out floatValue))
                    {
                        return new FloatConstant(tokens.Pop(), floatValue, owner);
                    }
                    throw new ParserException(nextToken, "Invalid float literal.");
                }
                return new IntegerConstant(
                    tokens.Pop(),
                    IntegerConstant.ParseIntConstant(nextToken, next),
                    owner);
            }

            throw new ParserException(tokens.Peek(), "Encountered unexpected token: '" + tokens.PeekValue() + "'");
        }
    }
}
