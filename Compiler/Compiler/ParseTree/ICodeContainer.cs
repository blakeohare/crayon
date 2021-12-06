using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal interface ICodeContainer
    {
        List<Lambda> Lambdas { get; }
        HashSet<string> ArgumentNameLookup { get; }
    }
}
