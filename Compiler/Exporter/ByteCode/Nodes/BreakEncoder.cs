using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class BreakEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, BreakStatement breakStatement)
        {
            buffer.Add(breakStatement.FirstToken, OpCode.BREAK, 0, 0);
        }
    }
}
