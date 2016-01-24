using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class LibraryFunctionCall: Expression
	{
		public string Name { get; private set; }
		public Expression[] Args { get; private set; }
		public string LibraryName { get; set; }

		public LibraryFunctionCall(Token token, string name, IList<Expression> args, string libraryName, Executable owner)
			: base(token, owner)
		{
			if (libraryName == null)
			{
				throw new ParserException(this.FirstToken, "Cannot call native library functions from outside a library.");
			}

			this.LibraryName = libraryName;

			string expectedPrefix = "lib_" + libraryName.ToLower() + "_";
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
