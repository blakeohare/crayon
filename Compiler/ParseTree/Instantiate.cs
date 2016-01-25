using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class Instantiate : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public Token NameToken { get; private set; }
		public string Name { get; private set; }
		public Expression[] Args { get; private set; }

		public Instantiate(Token firstToken, Token firstClassNameToken, string name, IList<Expression> args, Executable owner)
			: base(firstToken, owner)
		{
			this.NameToken = firstClassNameToken;
			this.Name = name;
			this.Args = args.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			string className = this.NameToken.Value;

			if (parser.IsTranslateMode)
			{
				StructDefinition structDefinition = parser.GetStructDefinition(className);

				if (structDefinition != null)
				{
					StructInstance si = new StructInstance(this.FirstToken, this.NameToken, this.Args, this.FunctionOrClassOwner);
					si = (StructInstance)si.Resolve(parser);
					return si;
				}
			}
			else
			{
				ClassDefinition classDefinition = parser.GetClass(className);
				if (classDefinition == null)
				{
					throw new ParserException(this.NameToken, "Class not defined: '" + className + "'");
				}
			}

			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i] = this.Args[i].Resolve(parser);
			}

			// TODO: use parser context to resolve this into a struct

			return this;
		}

		internal override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableIdAssignmentPass(parser);
			}
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.BatchExpressionNameResolver(parser, lookup, imports, this.Args);

			throw new System.NotImplementedException(); // TODO: this
		}
	}
}
