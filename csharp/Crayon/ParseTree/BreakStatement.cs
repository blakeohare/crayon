using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class BreakStatement : Executable
	{
		public BreakStatement(Token breakToken) : base(breakToken) { }

		public override IList<Executable> Resolve(Parser parser)
		{
			return Listify(this);
		}

		public override bool IsTerminator { get { return true; } }

		public override void VariableUsagePass(Parser parser)
		{
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
