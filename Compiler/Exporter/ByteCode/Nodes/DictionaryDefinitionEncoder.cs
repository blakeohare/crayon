using Parser;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Exporter.ByteCode.Nodes
{
    internal static class DictionaryDefinitionEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, DictionaryDefinition dictDef, bool outputUsed)
        {
            if (!outputUsed) throw new ParserException(dictDef, "Cannot have a dictionary all by itself.");

            int itemCount = dictDef.Keys.Length;
            List<Expression> expressionList = new List<Expression>();
            for (int i = 0; i < itemCount; ++i)
            {
                expressionList.Add(dictDef.Keys[i]);
                expressionList.Add(dictDef.Values[i]);
            }

            bcc.CompileExpressionList(parser, buffer, expressionList, true);

            buffer.Add(dictDef.FirstToken, OpCode.DEF_DICTIONARY, itemCount);
        }
    }
}
