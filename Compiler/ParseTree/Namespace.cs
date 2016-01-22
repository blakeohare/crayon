using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	// Despite being an "Executable", this isn't an executable thing.
	// It will get optimized away at resolution time.
	internal class Namespace : Executable
	{
		public Executable[] Code { get; set; }

		// Going to try to get away with not having a reference to the actual namespace name.
		// The namespace object should strictly be a wrapper of multiple executables as one executable and flattened as soon as possible.
		// Actual function and class definitions have a field that refer to the fully flattened namespace name.
		public Namespace(Token namespaceToken, IList<Executable> code)
			: base(namespaceToken)
		{
			this.Code = code.ToArray();
		}

		public void AppendFlattenedCode(IList<Executable> codeList)
		{
			foreach (Executable line in this.Code)
			{
				if (line is Namespace)
				{
					((Namespace)line).AppendFlattenedCode(codeList);
				}
				else
				{
					codeList.Add(line);
				}
			}
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			throw new ParserException(this.FirstToken, "Namespace declaration not allowed here. Namespaces may only exist in the root of a file or nested within other namespaces.");
		}

		internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			throw new NotImplementedException();
		}

		internal override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			throw new NotImplementedException();
		}

		internal override void VariableUsagePass(Parser parser)
		{
			throw new NotImplementedException();
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			throw new NotImplementedException();
		}
	}
}
