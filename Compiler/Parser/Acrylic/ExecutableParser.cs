using System.Collections.Generic;
using Parser.ParseTree;

namespace Parser.Acrylic
{
    internal class ExecutableParser : IExecutableParser
    {
        public Executable Parse(TokenStream tokens, bool simpleOnly, bool semicolonPresent, Node owner)
        {
            throw new System.NotImplementedException();
        }

        public IList<Executable> ParseBlock(TokenStream tokens, bool bracketsRequired, Node owner)
        {
            throw new System.NotImplementedException();
        }
    }
}
