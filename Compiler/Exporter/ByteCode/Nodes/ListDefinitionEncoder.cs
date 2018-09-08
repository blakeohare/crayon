using Parser;
using Parser.ParseTree;

namespace Exporter.ByteCode.Nodes
{
    internal static class ListDefinitionEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, ListDefinition listDef, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(listDef, "List allocation made without storing it. This is likely a mistake.");
            foreach (Expression item in listDef.Items)
            {
                bcc.CompileExpression(parser, buffer, item, true);
            }
            buffer.Add(listDef.FirstToken, OpCode.DEF_LIST, listDef.Items.Length);
        }
    }
}
