using System;

namespace Crayon.Pastel.Nodes
{
    class ReturnStatement : Executable
    {
        public Expression Expression { get; set; }

        public ReturnStatement(Token returnToken, Expression expression) : base(returnToken)
        {
            this.Expression = expression;
        }
    }
}
