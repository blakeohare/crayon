using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ClassReference : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public ClassDefinition ClassDefinition { get; private set; }

		public ClassReference(Token token, ClassDefinition clazz, Executable owner)
			: base(token, owner)
		{
			this.ClassDefinition = clazz;
		}

		internal override void GetAllVariableNames(Dictionary<string, bool> lookup) { }
		internal override Expression Resolve(Parser parser)
		{
			// normal usages should be optimized out by now.
			throw new ParserException(this.FirstToken, "Unexpected class reference.");
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new InvalidOperationException(); // Created during the resolve names phase.
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }
	}
}
