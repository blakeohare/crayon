using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class ContinueEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, ContinueStatement continueStatement)
        {
            buffer.Add(continueStatement.FirstToken, OpCode.CONTINUE, 0, 0);
        }
    }
}
