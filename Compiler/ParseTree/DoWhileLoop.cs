using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class DoWhileLoop : Executable
	{
		public Executable[] Code { get; private set; }
		public Expression Condition { get; private set; }

		public DoWhileLoop(Token doToken, IList<Executable> code, Expression condition)
			: base(doToken)
		{
			this.Code = code.ToArray();
			this.Condition = condition;
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			this.Code = Resolve(parser, this.Code).ToArray();

			this.Condition = this.Condition.Resolve(parser);

			return Listify(this);
		}

		internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			foreach (Executable line in this.Code)
			{
				line.GetAllVariableNames(lookup);
			}
		}

		internal override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			foreach (Executable ex in this.Code)
			{
				ex.AssignVariablesToIds(varIds);
			}
		}

		internal override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableUsagePass(parser);
			}

			this.Condition.VariableUsagePass(parser);
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableIdAssignmentPass(parser);
			}

			this.Condition.VariableIdAssignmentPass(parser);
		}
	}
}
