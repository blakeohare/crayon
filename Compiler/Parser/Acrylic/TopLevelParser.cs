using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class TopLevelParser : ITopLevelParser
    {
        private ParserContext parser;
        public TopLevelParser(ParserContext parser)
        {
            this.parser = parser;
        }

        public TopLevelEntity Parse(TokenStream tokens, TopLevelEntity owner, FileScope fileScope)
        {
            throw new System.NotImplementedException();
        }

        public ImportStatement ParseImport(TokenStream tokens, FileScope fileScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
