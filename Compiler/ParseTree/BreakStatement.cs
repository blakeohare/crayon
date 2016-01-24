using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class BreakStatement : Executable
	{
		public BreakStatement(Token breakToken, Executable owner)
			: base(breakToken, owner)
		{ }

		internal override IList<Executable> Resolve(Parser parser)
		{
			return Listify(this);
		}

		public override bool IsTerminator { get { return true; } }
		internal override void VariableUsagePass(Parser parser) { }
		internal override void VariableIdAssignmentPass(Parser parser) { }
	}
}
