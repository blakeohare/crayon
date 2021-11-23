using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class TernaryEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, Ternary ternary, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(ternary, outputUsed);

            bcc.CompileExpression(parser, buffer, ternary.Condition, true);
            ByteBuffer trueBuffer = new ByteBuffer();
            bcc.CompileExpression(parser, trueBuffer, ternary.TrueValue, true);
            ByteBuffer falseBuffer = new ByteBuffer();
            bcc.CompileExpression(parser, falseBuffer, ternary.FalseValue, true);
            trueBuffer.Add(null, OpCode.JUMP, falseBuffer.Size);
            buffer.Add(ternary.Condition.FirstToken, OpCode.JUMP_IF_FALSE, trueBuffer.Size);
            buffer.Concat(trueBuffer);
            buffer.Concat(falseBuffer);
        }
    }
}
