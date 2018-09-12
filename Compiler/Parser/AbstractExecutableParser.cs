using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    internal abstract class AbstractExecutableParser
    {
        internal abstract Executable Parse(TokenStream tokens, bool simpleOnly, bool semicolonPresent, Node owner);
        internal abstract IList<Executable> ParseBlock(TokenStream tokens, bool bracketsRequired, Node owner);
    }
}
