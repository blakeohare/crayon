namespace Crayon.Pastel.Nodes
{
    class InlineConstant : Expression
    {
        public object Value { get; set; }
        public PType Type { get; set; }

        public InlineConstant(PType type, Token firstToken, object value) : base(firstToken)
        {
            this.Type = type;
            this.Value = value;
        }
    }
}
