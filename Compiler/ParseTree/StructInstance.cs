using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class StructInstance : Expression
	{
		public override bool CanAssignTo { get { return false; } }

		public Token NameToken { get; private set; }
		public Expression[] Args { get; private set; }

		public StructInstance(Token firstToken, Token nameToken, IList<Expression> args, Executable owner)
			: base(firstToken, owner)
		{
			this.NameToken = nameToken;
			this.Args = args.ToArray();
		}

		internal override Expression Resolve(Parser parser)
		{
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i] = this.Args[i].Resolve(parser);
			}

			// TODO: verify args matches constructor length

			return this;
		}

		internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			throw new System.NotImplementedException();
		}

		internal override void SetLocalIdPass(VariableIdAllocator varIds)
		{
			throw new InvalidOperationException(); // translate mode only
		}
	}
}
