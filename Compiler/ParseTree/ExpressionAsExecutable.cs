using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ExpressionAsExecutable : Executable
	{
		public Expression Expression { get; private set; }

		public ExpressionAsExecutable(Expression expression)
			: base(expression.FirstToken)
		{
			this.Expression = expression;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Expression = this.Expression.Resolve(parser);

			if (this.Expression == null)
			{
				return new Executable[0];
			}

			if (this.Expression is Increment)
			{
				Increment inc = (Increment)this.Expression;
				Assignment output = new Assignment(inc.Root, inc.IncrementToken, inc.IsIncrement ? "+=" : "-=", new IntegerConstant(inc.IncrementToken, 1));
				return output.Resolve(parser);
			}

			return Listify(this);
		}

		public override void VariableUsagePass(Parser parser)
		{
			this.Expression.VariableUsagePass(parser);
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			this.Expression.VariableIdAssignmentPass(parser);
		}
	}
}
