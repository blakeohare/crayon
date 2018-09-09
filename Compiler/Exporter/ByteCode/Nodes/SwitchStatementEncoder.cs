using Parser;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.ByteCode.Nodes
{
    internal static class SwitchStatementEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, SwitchStatement switchStatement)
        {
            bool isInt = switchStatement.UsesIntegers;

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
                        if (isInt)
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

            int defaultOffsetLength = defaultChunkId == -1
                ? chunkBuffer.Size
                : chunkIdsToOffsets[defaultChunkId];

            List<int> args = new List<int>() { defaultOffsetLength };
            if (isInt)
            {
                foreach (int caseValue in integersToChunkIds.Keys.OrderBy(_ => _))
                {
                    int chunkId = integersToChunkIds[caseValue];
                    int offset = chunkIdsToOffsets[chunkId];
                    args.Add(caseValue);
                    args.Add(offset);
                }
            }
            else
            {
                foreach (string caseValue in stringsToChunkIds.Keys.OrderBy(_ => _))
                {
                    int chunkId = stringsToChunkIds[caseValue];
                    int offset = chunkIdsToOffsets[chunkId];
                    args.Add(parser.GetStringConstant(caseValue));
                    args.Add(offset);
                }
            }

            buffer.Add(
                switchStatement.FirstToken,
                isInt ? OpCode.SWITCH_INT : OpCode.SWITCH_STRING,
                args.ToArray());

            buffer.Concat(chunkBuffer);
        }
    }
}
