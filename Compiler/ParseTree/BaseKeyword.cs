using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class BaseKeyword : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public BaseKeyword(Token token, Executable owner)
			: base(token, owner)
		{
		}

		internal override Expression Resolve(Parser parser)
		{
			throw new ParserException(this.FirstToken, "'base' keyword can only be used as part of a method reference.");
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

		internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
		{
			return this;
		}

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
