using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.Pastel.Nodes;

namespace Crayon.Pastel
{
    internal class Parser
    {
        private static readonly HashSet<string> OP_TOKENS = new HashSet<string>(new string[] { "=", "+=", "*=", "-=", "&=", "|=", "^=" });
        private static readonly Executable[] EMPTY_CODE_BLOCK = new Executable[0];

        public FunctionDefinition[] Parse(TokenStream tokens)
        {
            List<FunctionDefinition> functions = new List<FunctionDefinition>();
            while (tokens.HasMore)
            {
                functions.Add(this.ParseFunctionDefinition(tokens));
            }
            return functions.ToArray();
        }

        private FunctionDefinition ParseFunctionDefinition(TokenStream tokens)
        {
            PType returnType = PType.Parse(tokens);
            Token nameToken = EnsureTokenIsValidName(tokens.Pop(), "Expected function name");
            tokens.PopExpected("(");
            List<PType> argTypes = new List<PType>();
            List<Token> argNames = new List<Token>();
            while (tokens.PopIfPresent(")"))
            {
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
            } else
            {
                hasCurlyBrace = tokens.PopIfPresent("{");
            }
            List<Executable> code = new List<Executable>();

            if (hasCurlyBrace)
            {
                while (!tokens.PopIfPresent("}"))
                {
                    code.Add(ParseExecutable(tokens, true));
                }
            }
            else
            {
                code.Add(ParseExecutable(tokens, true));
            }

            if (hasCurlyBrace)
            {
                tokens.PopExpected("}");
            }
            return code;
        }

        public Executable ParseExecutable(TokenStream tokens, bool isForLoop)
        {
            if (!isForLoop) {
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
            throw new Exception();
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
            throw new Exception();
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
