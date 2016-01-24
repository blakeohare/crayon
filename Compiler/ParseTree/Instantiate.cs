using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class Instantiate : Expression
	{
		public Token NameToken { get; private set; }
		public Expression[] Args { get; private set; }

		public Instantiate(Token firstToken, Token classNameToken, IList<Expression> args, Executable owner)
			: base(firstToken, owner)
		{
			this.NameToken = classNameToken;
			this.Args = args.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			string className = this.NameToken.Value;

			StructDefinition structDefinition = parser.GetStructDefinition(className);

			if (parser.IsTranslateMode)
			{
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
	}
}
