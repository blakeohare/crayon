using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ReturnStatement : Executable
	{
		public Expression Expression { get; private set; }

		public ReturnStatement(Token returnToken, Expression nullableExpression)
			: base(returnToken)
		{
			this.Expression = nullableExpression;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			if (this.Expression != null)
			{
				this.Expression = this.Expression.Resolve(parser);
			}
			return Listify(this);
		}
	}
}
