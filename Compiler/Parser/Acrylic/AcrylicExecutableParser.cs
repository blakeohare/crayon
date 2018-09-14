using Parser.ParseTree;

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
                            assignmentOpToken,
                            assignmentValue,
                            owner);
                    }
                }
            }

            tokens.RestoreState(tss);
            return null;
        }
    }
}
