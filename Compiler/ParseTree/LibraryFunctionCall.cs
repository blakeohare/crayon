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
			string callingLibrary = owner.LibraryName;

			if (callingLibrary == null)
			{
				throw new ParserException(this.FirstToken, "Cannot call native library functions from outside a library.");
			}

			this.LibraryName = callingLibrary;

			string expectedPrefix = "lib_" + callingLibrary.ToLower() + "_";
			if (!name.StartsWith(expectedPrefix))
			{
				throw new ParserException(this.FirstToken, "Invalid library function name. Must begin with a '$$" + expectedPrefix + "' prefix.");
			}
			this.Name = name;
			this.Args = args.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			// Args are already resolved.
			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new InvalidOperationException(); // this class is generated on the general resolve pass.
		}

		internal override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
