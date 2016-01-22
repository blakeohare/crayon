using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	class CompileTimeDictionary : Expression
	{
		public string Type { get; private set; }

		public CompileTimeDictionary(Token firstToken, string type)
			: base(firstToken)
		{
			this.Type = type;
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
