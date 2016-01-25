using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	class CompileTimeDictionary : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public string Type { get; private set; }

		public CompileTimeDictionary(Token firstToken, string type, Executable owner)
			: base(firstToken, owner)
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

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}
	}
}
