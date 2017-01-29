using System;
using System.Collections.Generic;
using Pastel.Nodes;
using Common;

namespace Pastel
{
    internal class PastelParser
    {
        private static readonly HashSet<string> OP_TOKENS = new HashSet<string>(new string[] { "=", "+=", "*=", "-=", "&=", "|=", "^=" });
        private static readonly Executable[] EMPTY_CODE_BLOCK = new Executable[0];
        private IDictionary<string, bool> boolConstants;
        private IInlineImportCodeLoader importCodeLoader;

        public PastelParser(IDictionary<string, bool> boolConstants, IInlineImportCodeLoader importCodeLoader)
        {
            this.boolConstants = boolConstants;
            this.importCodeLoader = importCodeLoader;
        }

        public ICompilationEntity[] ParseText(string filename, string text)
        {
            TokenStream tokens = new TokenStream(Tokenizer.Tokenize(filename, text));
            List<ICompilationEntity> output = new List<ICompilationEntity>();
            while (tokens.HasMore)
            {
                switch (tokens.PeekValue())
                {
                    case "enum":
                        throw new NotImplementedException();
                    case "const":
                        throw new NotImplementedException();
                    case "global":
                        throw new NotImplementedException();
                    default:
                        output.Add(this.ParseFunctionDefinition(tokens));
                        break;
                }
            }
            return output.ToArray();
        }

        public FunctionDefinition ParseFunctionDefinition(TokenStream tokens)
        {
            PType returnType = PType.Parse(tokens);
            Token nameToken = EnsureTokenIsValidName(tokens.Pop(), "Expected function name");
            tokens.PopExpected("(");
            List<PType> argTypes = new List<PType>();
            List<Token> argNames = new List<Token>();
            while (!tokens.PopIfPresent(")"))
            {
                if (argTypes.Count > 0) tokens.PopExpected(",");
                argTypes.Add(PType.Parse(tokens));
                argNames.Add(EnsureTokenIsValidName(tokens.Pop(), "Invalid function arg name"));
            }
            List<Executable> code = this.ParseCodeBlock(tokens, true);

            return new FunctionDefinition(nameToken, returnType, argTypes, argNames, code);
        }

        public List<Executable> ParseCodeBlock(TokenStream tokens, bool curlyBracesRequired)
        {
            bool hasCurlyBrace = false;
            if (curlyBracesRequired)
            {
                hasCurlyBrace = true;
                tokens.PopExpected("{");
            }
            else
            {
                hasCurlyBrace = tokens.PopIfPresent("{");
            }
            List<Executable> code = new List<Executable>();

            if (hasCurlyBrace)
            {
                while (!tokens.PopIfPresent("}"))
                {
                    code.Add(ParseExecutable(tokens, false));
                }
            }
            else
            {
                code.Add(ParseExecutable(tokens, false));
            }
            
            return code;
        }

        public Executable ParseExecutable(TokenStream tokens, bool isForLoop)
        {
            if (!isForLoop)
            {
                switch (tokens.PeekValue())
                {
                    case "if": return ParseIfStatement(tokens);
                    case "for": return ParseForLoop(tokens);
                    case "while": return ParseWhileLoop(tokens);
                    case "switch": return ParseSwitchStatement(tokens);
                    case "break": return ParseBreak(tokens);
                    case "return": return ParseReturn(tokens);
                }
            }

            string token1 = tokens.FlatPeekAhead(0);
            string token2 = tokens.FlatPeekAhead(1);
            string token3 = tokens.FlatPeekAhead(2);
            if (
                (token2 == "<" && IsValidName(token1) && IsValidName(token3)) || // Type<G1, G2> var = ...
                (token3 == "=" && IsValidName(token1) && IsValidName(token2)) || // Type var = ...
                (token2 == "[" && token3 == "]" && IsValidName(token1)) || // Type[] ...
                (token3 == ";" && IsValidName(token1) && IsValidName(token2))) // Type var;
            {
                PType type = PType.Parse(tokens);
                Token variableName = EnsureTokenIsValidName(tokens.Pop(), "Invalid variable name");

                if (tokens.PopIfPresent(";"))
                {
                    return new VariableDeclaration(type, variableName, null, null);
                }

                Token equalsToken = tokens.PopExpected("=");
                Expression assignmentValue = ParseExpression(tokens);
                if (!isForLoop)
                {
                    tokens.PopExpected(";");
                }
                return new VariableDeclaration(type, variableName, equalsToken, assignmentValue);
            }

            Expression expression = ParseExpression(tokens);

            if (!isForLoop && tokens.PopIfPresent(";"))
            {
                return new ExpressionAsExecutable(expression);
            }

            if (isForLoop && (tokens.IsNext(";") || tokens.IsNext(",") || tokens.IsNext(")")))
            {
                return new ExpressionAsExecutable(expression);
            }

            if (OP_TOKENS.Contains(tokens.PeekValue()))
            {
                Token opToken = tokens.Pop();
                Expression assignmentValue = ParseExpression(tokens);

                if (!isForLoop && tokens.PopIfPresent(";"))
                {
                    return new Assignment(expression, opToken, assignmentValue);
                }

                if (isForLoop && (tokens.IsNext(";") || tokens.IsNext(",") || tokens.IsNext(")")))
                {
                    return new Assignment(expression, opToken, assignmentValue);
                }
            }

            tokens.PopExpected(";"); // Exhausted possibilities. This will crash intentionally.
            return null; // unreachable code
        }

        public IfStatement ParseIfStatement(TokenStream tokens)
        {
            Token ifToken = tokens.PopExpected("if");
            tokens.PopExpected("(");
            Expression condition = this.ParseExpression(tokens);
            tokens.PopExpected(")");
            IList<Executable> ifCode = this.ParseCodeBlock(tokens, false);
            Token elseToken = null;
            IList<Executable> elseCode = EMPTY_CODE_BLOCK;
            if (tokens.IsNext("else"))
            {
                elseToken = tokens.Pop();
                elseCode = this.ParseCodeBlock(tokens, false);
            }
            return new IfStatement(ifToken, condition, ifCode, elseToken, elseCode);
        }

        public ForLoop ParseForLoop(TokenStream tokens)
        {
            Token forToken = tokens.PopExpected("for");
            List<Executable> initCode = new List<Executable>();
            Expression condition = null;
            List<Executable> stepCode = new List<Executable>();
            tokens.PopExpected("(");
            if (!tokens.PopIfPresent(";"))
            {
                initCode.Add(this.ParseExecutable(tokens, true));
                while (!tokens.PopIfPresent(","))
                {
                    initCode.Add(this.ParseExecutable(tokens, true));
                }
                tokens.PopExpected(";");
            }

            if (!tokens.PopIfPresent(";"))
            {
                condition = this.ParseExpression(tokens);
                tokens.PopExpected(";");
            }

            if (!tokens.PopIfPresent(")"))
            {
                stepCode.Add(this.ParseExecutable(tokens, true));
                while (!tokens.PopIfPresent(","))
                {
                    stepCode.Add(this.ParseExecutable(tokens, true));
                }
                tokens.PopExpected(")");
            }

            List<Executable> code = this.ParseCodeBlock(tokens, false);
            return new ForLoop(forToken, initCode, condition, stepCode, code);
        }

        public WhileLoop ParseWhileLoop(TokenStream tokens)
        {
            Token whileToken = tokens.PopExpected("while");
            tokens.PopExpected("(");
            Expression condition = this.ParseExpression(tokens);
            tokens.PopExpected(")");
            List<Executable> code = this.ParseCodeBlock(tokens, false);
            return new WhileLoop(whileToken, condition, code);
        }

        public SwitchStatement ParseSwitchStatement(TokenStream tokens)
        {
            Token switchToken = tokens.PopExpected("switch");
            tokens.PopExpected("(");
            Expression condition = this.ParseExpression(tokens);
            tokens.PopExpected(")");
            tokens.PopExpected("{");

            List<SwitchStatement.SwitchChunk> chunks = new List<SwitchStatement.SwitchChunk>();
            while (!tokens.PopIfPresent("}"))
            {
                List<Expression> caseExpressions = new List<Expression>();
                List<Token> caseAndDefaultTokens = new List<Token>();
                bool thereAreCases = true;
                while (thereAreCases)
                {
                    switch (tokens.PeekValue())
                    {
                        case "case":
                            caseAndDefaultTokens.Add(tokens.Pop());
                            Expression caseExpression = this.ParseExpression(tokens);
                            tokens.PopExpected(":");
                            caseExpressions.Add(caseExpression);
                            break;

                        case "default":
                            caseAndDefaultTokens.Add(tokens.Pop());
                            tokens.PopExpected(":");
                            caseExpressions.Add(null);
                            break;

                        default:
                            thereAreCases = false;
                            break;
                    }
                }

                List<Executable> chunkCode = new List<Executable>();
                string next = tokens.PeekValue();
                while (next != "}" && next != "default" && next != "case")
                {
                    chunkCode.Add(this.ParseExecutable(tokens, false));
                    next = tokens.PeekValue();
                }

                chunks.Add(new SwitchStatement.SwitchChunk(caseAndDefaultTokens, caseExpressions, chunkCode));
            }
            
            return new SwitchStatement(switchToken, condition, chunks);
        }

        public BreakStatement ParseBreak(TokenStream tokens)
        {
            Token breakToken = tokens.PopExpected("break");
            tokens.PopExpected(";");
            return new BreakStatement(breakToken);
        }

        public ReturnStatement ParseReturn(TokenStream tokens)
        {
            Token returnToken = tokens.PopExpected("return");
            if (tokens.PopIfPresent(";"))
            {
                return new ReturnStatement(returnToken, null);
            }

            Expression expression = this.ParseExpression(tokens);
            tokens.PopExpected(";");
            return new ReturnStatement(returnToken, expression);
        }

        public Expression ParseExpression(TokenStream tokens)
        {
            return this.ParseBooleanCombination(tokens);
        }

        private Expression ParseOpChain(TokenStream tokens, HashSet<string> opsToLookFor, Func<TokenStream, Expression> fp)
        {
            Expression expression = fp(tokens);
            if (opsToLookFor.Contains(tokens.PeekValue()))
            {
                List<Expression> expressions = new List<Expression>() { expression };
                List<Token> ops = new List<Token>();
                while (opsToLookFor.Contains(tokens.PeekValue()))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(fp(tokens));
                }
                return new OpChain(expressions, ops);
            }
            return expression;
        }

        private static readonly HashSet<string> OPS_BOOLEAN_COMBINATION = new HashSet<string>(new string[] { "&&", "||" });
        private Expression ParseBooleanCombination(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_BOOLEAN_COMBINATION, this.ParseBitwise);
        }

        private static readonly HashSet<string> OPS_BITWISE = new HashSet<string>(new string[] { "&", "|", "^" });
        private Expression ParseBitwise(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_BITWISE, this.ParseEquality);
        }

        private static readonly HashSet<string> OPS_EQUALITY = new HashSet<string>(new string[] { "==", "!=" });
        private Expression ParseEquality(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_EQUALITY, this.ParseInequality);
        }

        private static readonly HashSet<string> OPS_INEQUALITY = new HashSet<string>(new string[] { "<", ">", "<=", ">=" });
        private Expression ParseInequality(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_INEQUALITY, this.ParseAddition);
        }

        private static readonly HashSet<string> OPS_ADDITION = new HashSet<string>(new string[] { "+", "-" });
        private Expression ParseAddition(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_ADDITION, this.ParseMultiplication);
        }

        private static readonly HashSet<string> OPS_MULTIPLICATION = new HashSet<string>(new string[] { "*", "/", "%" });
        private Expression ParseMultiplication(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_ADDITION, this.ParsePrefixes);
        }

        private Expression ParsePrefixes(TokenStream tokens)
        {
            if (tokens.IsNext("-") || tokens.IsNext("!"))
            {
                Token op = tokens.Pop();
                Expression root = this.ParsePrefixes(tokens);
                return new UnaryOp(op, root);
            }

            if (tokens.IsNext("("))
            {
                return this.ParseParenthesisOrCast(tokens);
            }

            return this.ParseEntity(tokens);
        }

        private Expression ParseParenthesisOrCast(TokenStream tokens)
        {
            Token parenthesis = tokens.Peek();
            if (tokens.PopIfPresent("("))
            {
                string token1 = tokens.PeekValue();
                string token2 = tokens.FlatPeekAhead(1);
                string token3 = tokens.FlatPeekAhead(2);
                // This is a super-lame way to check for a cast vs parenthesis-wrapped expression, but I can't think of anything better at the moment.
                bool isCast =
                    (token2 == "<" && (token1 == "List" || token1 == "Dictionary")) ||
                    (token2 == "[" && token3 == "]" && IsValidName(token1)) ||
                    (token2 == ")" && IsValidName(token1) && (token3 == "(" || IsValidName(token3)));
                if (isCast)
                {
                    PType castType = PType.Parse(tokens);
                    tokens.PopExpected(")");
                    Expression castedValue = this.ParsePrefixes(tokens);

                    return new CastExpression(parenthesis, castType, castedValue);
                }

                Expression expression = this.ParseExpression(tokens);
                tokens.PopExpected(")");
                return expression;
            }
            return this.ParseEntity(tokens);
        }

        private Expression ParseEntity(TokenStream tokens)
        {
            if (tokens.IsNext("new"))
            {
                Token newToken = tokens.Pop();
                string errMsg = "Invalid name for 'new' statement";
                PType typeToConstruct = PType.Parse(tokens);
                if (!tokens.IsNext("(")) tokens.PopExpected("("); // intentional error if not present.
                Expression constructorReference = new ConstructorReference(newToken, typeToConstruct);
                return this.ParseEntityChain(constructorReference, tokens);
            }

            Expression root = this.ParseEntityRoot(tokens);

            while (true)
            {
                switch (tokens.PeekValue())
                {
                    case ".":
                    case "[":
                    case "(":
                        root = this.ParseEntityChain(root, tokens);
                        break;
                    default:
                        return root;
                }
            }
        }

        private Expression ParseEntityRoot(TokenStream tokens)
        {
            string next = tokens.PeekValue();
            switch (next)
            {
                case "true":
                case "false":
                    return new InlineConstant(PType.BOOL, tokens.Pop(), next == "true");
                case "null":
                    return new InlineConstant(null, tokens.Pop(), null);
                case ".":
                    Token dotToken = tokens.Pop();
                    Token numToken = EnsureInteger(tokens.Pop());
                    string strValue = "0." + numToken.Value;
                    double dblValue;
                    if (!numToken.HasWhitespacePrefix && double.TryParse(strValue, out dblValue))
                    {
                        return new InlineConstant(PType.DOUBLE, dotToken, dblValue);
                    }
                    throw new ParserException(dotToken, "Unexpected '.'");

                default: break;
            }
            char firstChar = next[0];
            switch (firstChar)
            {
                case '\'':
                    return new InlineConstant(PType.CHAR, tokens.Pop(), Util.ConvertStringTokenToValue(next));
                case '"':
                    return new InlineConstant(PType.STRING, tokens.Pop(), Util.ConvertStringTokenToValue(next));
            }

            if (firstChar >= '0' && firstChar <= '9')
            {
                Token numToken = EnsureInteger(tokens.Pop());
                if (tokens.IsNext("."))
                {
                    Token dotToken = tokens.Pop();
                    if (dotToken.HasWhitespacePrefix) throw new ParserException(dotToken, "Unexpected '.'");
                    Token decimalToken = EnsureInteger(tokens.Pop());
                    if (decimalToken.HasWhitespacePrefix) throw new ParserException(decimalToken, "Unexpected '" + decimalToken.Value + "'");
                    double dblValue;
                    if (double.TryParse(numToken.Value + "." + decimalToken.Value, out dblValue))
                    {
                        return new InlineConstant(PType.DOUBLE, numToken, dblValue);
                    }
                    throw new ParserException(decimalToken, "Unexpected token.");
                }
                return new InlineConstant(PType.INT, numToken, int.Parse(numToken.Value));
            }

            if (IsValidName(tokens.PeekValue()))
            {
                return new Variable(tokens.Pop());
            }

            throw new ParserException(tokens.Peek(), "Unrecognized expression.");
        }

        private Expression ParseEntityChain(Expression root, TokenStream tokens)
        {
            switch (tokens.PeekValue())
            {
                case ".":
                    Token dotToken = tokens.Pop();
                    Token field = PastelParser.EnsureTokenIsValidName(tokens.Pop(), "Invalid field name");
                    return new DotField(root, dotToken, field);
                case "[":
                    Token openBracket = tokens.Pop();
                    Expression index = this.ParseExpression(tokens);
                    tokens.PopExpected("]");
                    return new BracketIndex(root, openBracket, index);
                case "(":
                    Token openParen = tokens.Pop();
                    List<Expression> args = new List<Expression>();
                    while (!tokens.PopIfPresent(")"))
                    {
                        if (args.Count > 0) tokens.PopExpected(",");
                        args.Add(this.ParseExpression(tokens));
                    }
                    return new FunctionInvocation(root, openParen, args);
                default:
                    throw new Exception();
            }
        }

        private Token EnsureInteger(Token token)
        {
            string value = token.Value;
            switch (value)
            {
                case "0":
                case "1":
                case "2":
                    // this is like 80% of cases.
                    return token;
            }
            char c;
            for (int i = value.Length - 1; i >= 0; --i)
            {
                c = value[i];
                if (c < '0' || c > '9')
                {
                    throw new ParserException(token, "Expected number");
                }
            }
            return token;
        }

        public static Token EnsureTokenIsValidName(Token token, string errorMessage)
        {
            if (IsValidName(token.Value))
            {
                return token;
            }
            throw new ParserException(token, errorMessage);
        }

        public static bool IsValidName(string value)
        {
            char c;
            for (int i = value.Length - 1; i >= 0; --i)
            {
                c = value[i];
                if (!(
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9' && i > 0) ||
                    (c == '_')))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
