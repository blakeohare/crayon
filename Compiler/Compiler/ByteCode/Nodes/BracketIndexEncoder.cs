using Builder.ParseTree;

namespace Builder.ByteCode.Nodes
{
    internal static class BracketIndexEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, BracketIndex bracketIndex, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(bracketIndex, "This expression does nothing.");
            bcc.CompileExpression(parser, buffer, bracketIndex.Root, true);
            bcc.CompileExpression(parser, buffer, bracketIndex.Index, true);
            buffer.Add(bracketIndex.BracketToken, OpCode.INDEX);
        }
    }
}
