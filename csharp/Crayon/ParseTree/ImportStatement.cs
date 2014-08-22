using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ImportStatement : Executable
	{
		public Token FileToken { get; private set; }

		public ImportStatement(Token importToken, Token fileToken)
			: base(importToken)
		{
			this.FileToken = fileToken;
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
				string output = Util.ConvertStringTokenToValue(stringValue);
				if (output == null)
				{
					throw new ParserException(this.FileToken, "Invalid string escape sequence found.");
				}
				return output;
			}
		}
	}
}
