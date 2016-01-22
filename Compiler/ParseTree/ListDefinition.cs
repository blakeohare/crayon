using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ListDefinition : Expression
	{
		public Expression[] Items { get; private set; }
		public ListDefinition(Token openBracket, IList<Expression> items)
			: base(openBracket)
		{
			this.Items = items.ToArray();
		}

		public override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Items.Length; ++i)
			{
				this.Items[i] = this.Items[i].Resolve(parser);
			}

			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.Items.Length; ++i)
			{
				this.Items[i].VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.Items.Length; ++i)
			{
				this.Items[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
