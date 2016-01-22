﻿using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class FunctionCall : Expression
	{
		public Expression Root { get; private set; }
		public Token ParenToken { get; private set; }
		public Expression[] Args { get; private set; }

		public FunctionCall(Expression root, Token parenToken, IList<Expression> args)
			: base(root.FirstToken)
		{
			this.Root = root;
			this.ParenToken = parenToken;
			this.Args = args.ToArray();
		}

		public override Expression Resolve(Parser parser)
		{
			// TODO: isset(var) insertion goes here

			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i] = this.Args[i].Resolve(parser);
			}

			if (this.Root is Variable)
			{
				string varName = ((Variable)this.Root).Name;
				if (varName.StartsWith("$"))
				{
					SystemFunctionCall sfc = new SystemFunctionCall(this.Root.FirstToken, this.Args);
					return sfc.Resolve(parser);
				}

				if (parser.GetClass(varName) != null)
				{
					throw new ParserException(this.ParenToken, "Cannot invoke a class like a function. To construct a new class, the \"new\" keyword must be used.");
				}
			}

			this.Root = this.Root.Resolve(parser);

			return this;
		}

		public override void VariableUsagePass(Parser parser)
		{
			this.Root.VariableUsagePass(parser);
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			this.Root.VariableIdAssignmentPass(parser);
			for (int i = 0; i < this.Args.Length; ++i)
			{
				this.Args[i].VariableIdAssignmentPass(parser);
			}	
		}
	}
}
