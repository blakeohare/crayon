using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class LibraryFunctionReference : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public string Name { get; private set; }

		public LibraryFunctionReference(Token token, string name, Executable owner)
			: base(token, owner)
		{
			this.Name = name;
		}

		internal override Expression Resolve(Parser parser)
		{
			throw new ParserException(this.FirstToken, "Library functions cannot be passed around as references. They can only be invoked.");
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new InvalidOperationException(); // Created during resolve name phase.
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds) { }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new NotImplementedException();
        }
    }
}
