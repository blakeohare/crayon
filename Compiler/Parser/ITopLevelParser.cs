using Parser.ParseTree;

namespace Parser
{
    interface ITopLevelParser
    {
        ImportStatement ParseImport(TokenStream tokens, FileScope fileScope);
        TopLevelEntity Parse(TokenStream tokens, TopLevelEntity owner, FileScope fileScope);
    }
}
