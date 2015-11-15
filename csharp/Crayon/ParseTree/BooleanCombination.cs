using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class BooleanCombination : Expression
	{
		public Expression[] Expressions { get; private set; }
		public Token[] Ops { get; private set; }

		public BooleanCombination(IList<Expression> expressions, IList<Token> ops)
			: base(expressions[0].FirstToken)
		{
			this.Expressions = expressions.ToArray();
			this.Ops = ops.ToArray();
		}

		public override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Expressions.Length; ++i)
			{
				this.Expressions[i] = this.Expressions[i].Resolve(parser);
			}

			for (int i = 0; i < this.Ops.Length; ++i)
			{
				if (this.Expressions[i] is BooleanConstant)
				{
					// TODO: this can be optimized
					// but I am in a hurry right now.
				}
			}

			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
			foreach (Expression expr in this.Expressions)
			{
				expr.VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			foreach (Expression expr in this.Expressions)
			{
				expr.VariableIdAssignmentPass(parser);
			}
		}
	}
}
