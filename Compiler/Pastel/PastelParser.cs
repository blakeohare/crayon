using System;
using System.Collections.Generic;
using Pastel.Nodes;

namespace Pastel
{
    internal class PastelParser
    {
        private static readonly HashSet<string> OP_TOKENS = new HashSet<string>(new string[] { "=", "+=", "*=", "-=", "&=", "|=", "^=" });
        private static readonly Executable[] EMPTY_CODE_BLOCK = new Executable[0];

        private IDictionary<string, object> constants;

        private IInlineImportCodeLoader importCodeLoader;

        // TODO: Get rid of this somehow. The parser logic should be relatively stateless.
        // However, this value is set at the beginning and end of a code owner parsing
        // and the reference is passed to all instantiations of parse tree objects below.
        private ICompilationEntity currentCodeOwner = null;

        private PastelContext context;

        public PastelParser(
            PastelContext context,
            IDictionary<string, object> constants,
            IInlineImportCodeLoader importCodeLoader)
        {
            this.context = context;
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

                    case "@":
                        Token atToken = tokens.Pop();
                        ICompilationEntity[] inlinedEntities = this.ParseTopLevelInlineImport(atToken, tokens);
                        output.AddRange(inlinedEntities);
                        break;

                    default:
                        output.Add(this.ParseFunctionDefinition(tokens));
                        break;
                }
            }
            return output.ToArray();
        }

        private ICompilationEntity[] ParseTopLevelInlineImport(Token atToken, TokenStream tokens)
        {
            string sourceFile = null;
            string functionName = tokens.PeekValue();
            switch (functionName)
            {
                case "import":
                    tokens.PopExpected("import");
                    tokens.PopExpected("(");
                    Token stringToken = tokens.Pop();
                    tokens.PopExpected(")");
                    tokens.PopExpected(";");
                    sourceFile = PastelUtil.ConvertStringTokenToValue(stringToken.Value);
                    break;

                case "importIfTrue":
                case "importIfFalse":
                    tokens.Pop();
                    tokens.PopExpected("(");
                    Token constantExpression = tokens.Pop();
                    string constantValue = PastelUtil.ConvertStringTokenToValue(constantExpression.Value);
                    tokens.PopExpected(",");
                    Token pathToken = tokens.Pop();
                    tokens.PopExpected(")");
                    tokens.PopExpected(";");
                    object value = this.GetConstant(constantValue, false);
                    if (!(value is bool)) value = false;
                    bool valueBool = (bool)value;
                    if (functionName == "importIfFalse")
                    {
                        valueBool = !valueBool;
                    }

                    if (valueBool)
                    {
                        sourceFile = PastelUtil.ConvertStringTokenToValue(pathToken.Value);
                    }
                    break;

                default:
                    // intentional crash...
                    tokens.PopExpected("import");
                    break;
            }

            if (sourceFile != null)
            {
                string code = this.importCodeLoader.LoadCode(sourceFile);
                return this.ParseText(sourceFile, code);
            }

            return new ICompilationEntity[0];
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
            EnumDefinition enumDef = new EnumDefinition(enumToken, nameToken, this.context);
            this.currentCodeOwner = enumDef;
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

            enumDef.InitializeValues(valueTokens, valueExpressions);
            this.currentCodeOwner = null;
            return enumDef;
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
            return new StructDefinition(structToken, nameToken, structFieldTypes, structFieldNames, this.context);
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
            FunctionDefinition funcDef = new FunctionDefinition(nameToken, returnType, argTypes, argNames, this.context);
            this.currentCodeOwner = funcDef;
            List<Executable> code = this.ParseCodeBlock(tokens, true);
            this.currentCodeOwner = null;
            funcDef.Code = code.ToArray();
            return funcDef;
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

            int currentState = tokens.SnapshotState();
            PType assignmentType = PType.TryParse(tokens);
            if (assignmentType != null && tokens.HasMore && IsValidName(tokens.PeekValue()))
            {
                Token variableName = EnsureTokenIsValidName(tokens.Pop(), "Invalid variable name");

                if (tokens.PopIfPresent(";"))
                {
                    return new VariableDeclaration(assignmentType, variableName, null, null, this.context);
                }

                Token equalsToken = tokens.PopExpected("=");
                Expression assignmentValue = ParseExpression(tokens);
                if (!isForLoop)
                {
                    tokens.PopExpected(";");
                }
                return new VariableDeclaration(assignmentType, variableName, equalsToken, assignmentValue, this.context);
            }
            tokens.RevertState(currentState);

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
                Expression constructorReference = new ConstructorReference(newToken, typeToConstruct, this.currentCodeOwner);
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
                    return new InlineConstant(PType.BOOL, tokens.Pop(), next == "true", this.currentCodeOwner);
                case "null":
                    return new InlineConstant(PType.NULL, tokens.Pop(), null, this.currentCodeOwner);
                case ".":
                    Token dotToken = tokens.Pop();
                    Token numToken = tokens.Pop();
                    EnsureInteger(tokens.Pop(), false, false);
                    string strValue = "0." + numToken.Value;
                    double dblValue;
                    if (!numToken.HasWhitespacePrefix && double.TryParse(strValue, out dblValue))
                    {
                        return new InlineConstant(PType.DOUBLE, dotToken, dblValue, this.currentCodeOwner);
                    }
                    throw new ParserException(dotToken, "Unexpected '.'");

                default: break;
            }
            char firstChar = next[0];
            switch (firstChar)
            {
                case '\'':
                    return new InlineConstant(PType.CHAR, tokens.Pop(), PastelUtil.ConvertStringTokenToValue(next), this.currentCodeOwner);
                case '"':
                    return new InlineConstant(PType.STRING, tokens.Pop(), PastelUtil.ConvertStringTokenToValue(next), this.currentCodeOwner);
                case '@':
                    Token atToken = tokens.PopExpected("@");
                    Token compileTimeFunction = EnsureTokenIsValidName(tokens.Pop(), "Expected compile time function name.");
                    if (!tokens.IsNext("(")) tokens.PopExpected("(");
                    return new CompileTimeFunctionReference(atToken, compileTimeFunction, this.currentCodeOwner);
            }

            if (firstChar >= '0' && firstChar <= '9')
            {
                Token numToken = tokens.Pop();
                if (tokens.IsNext("."))
                {
                    EnsureInteger(numToken, false, false);
                    Token dotToken = tokens.Pop();
                    if (dotToken.HasWhitespacePrefix) throw new ParserException(dotToken, "Unexpected '.'");
                    Token decimalToken = tokens.Pop();
                    EnsureInteger(decimalToken, false, false);
                    if (decimalToken.HasWhitespacePrefix) throw new ParserException(decimalToken, "Unexpected '" + decimalToken.Value + "'");
                    double dblValue;
                    if (double.TryParse(numToken.Value + "." + decimalToken.Value, out dblValue))
                    {
                        return new InlineConstant(PType.DOUBLE, numToken, dblValue, this.currentCodeOwner);
                    }
                    throw new ParserException(decimalToken, "Unexpected token.");
                }
                else
                {
                    int numValue = EnsureInteger(numToken, true, true);
                    return new InlineConstant(PType.INT, numToken, numValue, this.currentCodeOwner);
                }
            }

            if (IsValidName(tokens.PeekValue()))
            {
                return new Variable(tokens.Pop(), this.currentCodeOwner);
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

        // This function became really weird for legitimate reasons, but deseparately needs to be written (or rather,
        // the places where this is called need to be rewritten).
        private int EnsureInteger(Token token, bool allowHex, bool calculateValue)
        {
            string value = token.Value;
            char c;
            if (allowHex && value.StartsWith("0x"))
            {
                value = value.Substring(2).ToLower();
                int num = 0;
                for (int i = 0; i < value.Length; ++i)
                {
                    num *= 16;
                    c = value[i];
                    if (c >= '0' && c <= '9')
                    {
                        num += c - '0';
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        num += c - 'a' + 10;
                    }
                }
                return calculateValue ? num : 0;
            }
            for (int i = value.Length - 1; i >= 0; --i)
            {
                c = value[i];
                if (c < '0' || c > '9')
                {
                    throw new ParserException(token, "Expected number");
                }
            }
            if (!calculateValue) return 0;
            int output;
            if (!int.TryParse(value, out output))
            {
                throw new ParserException(token, "Integer is too big.");
            }
            return output;
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
