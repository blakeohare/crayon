namespace Crayon.ParseTree
{
	internal class Variable : Expression
	{
		public string Name { get; private set; }

		public Variable(Token token, string name)
			: base(token)
		{
			this.Name = name;
		}

		public override Expression Resolve(Parser parser)
		{
			Expression constant = parser.GetConst(this.Name);
			if (constant != null)
			{
				return constant;
			}
			return this;
		}

		public override void GetAllVariableNames(System.Collections.Generic.Dictionary<string, bool> lookup)
		{
			lookup[this.Name] = true;
		}
	}
}
