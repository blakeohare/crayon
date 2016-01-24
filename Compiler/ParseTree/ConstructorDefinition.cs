using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class ConstructorDefinition : Executable
	{
		public Executable[] Code { get; private set; }
		public Token[] Args { get; private set; }
		public int[] ArgVarIDs { get; private set; }
		public Expression[] DefaultValues { get; private set; }
		public Expression[] BaseArgs { get; private set; }
		public Token BaseToken { get; private set; }

		public ConstructorDefinition(Token constructorToken, IList<Token> args, IList<Expression> defaultValues, IList<Expression> baseArgs, IList<Executable> code, Token baseToken, Executable owner)
			: base(constructorToken, owner)
		{
			this.Args = args.ToArray();
			this.ArgVarIDs = new int[this.Args.Length];
			this.DefaultValues = defaultValues.ToArray();
			this.BaseArgs = baseArgs.ToArray();
			this.Code = code.ToArray();
			this.BaseToken = baseToken;
		}

		internal override IList<Executable> Resolve(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.DefaultValues[i] = this.DefaultValues[i] == null ? null : this.DefaultValues[i].Resolve(parser);
			}

			for (int i = 0; i < this.BaseArgs.Length; ++i)
			{
				this.BaseArgs[i] = this.BaseArgs[i].Resolve(parser);
			}

			List<Executable> code = new List<Executable>();
			foreach (Executable line in this.Code)
			{
				code.AddRange(line.Resolve(parser));
			}
			this.Code = code.ToArray();

			return Listify(this);
		}

		internal override void VariableUsagePass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				Token arg = this.Args[i];
				parser.VariableRegister(arg.Value, true, arg);
				Expression defaultValue = this.DefaultValues[i];
				if (defaultValue != null)
				{
					defaultValue.VariableUsagePass(parser);
				}
			}

			for (int i = 0; i < this.BaseArgs.Length; ++i)
			{
				this.BaseArgs[i].VariableUsagePass(parser);
			}

			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.ArgVarIDs[i] = parser.VariableGetLocalAndGlobalIds(this.Args[i].Value)[0];
				if (this.DefaultValues[i] != null)
				{
					this.DefaultValues[i].VariableIdAssignmentPass(parser);
				}
			}

			for (int i = 0; i < this.BaseArgs.Length; ++i)
			{
				this.BaseArgs[i].VariableIdAssignmentPass(parser);
			}

			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
