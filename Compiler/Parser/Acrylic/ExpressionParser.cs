using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class ExpressionParser : IExpressionParser
    {
        private ParserContext parser;
        public ExpressionParser(ParserContext parser)
        {
            this.parser = parser;
        }

        public Expression Parse(TokenStream tokens, Node owner)
        {
            throw new System.NotImplementedException();
        }
    }
}
