using Pastel.Nodes;

namespace Pastel.Transpilers
{
    public class TranspilerContext
    {
        public ILibraryNativeInvocationTranslator CurrentLibraryFunctionTranslator { get; set; }

        private System.Text.StringBuilder buffer = new System.Text.StringBuilder();

        // This is a hack for conveying extra information to the top-level function serializer for switch statement stuff.
        // This reference is updated in TranslateFunctionDefinition.
        public FunctionDefinition PY_HACK_CurrentFunctionDef { get; set; }
        public int SwitchCounter { get; set; }

        public TranspilerContext()
        {
            this.SwitchCounter = 0;
        }

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

        public FunctionDefinition CurrentFunctionDefinition
        {
            get { return this.PY_HACK_CurrentFunctionDef; }
            set
            {
                this.PY_HACK_CurrentFunctionDef = value;
                this.SwitchCounter = 0;
            }
        }
    }
}
