using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class WhileLoopEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, WhileLoop whileLoop)
        {
            ByteBuffer loopBody = new ByteBuffer();
            bcc.Compile(parser, loopBody, whileLoop.Code);
            ByteBuffer condition = new ByteBuffer();
            bcc.CompileExpression(parser, condition, whileLoop.Condition, true);

            condition.Add(whileLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, loopBody.Size + 1);
            condition.Concat(loopBody);
            condition.Add(null, OpCode.JUMP, -condition.Size - 1);

            condition.ResolveBreaks();
            condition.ResolveContinues();

            buffer.Concat(condition);
        }
    }
}
