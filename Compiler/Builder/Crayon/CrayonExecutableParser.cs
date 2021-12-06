using Builder.ParseTree;

namespace Builder.Crayon
{
    internal class CrayonExecutableParser : AbstractExecutableParser
    {
        public CrayonExecutableParser(ParserContext parser) : base(parser) { }

        protected override bool IsForEachLoopParenthesisContents(TokenStream tokens)
        {
            TokenStream.StreamState tss = tokens.RecordState();
            try
            {
                if (tokens.PopIfWord() == null) return false;
                return tokens.IsNext(":");
            }
            finally
            {
                tokens.RestoreState(tss);
            }
        }

        protected override TypeTokenPair ParseForEachLoopIteratorVariable(TokenStream tokens, Node owner)
        {
            tokens.EnsureNotEof();
            Token variable = tokens.PopIfWord();
            if (variable == null) throw new ParserException(tokens.Peek(), "Expected variable here.");
            return new TypeTokenPair(AType.Any(variable), variable);
        }
    }
}
