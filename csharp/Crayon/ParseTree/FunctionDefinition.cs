using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class FunctionDefinition : Executable
	{
		public Token NameToken { get; private set; }
		public Token[] ArgNames { get; private set; }
		public Expression[] DefaultValues { get; private set; }
		public Executable[] Code { get; private set; }

		public FunctionDefinition(Token functionToken, Token nameToken, IList<Token> argNames, IList<Expression> argDefaultValues, IList<Executable> code)
			: base(functionToken)
		{
			this.NameToken = nameToken;
			this.ArgNames = argNames.ToArray();
			this.DefaultValues = argDefaultValues.ToArray();
			this.Code = code.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			for (int i = 0; i < this.DefaultValues.Length; ++i)
			{
				if (this.DefaultValues[i] != null)
				{
					this.DefaultValues[i] = this.DefaultValues[i].Resolve(parser);
				}
			}

			this.Code = Resolve(parser, this.Code).ToArray();

			if (this.Code.Length == 0 || !(this.Code[this.Code.Length - 1] is ReturnStatement))
			{
				List<Executable> newCode = new List<Executable>(this.Code);
				newCode.Add(new ReturnStatement(this.FirstToken, null));
				this.Code = newCode.ToArray();
			}

			return Listify(this);
		}
	}
}
