using Parser;
using Parser.ParseTree;
using System.Collections.Generic;

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
            List<int> args = new List<int>() { listDef.Items.Length };
            listDef.ResolvedType.ListItemType.BuildEncoding(args);
            buffer.Add(listDef.FirstToken, OpCode.DEF_LIST, args.ToArray());
        }
    }
}
