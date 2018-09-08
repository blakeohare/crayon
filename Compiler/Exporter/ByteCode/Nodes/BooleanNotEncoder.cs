using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class BooleanNotEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, BooleanNot boolNot, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(boolNot, "Cannot have this expression here.");

            bcc.CompileExpression(parser, buffer, boolNot.Root, true);
            buffer.Add(boolNot.FirstToken, OpCode.BOOLEAN_NOT);
        }
    }
}
