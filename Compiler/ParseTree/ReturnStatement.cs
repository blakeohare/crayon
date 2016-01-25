using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ReturnStatement : Executable
	{
		public Expression Expression { get; private set; }

		public ReturnStatement(Token returnToken, Expression nullableExpression, Executable owner)
			: base(returnToken, owner)
		{
			this.Expression = nullableExpression;
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			if (this.Expression != null)
			{
				this.Expression = this.Expression.Resolve(parser);
			}
			return Listify(this);
		}

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			if (this.Expression != null)
			{
				this.Expression = this.Expression.ResolveNames(parser, lookup, imports);
			}
			return this;
		}

		public override bool IsTerminator { get { return true; } }

		internal override void VariableUsagePass(Parser parser)
		{
			if (this.Expression != null)
			{
				this.Expression.VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			if (this.Expression != null)
			{
				this.Expression.VariableIdAssignmentPass(parser);
			}
		}
	}
}
