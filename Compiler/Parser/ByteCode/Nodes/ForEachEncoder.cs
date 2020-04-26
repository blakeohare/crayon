using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class ForEachEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ForEachLoop forEachLoop)
        {
            bcc.CompileExpression(parser, buffer, forEachLoop.IterationExpression, true);
            buffer.Add(
                forEachLoop.IterationExpression.FirstToken,
                OpCode.VERIFY_TYPE_IS_ITERABLE,
                forEachLoop.ListLocalId.ID,
                forEachLoop.IndexLocalId.ID);

            ByteBuffer body = new ByteBuffer();
            ByteBuffer body2 = new ByteBuffer();

            bcc.Compile(parser, body2, forEachLoop.Code);

            body.Add(
                forEachLoop.FirstToken,
                OpCode.ITERATION_STEP,
                body2.Size + 1,
                forEachLoop.IterationVariableId.ID,
                forEachLoop.IndexLocalId.ID,
                forEachLoop.ListLocalId.ID);

            body2.Add(null, OpCode.JUMP, -body2.Size - 2);
            body.Concat(body2);

            body.ResolveBreaks();
            body.ResolveContinues();

            buffer.Concat(body);
        }
    }
}
