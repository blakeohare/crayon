namespace Crayon.ParseTree
{
	internal class Variable : Expression
	{
		public string Name { get; private set; }

		public int LocalScopeId { get; set; }
		public int GlobalScopeId { get; set; }

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

		internal override Expression Resolve(Parser parser)
		{
			if (this.Name == "$var")
			{
				return new CompileTimeDictionary(this.FirstToken, "var");
			}

			if (this.Name == "this" || this.Name == "base")
			{
				if (parser.IsInClass)
				{
					if (this.Name == "this")
					{
						return new ThisKeyword(this.FirstToken).Resolve(parser);
					}
					else
					{
						return new BaseKeyword(this.FirstToken).Resolve(parser);
					}
				}

				throw new ParserException(this.FirstToken, "'" + this.Name + "' keyword is only allowed inside classes.");
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

		internal override void GetAllVariableNames(System.Collections.Generic.Dictionary<string, bool> lookup)
		{
			if (this.GetAnnotation("global") == null)
			{
				lookup[this.Name] = true;
			}
		}

		internal override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			varIds.RegisterVariable(this.Name);
		}

		internal override void VariableUsagePass(Parser parser)
		{
			// Assignments require context of the parent element and should handle this and bypass calling this function.
			// Therefore all invocations of this will assume usage as opposed to assignment.
			parser.VariableRegister(this.Name, false, this.FirstToken);
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			int[] ids = parser.VariableGetLocalAndGlobalIds(this.Name);
			this.LocalScopeId = ids[0];
			this.GlobalScopeId = ids[1];
		}
	}
}
