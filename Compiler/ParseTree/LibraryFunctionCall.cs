using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class LibraryFunctionCall: Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public string Name { get; private set; }
		public Expression[] Args { get; private set; }
		public string LibraryName { get; set; }

		public LibraryFunctionCall(Token token, string name, IList<Expression> args, Executable owner)
			: base(token, owner)
		{
			string callingLibrary = null;
			Executable ownerWalker = owner;
			while (callingLibrary == null && owner != null)
			{
				callingLibrary = owner.LibraryName;
				owner = owner.FunctionOrClassOwner;
			}

			if (callingLibrary == null)
			{
				throw new ParserException(this.FirstToken, "Cannot call native library functions from outside a library.");
			}

			this.LibraryName = callingLibrary;

			string expectedPrefix = "lib_" + callingLibrary.ToLower() + "_";
			if (!name.StartsWith(expectedPrefix) && !name.StartsWith("lib_core_"))
			{
				throw new ParserException(this.FirstToken, "Invalid library function name. Must begin with a '$$" + expectedPrefix + "' prefix.");
			}
			this.Name = name;
			this.Args = args.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i] = this.Args[i].Resolve(parser);
			}
			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new InvalidOperationException(); // this class is generated on the general resolve pass.
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].SetLocalIdPass(varIds);
			}
		}
	}
}
