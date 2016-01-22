using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ThisKeyword : Expression 
	{
		public ThisKeyword(Token token)
			: base(token)
		{
		}

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override void VariableUsagePass(Parser parser)
		{
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
