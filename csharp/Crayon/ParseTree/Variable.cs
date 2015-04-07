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

		public bool IsStatic
		{
			get
			{
				return this.Annotations != null &&
					this.Annotations.ContainsKey("uncontained");
			}
		}

		public override Expression Resolve(Parser parser)
		{
			if (this.Name == "$var")
			{
				return new CompileTimeDictionary(this.FirstToken, "var");
			}

			if (this.Name == "this")
			{
				if (parser.IsInClass)
				{
					return new ThisKeyword(this.FirstToken).Resolve(parser);
				}

				throw new ParserException(this.FirstToken, "'this' keyword is only allowed inside classes.");
			}

			if (Parser.IsReservedKeyword(this.Name))
			{
				throw new ParserException(this.FirstToken, "'" + this.Name + "' is a reserved keyword and cannot be used like this.");
			}

			Expression constant = parser.GetConst(this.Name);
			if (constant != null)
			{
				return constant;
			}
			return this;
		}

		public override void GetAllVariableNames(System.Collections.Generic.Dictionary<string, bool> lookup)
		{
			if (this.GetAnnotation("global") == null)
			{
				lookup[this.Name] = true;
			}
		}
	}
}
