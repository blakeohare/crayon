using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class Assignment : Executable
	{
		public Expression Target { get; private set; }
		public Expression Value { get; private set; }
		public Token AssignmentOpToken { get; private set; }
		public string AssignmentOp { get; private set; }

		public Assignment(Expression target, Token assignmentOpToken, string assignmentOp, Expression assignedValue)
			: base(target.FirstToken)
		{
			this.Target = target;
			this.AssignmentOpToken = assignmentOpToken;
			this.AssignmentOp = assignmentOp;
			this.Value = assignedValue;
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Target = this.Target.Resolve(parser);

			if (this.Target is Variable)
			{
				this.Target = this.Target.Resolve(parser);
			}
			else if (this.Target is BracketIndex)
			{
				BracketIndex bi = this.Target as BracketIndex;
				bi.Root = bi.Root.Resolve(parser);
				bi.Index = bi.Index.Resolve(parser);
			}
			else if (this.Target is DotStep)
			{
				DotStep ds = this.Target as DotStep;
				ds.Root = ds.Root.Resolve(parser);
			}

			this.Value = this.Value.Resolve(parser);

			return Listify(this);
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			this.Target.GetAllVariableNames(lookup);
		}

		public override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			this.Target.AssignVariablesToIds(varIds);
		}
	}
}
