using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class AcrylicTopLevelParser : AbstractTopLevelParser
    {
        private ParserContext parser;
        public AcrylicTopLevelParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal override TopLevelEntity Parse(TokenStream tokens, TopLevelEntity owner, FileScope fileScope)
        {
            throw new System.NotImplementedException();
        }

        internal override ImportStatement ParseImport(TokenStream tokens, FileScope fileScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
