using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ThisKeyword : Expression 
	{
		public ThisKeyword(Token token)
			: base(token)
		{
		}

		public override Expression Resolve(Parser parser)
		{
			return this;
		}
	}
}
