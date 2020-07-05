using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal interface ICodeContainer
    {
        List<Lambda> Lambdas { get; }
        HashSet<string> ArgumentNameLookup { get; }
    }
}
