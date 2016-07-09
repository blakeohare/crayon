using System.Collections.Generic;

namespace Crayon
{
    internal class ByteBuffer
    {
        private List<int[]> byteCode = new List<int[]>();
        private List<Token> tokens = new List<Token>();
        private List<string> stringArgs = new List<string>();

        public ByteBuffer() { }

        public OpCode LastOp
        {
            get
            {
                return (OpCode)this.byteCode[this.byteCode.Count - 1][0];
            }
        }

        public int Size { get { return this.byteCode.Count; } }

        public void Concat(ByteBuffer other)
        {
            this.byteCode.AddRange(other.byteCode);
            this.tokens.AddRange(other.tokens);
            this.stringArgs.AddRange(other.stringArgs);
        }

        public void Add(Token token, OpCode op, string stringValue, params int[] args)
        {
            List<int> nums = new List<int>(args);
            nums.Insert(0, (int)op);
            this.byteCode.Add(nums.ToArray());
            this.tokens.Add(token);
            this.stringArgs.Add(stringValue);
        }

        public void Add(Token token, OpCode op, params int[] args)
        {
            this.Add(token, op, null, args);
        }

        public List<int[]> ToIntList()
        {
            return this.byteCode;
        }

        public List<string> ToStringList()
        {
            return this.stringArgs;
        }

        public List<Token> ToTokenList()
        {
            return this.tokens;
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
            for (int i = 0; i < this.byteCode.Count; ++i)
            {
                if (this.byteCode[i][0] == (int)OpCode.CONTINUE)
                {
                    this.byteCode[i] = resolveAsJumpToEnd
                        ? new int[] { (int)OpCode.JUMP, this.byteCode.Count - i - 1 }
                        : new int[] { (int)OpCode.JUMP, -i - 1 };
                }
            }
        }

        // Breaks should be resolved into JUMPS that go to the end of the current byte code buffer.
        public void ResolveBreaks()
        {
            for (int i = 0; i < this.byteCode.Count; ++i)
            {
                if (this.byteCode[i][0] == (int)OpCode.BREAK)
                {
                    this.byteCode[i] = new int[] { (int)OpCode.JUMP, this.byteCode.Count - i - 1 };
                }
            }
        }
    }
}
