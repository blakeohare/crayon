using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class StructInstance : Expression
	{
		public Token NameToken { get; private set; }
		public Expression[] Args { get; private set; }

		public StructInstance(Token firstToken, Token nameToken, IList<Expression> args)
			: base(firstToken)
		{
			this.NameToken = nameToken;
			this.Args = args.ToArray();
		}

		public override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i] = this.Args[i].Resolve(parser);
			}

			// TODO: verify args matches constructor length

			return this;
		}
	}
}
