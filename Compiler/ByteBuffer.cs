using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    internal class ByteBuffer
    {
        private class ByteRow
        {
            public int[] ByteCode { get; set; }
            public Token Token { get; set; }
            public string StringArg { get; set; }
            public ByteCodeEsfToken EsfToken { get; set; }
        }

        private List<ByteRow> rows = new List<ByteRow>();

        public ByteBuffer() { }

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
            for (int i = 0; i < size; ++i)
            {
                ByteRow row = this.rows[i];
                if (row.ByteCode[0] == (int)OpCode.CONTINUE)
                {
                    row.ByteCode = resolveAsJumpToEnd
                        ? new int[] { (int)OpCode.JUMP, size - i - 1 }
                        : new int[] { (int)OpCode.JUMP, -i - 1 };
                }
            }
        }

        // Breaks should be resolved into JUMPS that go to the end of the current byte code buffer.
        public void ResolveBreaks()
        {
            int size = this.Size;
            for (int i = 0; i < size; ++i)
            {
                ByteRow row = this.rows[i];
                if (row.ByteCode[0] == (int)OpCode.BREAK)
                {
                    row.ByteCode = new int[] { (int)OpCode.JUMP, size - i - 1 };
                }
            }
        }
    }
}
