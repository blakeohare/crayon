using Parser;
using Parser.ParseTree;
using System.Collections.Generic;

namespace Exporter.ByteCode.Nodes
{
    internal static class SwitchStatementEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, SwitchStatement switchStatement)
        {
            bcc.CompileExpression(parser, buffer, switchStatement.Condition, true);

            ByteBuffer chunkBuffer = new ByteBuffer();

            Dictionary<int, int> chunkIdsToOffsets = new Dictionary<int, int>();
            Dictionary<int, int> integersToChunkIds = new Dictionary<int, int>();
            Dictionary<string, int> stringsToChunkIds = new Dictionary<string, int>();

            int defaultChunkId = -1;
            foreach (SwitchStatement.Chunk chunk in switchStatement.Chunks)
            {
                int chunkId = chunk.ID;

                if (chunk.Cases.Length == 1 && chunk.Cases[0] == null)
                {
                    defaultChunkId = chunkId;
                }
                else
                {
                    foreach (Expression expression in chunk.Cases)
                    {
                        if (switchStatement.UsesIntegers)
                        {
                            integersToChunkIds[((IntegerConstant)expression).Value] = chunkId;
                        }
                        else
                        {
                            stringsToChunkIds[((StringConstant)expression).Value] = chunkId;
                        }
                    }
                }

                chunkIdsToOffsets[chunkId] = chunkBuffer.Size;

                bcc.Compile(parser, chunkBuffer, chunk.Code);
            }

            chunkBuffer.ResolveBreaks();

            int switchId = parser.RegisterByteCodeSwitch(chunkIdsToOffsets, integersToChunkIds, stringsToChunkIds, switchStatement.UsesIntegers);

            int defaultOffsetLength = defaultChunkId == -1
                ? chunkBuffer.Size
                : chunkIdsToOffsets[defaultChunkId];

            buffer.Add(switchStatement.FirstToken, switchStatement.UsesIntegers ? OpCode.SWITCH_INT : OpCode.SWITCH_STRING, switchId, defaultOffsetLength);
            buffer.Concat(chunkBuffer);
        }

        internal static ByteBuffer BuildTables(ParserContext parser)
        {
            ByteBuffer output = new ByteBuffer();
            List<Dictionary<int, int>> intSwitches = parser.GetIntegerSwitchStatements();
            for (int i = 0; i < intSwitches.Count; ++i)
            {
                List<int> args = new List<int>();
                Dictionary<int, int> lookup = intSwitches[i];
                foreach (int key in lookup.Keys)
                {
                    int offset = lookup[key];
                    args.Add(key);
                    args.Add(offset);
                }

                output.Add(null, OpCode.BUILD_SWITCH_INT, args.ToArray());
            }

            List<Dictionary<string, int>> stringSwitches = parser.GetStringSwitchStatements();
            for (int i = 0; i < stringSwitches.Count; ++i)
            {
                Dictionary<string, int> lookup = stringSwitches[i];
                foreach (string key in lookup.Keys)
                {
                    int offset = lookup[key];
                    output.Add(null, OpCode.BUILD_SWITCH_STRING, key, i, offset);
                }
            }
            return output;
        }
    }
}
