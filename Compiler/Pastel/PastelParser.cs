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

        private IDictionary<string, object> constants;

        private IInlineImportCodeLoader importCodeLoader;

        public PastelParser(
            IDictionary<string, object> constants,
            IInlineImportCodeLoader importCodeLoader)
        {
            this.constants = constants;
            this.importCodeLoader = importCodeLoader;
        }

        private object GetConstant(string name, object defaultValue)
        {
            object output;
            if (this.constants.TryGetValue(name, out output))
            {
                return output;
            }
            return defaultValue;
        }

        internal bool GetParseTimeBooleanConstant(string name)
        {
            return (bool)this.GetConstant(name, false);
        }

        internal int GetParseTimeIntegerConstant(string name)
        {
            return (int)this.GetConstant(name, 0);
        }

        internal string GetParseTimeStringConstant(string name)
        {
            return (string)this.GetConstant(name, "");
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
                        output.Add(this.ParseEnumDefinition(tokens));
                        break;

                    case "const":
                        output.Add(this.ParseConstDefinition(tokens));
                        break;

                    case "struct":
                        output.Add(this.ParseStructDefinition(tokens));
                        break;

                    default:
                        output.Add(this.ParseFunctionDefinition(tokens));
                        break;
                }
            }
            return output.ToArray();
        }

        public Executable[] ParseImportedCode(string path)
        {
            path = path.Replace(".cry", ".pst"); // lol TODO: fix this in the .pst code.
            string code = this.importCodeLoader.LoadCode(path);
            TokenStream tokens = new TokenStream(Tokenizer.Tokenize(path, code));
            List<Executable> output = new List<Executable>();
            while (tokens.HasMore)
            {
                this.ParseCodeLine(output, tokens);
            }
            return output.ToArray();
        }

        public VariableDeclaration ParseConstDefinition(TokenStream tokens)
        {
            Token constToken = tokens.PopExpected("const");
            VariableDeclaration assignment = this.ParseAssignmentWithNewFirstToken(constToken, tokens);
            assignment.IsConstant = true;
            return assignment;
        }

        private VariableDeclaration ParseAssignmentWithNewFirstToken(Token newToken, TokenStream tokens)
        {
            Executable executable = this.ParseExecutable(tokens, false);
            VariableDeclaration assignment = executable as VariableDeclaration;
            if (assignment == null)
            {
                throw new ParserException(newToken, "Expected an assignment here.");
            }
            assignment.FirstToken = newToken;
            return assignment;
        }

        public EnumDefinition ParseEnumDefinition(TokenStream tokens)
        {
            Token enumToken = tokens.PopExpected("enum");
            Token nameToken = EnsureTokenIsValidName(tokens.Pop(), "Invalid name for an enum.");
            List<Token> valueTokens = new List<Token>();
            List<Expression> valueExpressions = new List<Expression>();
            tokens.PopExpected("{");
            bool first = true;
            while (!tokens.PopIfPresent("}"))
            {
                if (!first)
                {
                    tokens.PopExpected(",");
                }
                else
                {
                    first = false;
                }

                if (tokens.PopIfPresent("}"))
                {
                    break;
                }

                Token valueToken = EnsureTokenIsValidName(tokens.Pop(), "Invalid name for a enum value.");
                valueTokens.Add(valueToken);
                if (tokens.PopIfPresent("="))
                {
                    Expression value = ParseExpression(tokens);
                    valueExpressions.Add(value);
                }
                else
                {
                    valueExpressions.Add(null);
                }
            }

            return new EnumDefinition(enumToken, nameToken, valueTokens, valueExpressions);
        }

        public StructDefinition ParseStructDefinition(TokenStream tokens)
        {
            Token structToken = tokens.PopExpected("struct");
            Token nameToken = EnsureTokenIsValidName(tokens.Pop(), "Invalid struct name");
            List<PType> structFieldTypes = new List<PType>();
            List<Token> structFieldNames = new List<Token>();
            tokens.PopExpected("{");
            while (!tokens.PopIfPresent("}"))
            {
                PType fieldType = PType.Parse(tokens);
                Token fieldName = EnsureTokenIsValidName(tokens.Pop(), "Invalid struct field name");
                structFieldTypes.Add(fieldType);
                structFieldNames.Add(fieldName);
                tokens.PopExpected(";");
            }
            return new StructDefinition(structToken, nameToken, structFieldTypes, structFieldNames);
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
                    this.ParseCodeLine(code, tokens);
                }
            }
            else
            {
                this.ParseCodeLine(code, tokens);
            }

            return code;
        }

        private void ParseCodeLine(List<Executable> codeOut, TokenStream tokens)
        {
            Executable line = ParseExecutable(tokens, false);
            Executable[] lines = null;
            if (line is ExpressionAsExecutable)
            {
                lines = ((ExpressionAsExecutable)line).ImmediateResolveMaybe(this);
            }

            if (lines == null)
            {
                codeOut.Add(line);
            }
            else
            {
                codeOut.AddRange(lines);
            }
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

            string token1 = tokens.PeekAhead(0);
            string token2 = tokens.PeekAhead(1);
            string token3 = tokens.PeekAhead(2);

            string DEBUG_HELP = (token1 + " " + (token2 ?? "") + " " + (token3 ?? "")).Trim();
            if (DEBUG_HELP == "set me to stuff")
            {

            }

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

        public Executable ParseIfStatement(TokenStream tokens)
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

            if (condition is InlineConstant)
            {
                InlineConstant ic = (InlineConstant)condition;
                if (ic.Value is bool)
                {
                    return new ExecutableBatch(ifToken, (bool)ic.Value ? ifCode : elseCode);
                }
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
                while (tokens.PopIfPresent(","))
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
                while (tokens.PopIfPresent(","))
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
                    this.ParseCodeLine(chunkCode, tokens);
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
            return this.ParseOpChain(tokens, OPS_INEQUALITY, this.ParseBitShift);
        }

        private Expression ParseBitShift(TokenStream tokens)
        {
            Expression left = this.ParseAddition(tokens);
            Token bitShift = tokens.PopBitShiftHackIfPresent();
            if (bitShift != null)
            {
                Expression right = this.ParseAddition(tokens);
                return new OpChain(new Expression[] { left, right }, new Token[] { bitShift });
            }
            return left;
        }

        private static readonly HashSet<string> OPS_ADDITION = new HashSet<string>(new string[] { "+", "-" });
        private Expression ParseAddition(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_ADDITION, this.ParseMultiplication);
        }

        private static readonly HashSet<string> OPS_MULTIPLICATION = new HashSet<string>(new string[] { "*", "/", "%" });
        private Expression ParseMultiplication(TokenStream tokens)
        {
            return this.ParseOpChain(tokens, OPS_MULTIPLICATION, this.ParsePrefixes);
        }

        private Expression ParsePrefixes(TokenStream tokens)
        {
            string next = tokens.PeekValue();
            if (next == "-" || next == "!")
            {
                Token op = tokens.Pop();
                Expression root = this.ParsePrefixes(tokens);
                return new UnaryOp(op, root);
            }

            return this.ParseIncrementOrCast(tokens);
        }

        private Expression ParseIncrementOrCast(TokenStream tokens)
        {
            Token prefix = null;
            if (tokens.IsNext("++") || tokens.IsNext("--"))
            {
                prefix = tokens.Pop();
            }

            Expression expression;
            int tokenIndex = tokens.SnapshotState();
            if (prefix == null &&
                tokens.PeekValue() == "(" &&
                IsValidName(tokens.PeekAhead(1)))
            {
                Token parenthesis = tokens.Pop();
                PType castType = PType.TryParse(tokens);
                if (castType != null && tokens.PopIfPresent(")"))
                {
                    expression = this.ParseIncrementOrCast(tokens);
                    return new CastExpression(parenthesis, castType, expression);
                }
                tokens.RevertState(tokenIndex);
            }

            expression = this.ParseEntity(tokens);

            if (prefix != null)
            {
                expression = new InlineIncrement(prefix, prefix, expression, true);
            }

            if (tokens.IsNext("++") || tokens.IsNext("--"))
            {
                expression = new InlineIncrement(expression.FirstToken, tokens.Pop(), expression, false);
            }

            return expression;
        }

        private Expression ParseEntity(TokenStream tokens)
        {
            if (tokens.IsNext("new"))
            {
                Token newToken = tokens.Pop();
                PType typeToConstruct = PType.Parse(tokens);
                if (!tokens.IsNext("(")) tokens.PopExpected("("); // intentional error if not present.
                Expression constructorReference = new ConstructorReference(newToken, typeToConstruct);
                return this.ParseEntityChain(constructorReference, tokens);
            }

            if (tokens.PopIfPresent("("))
            {
                Expression expression = this.ParseExpression(tokens);
                tokens.PopExpected(")");
                return this.ParseOutEntitySuffixes(tokens, expression);
            }

            Expression root = this.ParseEntityRoot(tokens);
            return this.ParseOutEntitySuffixes(tokens, root);
        }

        private Expression ParseOutEntitySuffixes(TokenStream tokens, Expression root)
        {
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
                    return new InlineConstant(PType.NULL, tokens.Pop(), null);
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
                case '@':
                    Token atToken = tokens.PopExpected("@");
                    Token compileTimeFunction = EnsureTokenIsValidName(tokens.Pop(), "Expected compile time function name.");
                    if (!tokens.IsNext("(")) tokens.PopExpected("(");
                    return new CompileTimeFunctionReference(atToken, compileTimeFunction);
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
                    return new FunctionInvocation(root, openParen, args).MaybeImmediatelyResolve(this);
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
