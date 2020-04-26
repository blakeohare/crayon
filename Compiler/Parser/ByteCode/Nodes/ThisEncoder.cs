using Parser.ParseTree;

namespace Parser.ByteCode.Nodes
{
    internal static class ThisEncoder
    {
        public static void Compile(ParserContext parser, ByteBuffer buffer, ThisKeyword thisKeyword, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(thisKeyword, "This expression doesn't do anything.");

            buffer.Add(thisKeyword.FirstToken, OpCode.THIS);
        }
    }
}
