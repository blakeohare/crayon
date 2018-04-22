namespace Pastel.Transpilers
{
    public class TranspilerContext
    {
        private System.Text.StringBuilder buffer = new System.Text.StringBuilder();

        public TranspilerContext Append(char c)
        {
            this.buffer.Append(c);
            return this;
        }

        public TranspilerContext Append(string s)
        {
            this.buffer.Append(s);
            return this;
        }

        public TranspilerContext Append(int v)
        {
            this.buffer.Append(v);
            return this;
        }

        public string FlushAndClearBuffer()
        {
            string value = this.buffer.ToString();
            this.buffer.Clear();
            return value;
        }
    }
}
