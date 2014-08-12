using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class ConstStatement : Executable
	{
		public Expression Expression { get; private set; }
		public Token NameToken { get; private set; }
		public string Name { get; private set; }

		public ConstStatement(Token constToken, Token nameToken, Expression expression)
			: base(constToken)
		{
			this.Expression = expression;
			this.NameToken = nameToken;
			this.Name = nameToken.Value;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Expression = this.Expression.Resolve(parser);

			if (this.Expression is IntegerConstant ||
				this.Expression is BooleanConstant ||
				this.Expression is FloatConstant ||
				this.Expression is StringConstant)
			{
				// that's fine.
			}
			else
			{
				throw new ParserException(this.FirstToken, "Invalid valud for const. Expression must resolve to a constant at compile time.");
			}

			parser.RegisterConst(this.NameToken, this.Expression);
			return new Executable[0];
		}
	}
}
