using Parser.ParseTree;
using System;

namespace Parser.Crayon
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

        protected override Tuple<AType, Token> ParseForEachLoopIteratorVariable(TokenStream tokens, Node owner)
        {
            tokens.EnsureNotEof();
            Token variable = tokens.PopIfWord();
            if (variable == null) throw new ParserException(tokens.Peek(), "Expected variable here.");
            return new Tuple<AType, Token>(null, variable);
        }
    }
}
