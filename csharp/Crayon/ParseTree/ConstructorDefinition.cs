using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ConstructorDefinition : Executable
	{
		public Executable[] Code { get; private set; }
		public Token[] Args { get; private set; }
		public Expression[] DefaultValues { get; private set; }
		public Expression[] BaseArgs { get; private set; }
		public Token BaseToken { get; private set; }

		public ConstructorDefinition(Token constructorToken, IList<Token> args, IList<Expression> defaultValues, IList<Expression> baseArgs, IList<Executable> code, Token baseToken)
			: base(constructorToken)
		{
			this.Args = args.ToArray();
			this.DefaultValues = defaultValues.ToArray();
			this.BaseArgs = baseArgs.ToArray();
			this.Code = code.ToArray();
			this.BaseToken = baseToken;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			List<Executable> code = new List<Executable>();
			foreach (Executable line in this.Code)
			{
				code.AddRange(line.Resolve(parser));
			}
			this.Code = code.ToArray();

			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.DefaultValues[i] = this.DefaultValues[i] == null ? null : this.DefaultValues[i].Resolve(parser);
			}

			for (int i = 0; i < this.BaseArgs.Length; ++i)
			{
				this.BaseArgs[i] = this.BaseArgs[i].Resolve(parser);
			}

			return Listify(this);
		}
	}
}
