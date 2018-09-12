using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class AcrylicExpressionParser : AbstractExpressionParser
    {
        private ParserContext parser;
        public AcrylicExpressionParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal override Expression Parse(TokenStream tokens, Node owner)
        {
            throw new System.NotImplementedException();
        }
    }
}
