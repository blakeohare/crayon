using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ImportStatement : Executable
	{
		public string ImportPath { get; set; }

		public ImportStatement(Token importToken, string path)
			: base(importToken, null)
		{
			this.ImportPath = path;
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			throw new Exception("Imports shouldn't exist at this point in the compilation pipeline.");
		}

		internal override void VariableUsagePass(Parser parser) { }
		internal override void VariableIdAssignmentPass(Parser parser) { }

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new InvalidOperationException();
		}
	}
}
