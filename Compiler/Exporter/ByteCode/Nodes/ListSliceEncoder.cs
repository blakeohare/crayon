using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class ListSliceEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ListSlice listSlice, bool outputUsed)
        {
            ByteCodeCompiler.EnsureUsed(listSlice, outputUsed);
            bcc.CompileExpression(parser, buffer, listSlice.Root, true);

            Expression step = listSlice.Items[2];
            bool isStep1 = step is IntegerConstant && ((IntegerConstant)step).Value == 1;

            int serializeThese = isStep1 ? 2 : 3;
            for (int i = 0; i < serializeThese; ++i)
            {
                Expression item = listSlice.Items[i];
                if (item != null)
                {
                    bcc.CompileExpression(parser, buffer, item, true);
                }
            }

            bool firstIsPresent = listSlice.Items[0] != null;
            bool secondIsPresent = listSlice.Items[1] != null;

            buffer.Add(listSlice.BracketToken, OpCode.LIST_SLICE, new int[] { firstIsPresent ? 1 : 0, secondIsPresent ? 1 : 0, isStep1 ? 0 : 1 });
        }
    }
}
