using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class ForLoopEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ForLoop forLoop)
        {
            bcc.Compile(parser, buffer, forLoop.Init);

            ByteBuffer codeBuffer = new ByteBuffer();
            bcc.Compile(parser, codeBuffer, forLoop.Code);
            codeBuffer.ResolveContinues(true); // resolve continues as jump-to-end before you add the step instructions.
            bcc.Compile(parser, codeBuffer, forLoop.Step);

            ByteBuffer forBuffer = new ByteBuffer();
            bcc.CompileExpression(parser, forBuffer, forLoop.Condition, true);
            forBuffer.Add(forLoop.Condition.FirstToken, OpCode.JUMP_IF_FALSE, codeBuffer.Size + 1); // +1 to go past the jump I'm about to add.

            forBuffer.Concat(codeBuffer);
            forBuffer.Add(null, OpCode.JUMP, -forBuffer.Size - 1);

            forBuffer.ResolveBreaks();

            buffer.Concat(forBuffer);
        }
    }
}
