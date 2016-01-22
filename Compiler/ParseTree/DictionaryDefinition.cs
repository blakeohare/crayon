using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class DictionaryDefinition : Expression
	{
		public Expression[] Keys { get; private set; }
		public Expression[] Values { get; private set; }

		public DictionaryDefinition(Token braceToken, IList<Expression> keys, IList<Expression> values)
			: base(braceToken)
		{
			this.Keys = keys.ToArray();
			this.Values = values.ToArray();
		}

		public override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Keys.Length; ++i)
			{
				this.Keys[i] = this.Keys[i].Resolve(parser);
				this.Values[i] = this.Values[i].Resolve(parser);
				// TODO: verify no duplicate keys?
			}
			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
			for (int i = 0, length = this.Keys.Length; i < length; ++i)
			{
				this.Keys[i].VariableUsagePass(parser);
				this.Values[i].VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0, length = this.Keys.Length; i < length; ++i)
			{
				this.Keys[i].VariableIdAssignmentPass(parser);
				this.Values[i].VariableIdAssignmentPass(parser);
			}	
		}
	}
}
