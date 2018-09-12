using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class AcrylicExpressionParser : AbstractExpressionParser
    {
        public AcrylicExpressionParser(ParserContext parser)
            : base(parser)
        { }

        internal override Expression Parse(TokenStream tokens, Node owner)
        {
            throw new System.NotImplementedException();
        }
    }
}
