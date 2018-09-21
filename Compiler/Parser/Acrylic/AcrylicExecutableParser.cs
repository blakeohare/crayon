using Parser.ParseTree;
using System;

namespace Parser.Acrylic
{
    internal class AcrylicExecutableParser : AbstractExecutableParser
    {
        public AcrylicExecutableParser(ParserContext parser)
            : base(parser)
        { }

        protected override Assignment MaybeParseTypedVariableDeclaration(TokenStream tokens, Node owner)
        {
            TokenStream.StreamState tss = tokens.RecordState();
            AType possiblyAVariableDeclaration = this.parser.TypeParser.TryParse(tokens);
            if (possiblyAVariableDeclaration != null)
            {
                AType variableDeclarationType = possiblyAVariableDeclaration;
                Token variableToken = tokens.PopIfWord();
                if (variableToken != null)
                {
                    // This is a variable declaration.
                    Expression assignmentValue = null;
                    Token assignmentOpToken = tokens.Peek();
                    if (tokens.PopIfPresent("="))
                    {
                        assignmentValue = this.parser.ExpressionParser.Parse(tokens, owner);
                    }
                    else if (tokens.PopIfPresent(";"))
                    {
                        assignmentValue = new NullConstant(tokens.Pop(), owner);
                    }

                    if (assignmentValue != null)
                    {
                        return new Assignment(
                            new Variable(variableToken, variableToken.Value, owner),
                            variableDeclarationType,
                            assignmentOpToken,
                            assignmentValue,
                            owner);
                    }
                }
            }

            tokens.RestoreState(tss);
            return null;
        }

        protected override bool IsForEachLoopParenthesisContents(TokenStream tokens)
        {
            TokenStream.StreamState tss = tokens.RecordState();
            try
            {
                AType type = this.parser.TypeParser.TryParse(tokens);
                if (type == null) return false;
                if (tokens.PopIfWord() == null) return false;
                return tokens.IsNext(":");
            }
            finally
            {
                tokens.RestoreState(tss);
            }
        }

        protected override Tuple<AType, Token> ParseForEachLoopIteratorVariable(TokenStream tokens, Node owner)
        {
            AType type = this.parser.TypeParser.Parse(tokens);
            tokens.EnsureNotEof();
            Token variable = tokens.PopIfWord();
            if (variable == null) throw new ParserException(tokens.Peek(), "Expected variable here.");
            return new Tuple<AType, Token>(type, variable);
        }
    }
}
