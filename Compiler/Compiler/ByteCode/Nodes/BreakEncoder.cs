using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class BreakEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, BreakStatement breakStatement)
        {
            buffer.Add(breakStatement.FirstToken, OpCode.BREAK, 0, 0);
        }
    }
}
