using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class BooleanCombinationEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, BooleanCombination boolComb, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(boolComb, "Cannot have this expression here.");

            ByteBuffer rightBuffer = new ByteBuffer();
            Expression[] expressions = boolComb.Expressions;
            bcc.CompileExpression(parser, rightBuffer, expressions[expressions.Length - 1], true);
            for (int i = expressions.Length - 2; i >= 0; --i)
            {
                ByteBuffer leftBuffer = new ByteBuffer();
                bcc.CompileExpression(parser, leftBuffer, expressions[i], true);
                Token op = boolComb.Ops[i];
                if (op.Value == "&&")
                {
                    leftBuffer.Add(op, OpCode.JUMP_IF_FALSE_NO_POP, rightBuffer.Size);
                }
                else
                {
                    leftBuffer.Add(op, OpCode.JUMP_IF_TRUE_NO_POP, rightBuffer.Size);
                }
                leftBuffer.Concat(rightBuffer);
                rightBuffer = leftBuffer;
            }

            buffer.Concat(rightBuffer);
        }
    }
}
