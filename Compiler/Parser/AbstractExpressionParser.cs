using Parser.ParseTree;

namespace Parser
{
    internal abstract class AbstractExpressionParser
    {
        internal abstract Expression Parse(TokenStream tokens, Node owner);
    }
}
