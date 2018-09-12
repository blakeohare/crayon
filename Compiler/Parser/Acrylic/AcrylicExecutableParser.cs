using System.Collections.Generic;
using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class AcrylicExecutableParser : AbstractExecutableParser
    {
        private ParserContext parser;
        public AcrylicExecutableParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal override Executable Parse(TokenStream tokens, bool simpleOnly, bool semicolonPresent, Node owner)
        {
            throw new System.NotImplementedException();
        }

        internal override IList<Executable> ParseBlock(TokenStream tokens, bool bracketsRequired, Node owner)
        {
            throw new System.NotImplementedException();
        }
    }
}
