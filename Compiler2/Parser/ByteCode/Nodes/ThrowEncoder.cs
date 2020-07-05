using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class ThrowEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ThrowStatement throwStatement)
        {
            bcc.CompileExpression(parser, buffer, throwStatement.Expression, true);
            buffer.Add(throwStatement.FirstToken, OpCode.THROW);
        }
    }
}
