using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.ParseTree
{
	internal class ForEachLoop : Executable
	{
		public Token IterationVariable { get; private set; }
		public int IterationVariableId { get; private set; }
		public Expression IterationExpression { get; private set; }
		public Executable[] Code { get; private set; }
 
		public ForEachLoop(Token forToken, Token iterationVariable, Expression iterationExpression, IList<Executable> code)
			: base(forToken)
		{
			this.IterationVariable = iterationVariable;
			this.IterationExpression = iterationExpression;
			this.Code = code.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.IterationExpression = this.IterationExpression.Resolve(parser);
			this.Code = Resolve(parser, this.Code).ToArray();
			return Listify(this);
		}

		public override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			string iterator = this.IterationVariable.Value;
			varIds.RegisterVariable(iterator);

			foreach (Executable ex in this.Code)
			{
				ex.AssignVariablesToIds(varIds);
			}
		}

		public override void VariableUsagePass(Parser parser)
		{
			this.IterationExpression.VariableUsagePass(parser);
			parser.VariableRegister(this.IterationVariable.Value, true, this.IterationVariable);
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			this.IterationExpression.VariableIdAssignmentPass(parser);
			int[] ids = parser.VariableGetLocalAndGlobalIds(this.IterationVariable.Value);
			int scopeId = ids[0] == -1 ? ids[1] : ids[0];
			this.IterationVariableId = scopeId;
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
