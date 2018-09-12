using Parser.ParseTree;

namespace Parser
{
    internal abstract class AbstractTopLevelParser
    {
        internal abstract ImportStatement ParseImport(TokenStream tokens, FileScope fileScope);
        internal abstract TopLevelEntity Parse(TokenStream tokens, TopLevelEntity owner, FileScope fileScope);
    }
}
