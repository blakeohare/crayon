using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class NegativeSignEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, NegativeSign negativeSign, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(negativeSign, "This expression does nothing.");
            bcc.CompileExpression(parser, buffer, negativeSign.Root, true);
            buffer.Add(negativeSign.FirstToken, OpCode.NEGATIVE_SIGN);
        }
    }
}
