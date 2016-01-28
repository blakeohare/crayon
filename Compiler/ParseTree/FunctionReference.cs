using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class FunctionReference : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public FunctionDefinition FunctionDefinition { get; set; }

		public FunctionReference(Token token, FunctionDefinition funcDef, Executable owner)
			: base(token, owner)
		{
			this.FunctionDefinition = funcDef;
		}

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new InvalidOperationException(); // Generated in the resolve name phase.
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }
	}
}
