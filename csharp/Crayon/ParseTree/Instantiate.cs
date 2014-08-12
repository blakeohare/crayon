using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class Instantiate : Expression
	{
		public Token NameToken { get; private set; }
		public Expression[] Args { get; private set; }

		public Instantiate(Token firstToken, Token classNameToken, IList<Expression> args)
			: base(firstToken)
		{
			this.NameToken = classNameToken;
			this.Args = args.ToArray();
		}

		public override Expression Resolve(Parser parser)
		{
			string className = this.NameToken.Value;

			StructDefinition structDefinition = parser.GetStructDefinition(className);

			if (structDefinition != null)
			{
				StructInstance si = new StructInstance(this.FirstToken, this.NameToken, this.Args);
				si = (StructInstance)si.Resolve(parser);
				return si;
			}

			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i] = this.Args[i].Resolve(parser);
			}

			// TODO: use parser context to resolve this into a struct

			return this;
		}
	}
}
