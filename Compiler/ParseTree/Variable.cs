using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class Variable : Expression
	{
		public override bool CanAssignTo { get { return true; } }

		public string Name { get; private set; }

		public int LocalScopeId { get; set; }

		public Variable(Token token, string name, Executable owner)
			: base(token, owner)
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
				return new CompileTimeDictionary(this.FirstToken, "var", this.FunctionOrClassOwner);
			}

			if (this.Name == "this" || this.Name == "base")
			{
				if (parser.IsInClass)
				{
					if (this.Name == "this")
					{
						return new ThisKeyword(this.FirstToken, this.FunctionOrClassOwner).Resolve(parser);
					}
					else
					{
						return new BaseKeyword(this.FirstToken, this.FunctionOrClassOwner).Resolve(parser);
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

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			if (this.Name.StartsWith("$$"))
			{
				return new LibraryFunctionReference(this.FirstToken, this.Name.Substring(2), this.FunctionOrClassOwner);
			}

			if (this.Name == "this")
			{
				return new ThisKeyword(this.FirstToken, this.FunctionOrClassOwner);
			}

			Executable exec = DoNameLookup(lookup, imports, this.Name);
			if (exec != null)
			{
				return Resolver.ConvertStaticReferenceToExpression(exec, this.FirstToken, this.FunctionOrClassOwner);
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

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			this.LocalScopeId = varIds.GetVarId(this.FirstToken, true);
		}

		public override string ToString()
		{
			return "<Variable> " + this.Name;
		}
	}
}
