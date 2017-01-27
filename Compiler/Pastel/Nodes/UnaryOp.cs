namespace Crayon.Pastel.Nodes
{
    class UnaryOp : Expression
    {
        public Expression Expression { get; set; }

        public UnaryOp(Token op, Expression root) : base(op)
        {
            this.Expression = root;
        }
    }
}
