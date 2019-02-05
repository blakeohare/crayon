using System.Collections.Generic;

namespace Parser.ParseTree
{
    public interface ICodeContainer
    {
        List<Lambda> Lambdas { get; }
        HashSet<string> ArgumentNameLookup { get; }
    }
}
