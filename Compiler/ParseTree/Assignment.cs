using System.Collections.Generic;

namespace Crayon.ParseTree
{
	internal class Assignment : Executable
	{
		public Expression Target { get; private set; }
		public Expression Value { get; private set; }
		public Token AssignmentOpToken { get; private set; }
		public string AssignmentOp { get; private set; }
		public Variable TargetAsVariable { get { return this.Target as Variable; } }

		public Assignment(Expression target, Token assignmentOpToken, string assignmentOp, Expression assignedValue, Executable owner)
			: base(target.FirstToken, owner)
		{
			this.Target = target;
			this.AssignmentOpToken = assignmentOpToken;
			this.AssignmentOp = assignmentOp;
			this.Value = assignedValue;
		}

		internal override IList<Executable> Resolve(Parser parser)
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

		internal override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			this.Target.GetAllVariableNames(lookup);
		}

		internal override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			this.Target.AssignVariablesToIds(varIds);
		}

		internal override void VariableUsagePass(Parser parser)
		{
			this.Value.VariableUsagePass(parser);

			Variable variable = this.TargetAsVariable;

			// Note that things like += do not declare a new variable name and so they don't count as assignment
			// in this context. foo += value should NEVER take a global scope value and assign it to a local scope value.
			// Globals cannot be assigned to from outside the global scope.
			if (variable != null &&
				this.AssignmentOpToken.Value == "=")
			{
				parser.VariableRegister(variable.Name, true, this.Target.FirstToken);
			}
			else
			{
				this.Target.VariableUsagePass(parser);
			}
		}

		internal override void VariableIdAssignmentPass(Parser parser)
		{
			this.Value.VariableIdAssignmentPass(parser);
			Variable variable = this.TargetAsVariable;
			if (variable != null)
			{
				int[] ids = parser.VariableGetLocalAndGlobalIds(variable.Name);
				variable.LocalScopeId = ids[0];
				variable.GlobalScopeId = ids[1];
			}
			else
			{
				this.Target.VariableIdAssignmentPass(parser);
			}
		}

		internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
		{
			this.Target = this.Target.ResolveNames(parser, lookup, imports);
			this.Value = this.Value.ResolveNames(parser, lookup, imports);

			// TODO: abstract property, CanAssignTo
			if (!this.Target.CanAssignTo)
			{
				throw new ParserException(this.Target.FirstToken, "Cannot use assignment on this.");
			}

			return this;
		}
	}
}
