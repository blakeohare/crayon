using Parser;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.ByteCode.Nodes
{
    internal static class TryStatementEncoder
    {
        public static void Compile(ByteCodeCompiler bcc, ParserContext parser, ByteBuffer buffer, TryStatement tryStatement)
        {
            ByteBuffer tryCode = new ByteBuffer();
            bcc.Compile(parser, tryCode, tryStatement.TryBlock);

            if (tryStatement.TryBlock.Length > 0 && tryStatement.TryBlock[0] is TryStatement)
            {
                // If the try block begins with another try block, that'll mess with the EsfToken metadata
                // which is declared at the beginning of the try block's PC and is singular.
                // Get around this limitation by tacking on a noop (JUMP +0) in this reasonably rare edge case.
                tryCode.AddFrontSlow(null, OpCode.JUMP, 0);
            }

            List<ByteBuffer> catchBlocks = new List<ByteBuffer>();

            for (int i = 0; i < tryStatement.CatchBlocks.Length; ++i)
            {
                TryStatement.CatchBlock catchBlock = tryStatement.CatchBlocks[i];
                ByteBuffer catchBlockBuffer = new ByteBuffer();
                bcc.Compile(parser, catchBlockBuffer, catchBlock.Code);
                catchBlocks.Add(catchBlockBuffer);
            }

            ByteBuffer finallyCode = new ByteBuffer();
            bcc.Compile(parser, finallyCode, tryStatement.FinallyBlock);
            finallyCode.ResolveBreaksAndContinuesForFinally(false);
            finallyCode.Add(null, OpCode.FINALLY_END,
                new int[] {
                    // First 2 args are the same as a BREAK op code
                    // Last 2 args are the same as a CONTINUE op code
                    // These are all 0 and are resolved into their final values in the same pass as BREAK and CONTINUE
                    0, // break flag 0|1|2
                    0, // break offset
                    0, // continue flag 0|1|2
                    0 // continue offset
                });


            // All user code is now compiled and offsets are sort of known.
            // Now build a lookup jump router thingy for all the catch blocks, if any.

            ByteBuffer allCatchBlocks = new ByteBuffer();
            if (catchBlocks.Count > 0)
            {
                /*
                    It'll look something like this...
                    0   EXCEPTION_HANDLED_TOGGLE true
                    1   JUMP_IF_EXCEPTION_IS_TYPE offset varId type1, type2, ...
                    2   JUMP_IF_EXCEPTION_IS_TYPE offset varId type3
                    3   EXCEPTION_HANDLED_TOGGLE false
                    4   JUMP [to finally]

                    5   catch block 1
                        ...
                    22  last line in catch block 1
                    23  JUMP [to finally]

                    24  catch block 2...
                        ...
                    72  last line in catch block 2

                    73  finally code begins...
                */

                // Add jumps to the end of each catch block to jump to the end.
                // Going in reverse order is easier for this.
                int totalSize = 0;
                for (int i = catchBlocks.Count - 1; i >= 0; --i)
                {
                    ByteBuffer catchBlockBuffer = catchBlocks[i];
                    if (totalSize > 0) // omit the last block since a JUMP 0 is pointless.
                    {
                        catchBlockBuffer.Add(null, OpCode.JUMP, totalSize);
                    }
                    totalSize += catchBlockBuffer.Size;
                }

                // Now generate the header. This is also done backwards since it's easier.
                ByteBuffer exceptionSortHeader = new ByteBuffer();

                int offset = 2 // EXCEPTION_HANDLED_TOGGLE + final JUMP
                    + catchBlocks.Count - 1; // remaining jump instructions to jump over

                // Add all the JUMP_IF_EXCEPTION_OF_TYPE instructions.
                for (int i = 0; i < catchBlocks.Count; ++i)
                {
                    TryStatement.CatchBlock cb = tryStatement.CatchBlocks[i];
                    ByteBuffer cbByteBuffer = catchBlocks[i];
                    int variableId = cb.VariableLocalScopeId.ID;

                    // for each catch block insert a type-check-jump
                    List<int> typeCheckArgs = new List<int>() { offset, variableId }; // first arg is offset, second is variable ID (or -1), successive args are all class ID's
                    typeCheckArgs.AddRange(cb.TypeClasses.Select<ClassDefinition, int>(cd => cd.ClassID));
                    exceptionSortHeader.Add(null, OpCode.JUMP_IF_EXCEPTION_OF_TYPE, typeCheckArgs.ToArray());

                    // add the block to the running total
                    offset += cbByteBuffer.Size;

                    // ...but subtract 1 for the JUMP_IF_EXCEPTION_OF_TYPE you just added.
                    offset -= 1;
                }
                exceptionSortHeader.Add(null, OpCode.EXCEPTION_HANDLED_TOGGLE, 0);
                exceptionSortHeader.Add(null, OpCode.JUMP, totalSize);

                allCatchBlocks.Add(null, OpCode.EXCEPTION_HANDLED_TOGGLE, 1);
                allCatchBlocks.Concat(exceptionSortHeader);
                foreach (ByteBuffer catchBlock in catchBlocks)
                {
                    allCatchBlocks.Concat(catchBlock);
                }
            }

            int tryBegin = buffer.Size;
            buffer.Concat(tryCode);
            buffer.Add(null, OpCode.JUMP, allCatchBlocks.Size);
            buffer.Concat(allCatchBlocks);
            buffer.ResolveBreaksAndContinuesForFinally(true);

            buffer.Concat(finallyCode);

            int offsetToCatch = tryCode.Size + 1;
            int offsetToFinally = offsetToCatch + allCatchBlocks.Size;
            buffer.SetEsfToken(tryBegin, offsetToCatch, offsetToFinally);
        }
    }
}
