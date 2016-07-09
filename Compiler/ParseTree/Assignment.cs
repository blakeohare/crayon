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

        internal override void GenerateGlobalNameIdManifest(VariableIdAllocator varIds)
        {
            if (this.Target is Variable)
            {
                varIds.RegisterVariable(((Variable)this.Target).Name);
            }
        }

        internal override void CalculateLocalIdPass(VariableIdAllocator varIds)
        {
            Variable variable = this.TargetAsVariable;

            // Note that things like += do not declare a new variable name and so they don't count as assignment
            // in this context. foo += value should NEVER take a global scope value and assign it to a local scope value.
            // Globals cannot be assigned to from outside the global scope.
            if (variable != null &&
                this.AssignmentOpToken.Value == "=")
            {
                varIds.RegisterVariable(variable.Name);
            }
        }

        internal override void SetLocalIdPass(VariableIdAllocator varIds)
        {
            this.Value.SetLocalIdPass(varIds);
            this.Target.SetLocalIdPass(varIds);
        }

        internal override Executable ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Target = this.Target.ResolveNames(parser, lookup, imports);
            this.Value = this.Value.ResolveNames(parser, lookup, imports);

            if (!this.Target.CanAssignTo)
            {
                throw new ParserException(this.Target.FirstToken, "Cannot use assignment on this.");
            }

            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Target.GetAllVariablesReferenced(vars);
            this.Value.GetAllVariablesReferenced(vars);
        }
    }
}
