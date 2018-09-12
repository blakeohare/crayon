using Parser.ParseTree;

namespace Parser
{
    interface IExpressionParser
    {
        Expression Parse(TokenStream tokens, Node owner);
    }
}
