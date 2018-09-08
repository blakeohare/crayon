using Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exporter.ByteCode
{
    public class ByteBuffer
    {
        private class ByteRow
        {
            public int[] ByteCode { get; set; }
            public Token Token { get; set; }
            public string StringArg { get; set; }
            public ByteCodeEsfToken EsfToken { get; set; }
            public int ValueStackSizeChange { get; set; }
        }

        private List<ByteRow> rows = new List<ByteRow>();

        public OpCode LastOp
        {
            get { return (OpCode)this.rows[this.rows.Count - 1].ByteCode[0]; }
        }

        public int Size { get { return this.rows.Count; } }

        public void Concat(ByteBuffer other)
        {
            this.rows.AddRange(other.rows);
        }

        public void Add(Token token, OpCode op, string stringValue, params int[] args)
        {
            List<int> nums = new List<int>(args.Length + 1) { (int)op };
            nums.AddRange(args);
            int[] byteCode = nums.ToArray();

            this.rows.Add(new ByteRow()
            {
                ByteCode = byteCode,
                StringArg = stringValue,
                Token = token
            });
        }

        public void Add(Token token, OpCode op, params int[] args)
        {
            this.Add(token, op, null, args);
        }

        public void AddFrontSlow(Token token, OpCode op, params int[] args)
        {
            List<int> nums = new List<int>(args.Length + 1) { (int)op };
            nums.AddRange(args);
            int[] byteCode = nums.ToArray();

            this.rows.Insert(0, new ByteRow()
            {
                ByteCode = byteCode,
                Token = token,
            });
        }

        public List<int[]> ToIntList()
        {
            return new List<int[]>(this.rows.Select<ByteRow, int[]>(row => row.ByteCode));
        }

        public List<string> ToStringList()
        {
            return new List<string>(this.rows.Select<ByteRow, string>(row => row.StringArg));
        }

        public List<Token> ToTokenList()
        {
            return new List<Token>(this.rows.Select<ByteRow, Token>(row => row.Token));
        }

        public void ResolveContinues()
        {
            this.ResolveContinues(false);
        }

        // Continues should be resolved into JUMPs that go to the beginning of the current byte code buffer
        // If resolveAsJumpToEnd is true, it'll do the opposite. This hack is used by for loop, where the
        // step condition must be run before returning to the top.
        public void ResolveContinues(bool resolveAsJumpToEnd)
        {
            int size = this.Size;
            int[] byteCode;
            for (int i = 0; i < size; ++i)
            {
                byteCode = this.rows[i].ByteCode;
                switch ((OpCode)byteCode[0])
                {
                    case OpCode.CONTINUE:
                    case OpCode.FINALLY_END:
                        int index = (OpCode)byteCode[0] == OpCode.CONTINUE ? 1 : 3;
                        if (byteCode[index] == 0)
                        {
                            byteCode[index] = 1;
                            byteCode[index + 1] = resolveAsJumpToEnd
                                ? size - i - 1
                                : -i - 1;
                        }
                        break;
                }
            }
        }

        // Breaks should be resolved into JUMPS that go to the end of the current byte code buffer.
        public void ResolveBreaks()
        {
            int size = this.Size;
            int[] byteCode;
            for (int i = 0; i < size; ++i)
            {
                byteCode = this.rows[i].ByteCode;
                switch ((OpCode)byteCode[0])
                {
                    case OpCode.BREAK:
                    case OpCode.FINALLY_END:
                        if (byteCode[1] == 0)
                        {
                            byteCode[1] = 1;
                            byteCode[2] = size - i - 1;
                        }
                        break;
                }
            }
        }

        // Try catch code can be resolved with this function at any time
        // Finally code MUST be resolved right before adding the FINALLY_END op to the byte buffer.
        public void ResolveBreaksAndContinuesForFinally(bool isInTryCatch)
        {
            // If it's in the finally, then set status as 1 and jump to the last instruction
            // If it's in the try/catch, just set the status to 2 which will indicate use ESF data
            int[] byteCode;
            int size = this.Size;
            for (int i = 0; i < size; ++i)
            {
                byteCode = this.rows[i].ByteCode;
                switch ((OpCode)byteCode[0])
                {
                    case OpCode.BREAK:
                    case OpCode.CONTINUE:
                    case OpCode.FINALLY_END:
                        if (isInTryCatch)
                        {
                            // Set the jump type to 2 which means just use the finally offset in the ESF data.
                            byteCode[1] = 2;
                            if ((OpCode)byteCode[0] == OpCode.FINALLY_END)
                            {
                                byteCode[3] = 2;
                            }
                        }
                        else
                        {
                            // Set the jump to go to the end of the byte buffer. This is right before
                            // the FINALLY_END op has been added to this buffer.
                            byteCode[1] = 1;
                            byteCode[2] = size - i - 1;
                            if ((OpCode)byteCode[0] == OpCode.FINALLY_END)
                            {
                                byteCode[3] = 1;
                                byteCode[4] = byteCode[2];
                            }
                        }
                        break;
                }
            }
        }

        public void OptimizeJumps()
        {
            int offset;
            for (int i = 0; i < this.Size; ++i)
            {
                // TODO: optimize jump-if's as well (but don't follow other jump-if's
                if (this.rows[i].ByteCode[0] == (int)OpCode.JUMP)
                {
                    offset = this.rows[i].ByteCode[1];
                    if (this.rows[i + offset + 1].ByteCode[0] == (int)OpCode.JUMP)
                    {
                        this.rows[i].ByteCode[1] = this.FindFinalPc(i) - i - 1;
                    }
                }
            }
        }

        private int FindFinalPc(int startPc)
        {
            int currentPc = startPc;
            while (this.rows[currentPc].ByteCode[0] == (int)OpCode.JUMP)
            {
                currentPc = currentPc + this.rows[currentPc].ByteCode[1] + 1;
            }
            return currentPc;
        }

        public void SetEsfToken(int index, int catchOffset, int finallyOffset)
        {
            this.rows[index].EsfToken = new ByteCodeEsfToken()
            {
                ExceptionSortPcOffsetFromTry = catchOffset,
                FinallyPcOffsetFromTry = finallyOffset,
            };
        }

        public int[] GetFinalizedEsfData()
        {
            List<int> esfData = new List<int>();
            int size = this.Size;
            ByteRow row;
            for (int pc = 0; pc < size; ++pc)
            {
                row = this.rows[pc];
                if (row.EsfToken != null)
                {
                    esfData.Add(pc);
                    esfData.Add(pc + row.EsfToken.ExceptionSortPcOffsetFromTry);
                    esfData.Add(pc + row.EsfToken.FinallyPcOffsetFromTry);
                }
            }
            return esfData.ToArray();
        }

        public int[] GetFinalizedValueStackDepthData()
        {
            List<int> vsdData = new List<int>();
            int size = this.Size;
            ByteRow row;
            for (int pc = 0; pc < size; ++pc)
            {
                row = this.rows[pc];
                if (row.ValueStackSizeChange != 0)
                {
                    vsdData.Add(pc);
                    vsdData.Add(row.ValueStackSizeChange);
                }
            }
            return vsdData.ToArray();
        }

        public void SetLastValueStackDepthOffset(int offset)
        {
            this.rows[this.rows.Count - 1].ValueStackSizeChange += offset;
        }

        public int GetEsfPc()
        {
            int size = this.Size;
            for (int i = 0; i < size; ++i)
            {
                if (this.rows[i].ByteCode[0] == (int)OpCode.ESF_LOOKUP)
                {
                    return i;
                }
            }
            throw new Exception("ESF_LOOKUP op not found.");
        }

        public void SetArgs(int index, int[] newArgs)
        {
            ByteRow row = this.rows[index];
            List<int> newByteRow = new List<int>() { row.ByteCode[0] };
            newByteRow.AddRange(newArgs);
            row.ByteCode = newByteRow.ToArray();
        }

        public static ByteBuffer FromLiteralLookup(LiteralLookup literalLookup)
        {
            ByteBuffer output = new ByteBuffer();
            int size = literalLookup.LiteralTypes.Count;
            for (int i = 0; i < size; ++i)
            {
                Types type = literalLookup.LiteralTypes[i];
                object value = literalLookup.LiteralValues[i];
                switch (type)
                {
                    case Types.NULL:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.NULL);
                        break;
                    case Types.BOOLEAN:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.BOOLEAN, ((bool)value) ? 1 : 0);
                        break;
                    case Types.FLOAT:
                        output.Add(null, OpCode.ADD_LITERAL, value.ToString(), (int)Types.FLOAT);
                        break;
                    case Types.INTEGER:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.INTEGER, (int)value);
                        break;
                    case Types.STRING:
                        output.Add(null, OpCode.ADD_LITERAL, value.ToString(), (int)Types.STRING);
                        break;
                    case Types.CLASS:
                        output.Add(null, OpCode.ADD_LITERAL, (int)Types.CLASS, (int)value);
                        break;
                    case Types.FUNCTION:
                        output.Add(null, OpCode.ADD_LITERAL, value.ToString(), (int)Types.FUNCTION);
                        break;
                    default:
                        // unknown literal type.
                        throw new InvalidOperationException();
                }
            }

            size = literalLookup.Names.Count;
            for (int i = 0; i < size; ++i)
            {
                output.Add(null, OpCode.ADD_NAME, literalLookup.Names[i]);
            }

            return output;
        }
    }
}
