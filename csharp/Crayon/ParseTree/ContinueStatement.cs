using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ContinueStatement : Executable
	{
		public ContinueStatement(Token continueToken) : base(continueToken) { }
		public override IList<Executable> Resolve(Parser parser)
		{
			return Listify(this);
		}

		public override void VariableUsagePass(Parser parser)
		{
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
