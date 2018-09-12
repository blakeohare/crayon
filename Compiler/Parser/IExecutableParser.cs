using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    interface IExecutableParser
    {
        Executable Parse(TokenStream tokens, bool simpleOnly, bool semicolonPresent, Node owner);
        IList<Executable> ParseBlock(TokenStream tokens, bool bracketsRequired, Node owner);
    }
}
