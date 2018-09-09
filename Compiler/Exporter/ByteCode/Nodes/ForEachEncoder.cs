using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class ForEachEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ForEachLoop forEachLoop)
        {
            buffer.Add(null, OpCode.LITERAL, parser.GetIntConstant(0));
            bcc.CompileExpression(parser, buffer, forEachLoop.IterationExpression, true);
            buffer.Add(forEachLoop.IterationExpression.FirstToken, OpCode.VERIFY_TYPE_IS_ITERABLE);

            buffer.SetLastValueStackDepthOffset(2);

            ByteBuffer body = new ByteBuffer();
            ByteBuffer body2 = new ByteBuffer();

            bcc.Compile(parser, body2, forEachLoop.Code);

            body.Add(forEachLoop.FirstToken, OpCode.ITERATION_STEP, body2.Size + 1, forEachLoop.IterationVariableId.ID);

            body2.Add(null, OpCode.JUMP, -body2.Size - 2);
            body.Concat(body2);

            body.ResolveBreaks();
            body.ResolveContinues();

            buffer.Concat(body);
            buffer.Add(null, OpCode.POP); // list
            buffer.Add(null, OpCode.POP); // index
            buffer.SetLastValueStackDepthOffset(-2);
        }
    }
}
