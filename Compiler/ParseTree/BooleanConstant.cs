using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class BooleanConstant : Expression, IConstantValue
	{
		public override bool CanAssignTo { get { return false; } }

		public bool Value { get; private set; }

		public override bool IsLiteral { get { return true; } }

		public BooleanConstant(Token token, bool value, Executable owner)
			: base(token, owner)
		{
			this.Value = value;
		}

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override void VariableUsagePass(Parser parser) { }
		internal override void VariableIdAssignmentPass(Parser parser) { }

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}

		public Expression CloneValue(Token token, Executable owner)
		{
			return new BooleanConstant(token, this.Value, owner);
		}
	}
}
