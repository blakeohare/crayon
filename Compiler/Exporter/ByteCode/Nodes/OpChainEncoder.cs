using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class OpChainEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, OpChain opChain, bool outputUsed)
        {
            if (!outputUsed)
            {
                if (opChain.OpToken.Value == "==")
                {
                    throw new ParserException(opChain.OpToken, "'==' cannot be used like this. Did you mean to use just a single '=' instead?");
                }
                throw new ParserException(opChain, "This expression isn't valid here.");
            }

            bcc.CompileExpressionList(parser, buffer, new Expression[] { opChain.Left, opChain.Right }, true);


            switch (opChain.OpTEMP)
            {
                case Ops.EQUALS:
                case Ops.NOT_EQUALS:
                    buffer.Add(opChain.OpToken, OpCode.EQUALS, opChain.OpTEMP == Ops.EQUALS ? 0 : 1);
                    break;

                default:
                    buffer.Add(opChain.OpToken, OpCode.BINARY_OP, (int)opChain.OpTEMP);
                    break;
            }

            if (!outputUsed)
            {
                buffer.Add(null, OpCode.POP);
            }
        }
    }
}
