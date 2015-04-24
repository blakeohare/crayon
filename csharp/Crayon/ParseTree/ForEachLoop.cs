using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ForEachLoop : Executable
	{
		public Token IterationVariable { get; private set; }
		public Expression IterationExpression { get; private set; }
		public Executable[] Body { get; private set; }
 
		public ForEachLoop(Token forToken, Token iterationVariable, Expression iterationExpression, IList<Executable> body)
			: base(forToken)
		{
			this.IterationVariable = iterationVariable;
			this.IterationExpression = iterationExpression;
			this.Body = body.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.IterationExpression = this.IterationExpression.Resolve(parser);
			this.Body = Resolve(parser, this.Body).ToArray();
			return Listify(this);
		}

		public override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			string iterator = this.IterationVariable.Value;
			varIds.RegisterVariable(iterator);

			foreach (Executable ex in this.Body)
			{
				ex.AssignVariablesToIds(varIds);
			}
		}
	}
}
