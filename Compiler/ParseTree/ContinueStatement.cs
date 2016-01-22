using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ContinueStatement : Executable
	{
		public ContinueStatement(Token continueToken) : base(continueToken) { }

		internal override IList<Executable> Resolve(Parser parser)
		{
			return Listify(this);
		}

		internal override void VariableUsagePass(Parser parser)
		{
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
