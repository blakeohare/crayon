using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class EnumReference : Expression
	{
		public EnumDefinition EnumDefinition { get; set; }

		public EnumReference(Token token, EnumDefinition enumDefinition, Executable owner)
			: base(token, owner)
		{
			this.EnumDefinition = enumDefinition;
		}

		public override bool CanAssignTo { get { return false; } }

		internal override Expression Resolve(Parser parser)
		{
			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new NotImplementedException();
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			throw new NotImplementedException();
		}

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
    }
}
