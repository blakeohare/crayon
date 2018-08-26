using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class Assignment : Executable
    {
        public Expression Target { get; private set; }
        public Expression Value { get; private set; }
        public Token AssignmentOpToken { get; private set; }
        public string AssignmentOp { get; private set; }
        public Variable TargetAsVariable { get { return this.Target as Variable; } }

        public Assignment(Expression target, Token assignmentOpToken, string assignmentOp, Expression assignedValue, TopLevelConstruct owner)
            : base(target.FirstToken, owner)
        {
            this.Target = target;
            this.AssignmentOpToken = assignmentOpToken;
            this.AssignmentOp = assignmentOp;
            this.Value = assignedValue;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
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

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Value.PerformLocalIdAllocation(parser, varIds, phase);

            if ((phase & VariableIdAllocPhase.REGISTER) != 0)
            {
                bool isVariableDeclared =
                    // A variable is considered declared if the target is a variable and = is used instead of something like +=
                    this.Target is Variable &&
                    this.AssignmentOpToken.Value == "=";

                if (isVariableDeclared)
                {
                    varIds.RegisterVariable(this.TargetAsVariable.Name);
                }
            }

            this.Target.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Target = this.Target.ResolveEntityNames(parser);
            this.Value = this.Value.ResolveEntityNames(parser);

            if (!this.Target.CanAssignTo)
            {
                throw new ParserException(this.Target, "Cannot use assignment on this.");
            }

            return this;
        }
    }
}
