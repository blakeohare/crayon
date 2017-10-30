using System;
using System.Collections.Generic;
using Common;
using Parser.ParseTree;

namespace Parser
{
    public class ExpressionParser
    {
        private ParserContext parser;
        public ExpressionParser(ParserContext parser)
        {
            this.parser = parser;
        }

        public Expression Parse(TokenStream tokens, TopLevelConstruct owner)
        {
            Dictionary<string, Annotation> annotations = null;
            if (tokens.IsNext("@"))
            {
                annotations = new Dictionary<string, Annotation>();
                while (tokens.IsNext("@"))
                {
                    Annotation annotation = this.parser.AnnotationParser.ParseAnnotation(tokens);
                    annotations[annotation.Type] = annotation;
                }
            }
            Expression output = ParseTernary(tokens, owner);
            output.Annotations = annotations;
            return output;
        }

        private Expression ParseTernary(TokenStream tokens, TopLevelConstruct owner)
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

        private Expression ParseNullCoalescing(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression root = ParseBooleanCombination(tokens, owner);
            if (tokens.PopIfPresent("??"))
            {
                Expression secondaryExpression = ParseNullCoalescing(tokens, owner);
                return new NullCoalescer(root, secondaryExpression, owner);
            }
            return root;
        }

        private Expression ParseBooleanCombination(TokenStream tokens, TopLevelConstruct owner)
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

        private Expression ParseBitwiseOp(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseEqualityComparison(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "|" || next == "&" || next == "^")
            {
                Token bitwiseToken = tokens.Pop();
                Expression rightExpr = ParseBitwiseOp(tokens, owner);
                return new BinaryOpChain(expr, bitwiseToken, rightExpr, owner);
            }
            return expr;
        }

        private Expression ParseEqualityComparison(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseInequalityComparison(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "==" || next == "!=")
            {
                Token equalityToken = tokens.Pop();
                Expression rightExpr = ParseEqualityComparison(tokens, owner);
                return new BinaryOpChain(expr, equalityToken, rightExpr, owner);
            }
            return expr;
        }

        private Expression ParseInequalityComparison(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseBitShift(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "<" || next == ">" || next == "<=" || next == ">=")
            {
                // Don't allow chaining of inqeualities
                Token opToken = tokens.Pop();
                Expression rightExpr = ParseBitShift(tokens, owner);
                return new BinaryOpChain(expr, opToken, rightExpr, owner);
            }
            return expr;
        }

        private Expression ParseBitShift(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseAddition(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "<<" || next == ">>")
            {
                Token opToken = tokens.Pop();
                Expression rightExpr = ParseBitShift(tokens, owner);
                return new BinaryOpChain(expr, opToken, rightExpr, owner);
            }
            return expr;
        }

        private static readonly HashSet<string> ADDITION_OPS = new HashSet<string>("+ -".Split(' '));
        private static readonly HashSet<string> MULTIPLICATION_OPS = new HashSet<string>("* / %".Split(' '));
        private static readonly HashSet<string> NEGATE_OPS = new HashSet<string>("! -".Split(' '));

        private Expression ParseAddition(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseMultiplication(tokens, owner);
            string next = tokens.PeekValue();
            while (ADDITION_OPS.Contains(next))
            {
                Token op = tokens.Pop();
                Expression right = ParseMultiplication(tokens, owner);
                expr = new BinaryOpChain(expr, op, right, owner);
                next = tokens.PeekValue();
            }
            return expr;
        }

        private Expression ParseMultiplication(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseNegate(tokens, owner);
            string next = tokens.PeekValue();
            while (MULTIPLICATION_OPS.Contains(next))
            {
                Token op = tokens.Pop();
                Expression right = ParseNegate(tokens, owner);
                expr = new BinaryOpChain(expr, op, right, owner);
                next = tokens.PeekValue();
            }
            return expr;
        }

        private Expression ParseNegate(TokenStream tokens, TopLevelConstruct owner)
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

        private Expression ParseExponents(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression expr = ParseIncrement(tokens, owner);
            string next = tokens.PeekValue();
            if (next == "**")
            {
                Token op = tokens.Pop();
                Expression right = ParseNegate(tokens, owner);
                expr = new BinaryOpChain(expr, op, right, owner);
            }
            return expr;
        }

        private Expression ParseIncrement(TokenStream tokens, TopLevelConstruct owner)
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

        private static readonly HashSet<char> VARIABLE_STARTER = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_$".ToCharArray());

        private Expression ParseInstantiate(TokenStream tokens, TopLevelConstruct owner)
        {
            Token newToken = tokens.PopExpected(this.parser.Keywords.NEW);
            Token classNameToken = tokens.Pop();
            string name = this.parser.PopClassNameWithFirstTokenAlreadyPopped(tokens, classNameToken);

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

            return new Instantiate(newToken, classNameToken, name, args, owner);
        }

        private Expression ParseEntity(TokenStream tokens, TopLevelConstruct owner)
        {
            Expression root;
            if (tokens.PopIfPresent("("))
            {
                root = this.Parse(tokens, owner);
                tokens.PopExpected(")");
            }
            else
            {
                root = ParseEntityWithoutSuffixChain(tokens, owner);
            }
            bool anySuffixes = true;
            while (anySuffixes)
            {
                if (tokens.IsNext("."))
                {
                    Token dotToken = tokens.Pop();
                    Token stepToken = tokens.Pop();
                    // HACK alert: "class" is a valid field on a class.
                    // ParserVerifyIdentifier is invoked downstream for non-resolved fields.
                    if (stepToken.Value != this.parser.Keywords.CLASS)
                    {
                        this.parser.VerifyIdentifier(stepToken);
                    }
                    root = new DotStep(root, dotToken, stepToken, owner);
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
                else
                {
                    anySuffixes = false;
                }
            }
            return root;
        }

        private Expression ParseEntityWithoutSuffixChain(TokenStream tokens, TopLevelConstruct owner)
        {
            string next = tokens.PeekValue();

            if (next == null) tokens.ThrowEofException();
            if (next == this.parser.Keywords.NULL) return new NullConstant(tokens.Pop(), owner);
            if (next == this.parser.Keywords.TRUE) return new BooleanConstant(tokens.Pop(), true, owner);
            if (next == this.parser.Keywords.FALSE) return new BooleanConstant(tokens.Pop(), false, owner);

            Token peekToken = tokens.Peek();
            if (next.StartsWith("'")) return new StringConstant(tokens.Pop(), StringConstant.ParseOutRawValue(peekToken), owner);
            if (next.StartsWith("\"")) return new StringConstant(tokens.Pop(), StringConstant.ParseOutRawValue(peekToken), owner);
            if (next == this.parser.Keywords.NEW) return this.ParseInstantiate(tokens, owner);

            char firstChar = next[0];
            if (VARIABLE_STARTER.Contains(firstChar))
            {
                Token varToken = tokens.Pop();
                return new Variable(varToken, varToken.Value, owner);
            }

            if (firstChar == '[')
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
                return new ListDefinition(bracketToken, elements, owner);
            }

            if (firstChar == '{')
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
                return new DictionaryDefinition(braceToken, keys, values, owner);
            }

            if (next.Length > 2 && next.Substring(0, 2) == "0x")
            {
                Token intToken = tokens.Pop();
                int intValue = IntegerConstant.ParseIntConstant(intToken, intToken.Value);
                return new IntegerConstant(intToken, intValue, owner);
            }

            if (ParserContext.IsInteger(next))
            {
                Token numberToken = tokens.Pop();
                string numberValue = numberToken.Value;

                if (tokens.IsNext("."))
                {
                    Token decimalToken = tokens.Pop();
                    if (decimalToken.HasWhitespacePrefix)
                    {
                        throw new ParserException(decimalToken, "Decimals cannot have whitespace before them.");
                    }

                    Token afterDecimal = tokens.Pop();
                    if (afterDecimal.HasWhitespacePrefix) throw new ParserException(afterDecimal, "Cannot have whitespace after the decimal.");
                    if (!ParserContext.IsInteger(afterDecimal.Value)) throw new ParserException(afterDecimal, "Decimal must be followed by an integer.");

                    numberValue += "." + afterDecimal.Value;

                    double floatValue = FloatConstant.ParseValue(numberToken, numberValue);
                    return new FloatConstant(numberToken, floatValue, owner);
                }

                int intValue = IntegerConstant.ParseIntConstant(numberToken, numberToken.Value);
                return new IntegerConstant(numberToken, intValue, owner);
            }

            if (tokens.IsNext("."))
            {
                Token dotToken = tokens.PopExpected(".");
                string numberValue = "0.";
                Token postDecimal = tokens.Pop();
                if (postDecimal.HasWhitespacePrefix || !ParserContext.IsInteger(postDecimal.Value))
                {
                    throw new ParserException(dotToken, "Unexpected dot.");
                }

                numberValue += postDecimal.Value;

                double floatValue;
                if (Util.ParseDouble(numberValue, out floatValue))
                {
                    return new FloatConstant(dotToken, floatValue, owner);
                }

                throw new ParserException(dotToken, "Invalid float literal.");
            }

            throw new ParserException(tokens.Peek(), "Encountered unexpected token: '" + tokens.PeekValue() + "'");
        }
    }
}
