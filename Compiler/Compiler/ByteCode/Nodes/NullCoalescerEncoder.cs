using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class NullCoalescerEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, NullCoalescer nullCoalescer, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(nullCoalescer, outputUsed);

            bcc.CompileExpression(parser, buffer, nullCoalescer.PrimaryExpression, true);
            ByteBuffer secondaryExpression = new ByteBuffer();
            bcc.CompileExpression(parser, secondaryExpression, nullCoalescer.SecondaryExpression, true);
            buffer.Add(nullCoalescer.FirstToken, OpCode.POP_IF_NULL_OR_JUMP, secondaryExpression.Size);
            buffer.Concat(secondaryExpression);
        }
    }
}
