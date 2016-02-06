using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ListDefinition : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public Expression[] Items { get; private set; }
		public ListDefinition(Token openBracket, IList<Expression> items, Executable owner)
			: base(openBracket, owner)
		{
			this.Items = items.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Items.Length; ++i)
			{
				this.Items[i] = this.Items[i].Resolve(parser);
			}

			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.BatchExpressionNameResolver(parser, lookup, imports, this.Items);
			return this;
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			foreach (Expression item in this.Items)
			{
				item.SetLocalIdPass(varIds);
			}
		}
	}
}
