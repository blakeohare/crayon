using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class NullConstant : Expression, IConstantValue
	{
		public override bool CanAssignTo { get { return false; } }

		public NullConstant(Token token, Executable owner)
			: base(token, owner)
		{ }

		public override bool IsLiteral { get { return true; } }

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

		public Expression CloneValue(Token token, Executable owner)
		{
			return new NullConstant(token, owner);
		}
	}
}
