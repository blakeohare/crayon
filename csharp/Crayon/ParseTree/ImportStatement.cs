using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ImportStatement : Executable
	{
		public Token FileToken { get; private set; }
		public bool IsSystemLibrary { get; private set; }

		public ImportStatement(Token importToken, Token fileToken, bool isSystemLibrary)
			: base(importToken)
		{
			this.FileToken = fileToken;
			this.IsSystemLibrary = isSystemLibrary;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			throw new Exception("Imports shouldn't exist at this point in the compilation pipeline.");
		}

		public string FilePath
		{
			get
			{
				string stringValue = this.FileToken.Value;
				if (this.IsSystemLibrary)
				{
					return stringValue;
				}

				string output = Util.ConvertStringTokenToValue(stringValue);
				if (output == null)
				{
					throw new ParserException(this.FileToken, "Invalid string escape sequence found.");
				}
				return output;
			}
		}

		public override void VariableUsagePass(Parser parser)
		{
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
		}
	}
}
