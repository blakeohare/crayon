namespace Crayon.Pastel.Nodes
{
    class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; set; }

        public ExpressionAsExecutable(Expression expression) : base(expression.FirstToken)
        {
            this.Expression = expression;
        }
    }
}
