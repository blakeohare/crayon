using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ThisKeyword : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public ThisKeyword(Token token, Executable owner)
			: base(token, owner)
		{
		}

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }
	}
}
