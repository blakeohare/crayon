using System;
using System.Collections.Generic;

namespace Crayon.Pastel.Nodes
{
    class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; set; }

        public ExpressionAsExecutable(Expression expression) : base(expression.FirstToken)
        {
            this.Expression = expression;
        }

        public override IList<Executable> NameResolution(Dictionary<string, FunctionDefinition> functionLookup, Dictionary<string, StructDefinition> structLookup)
        {
            throw new NotImplementedException();
        }

        public override void ResolveTypes()
        {
            throw new NotImplementedException();
        }
    }
}
