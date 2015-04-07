using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	class CompileTimeDictionary:Expression
	{
		public string Type { get; private set; }

		public CompileTimeDictionary(Token firstToken, string type)
			: base(firstToken)
		{
			this.Type = type;
		}

		public override Expression Resolve(Parser parser)
		{
			// parent expression will do the resolving.
			return this;
		}
	}
}
