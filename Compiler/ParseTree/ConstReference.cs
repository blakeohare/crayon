using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ConstReference : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public ConstStatement ConstStatement { get; private set; }

		public ConstReference(Token token, ConstStatement con, Executable owner)
			: base(token, owner)
		{
			this.ConstStatement = con;
		}

		internal override Expression Resolve(Parser parser)
		{
			throw new NotImplementedException();
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new NotImplementedException();
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			throw new NotImplementedException();
		}
	}
}
