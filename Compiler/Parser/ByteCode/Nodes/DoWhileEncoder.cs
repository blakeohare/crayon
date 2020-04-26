using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class DoWhileEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, DoWhileLoop doWhileLoop)
        {
            ByteBuffer loopBody = new ByteBuffer();
            bcc.Compile(parser, loopBody, doWhileLoop.Code);
            loopBody.ResolveContinues(true); // continues should jump to the condition, hence the true.

            ByteBuffer condition = new ByteBuffer();
            bcc.CompileExpression(parser, condition, doWhileLoop.Condition, true);
            loopBody.Concat(condition);
            loopBody.Add(doWhileLoop.Condition.FirstToken, OpCode.JUMP_IF_TRUE, -loopBody.Size - 1);
            loopBody.ResolveBreaks();

            buffer.Concat(loopBody);
        }
    }
}
