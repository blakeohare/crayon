using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BooleanNot : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            this.Root = this.Root.PastelResolve(parser);
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression Root { get; private set; }

        public BooleanNot(Token bang, Expression root, TopLevelConstruct owner)
            : base(bang, owner)
        {
            this.Root = root;
        }

        internal override Expression Resolve(Parser parser)
        {
            this.Root = this.Root.Resolve(parser);

            if (this.Root is BooleanConstant)
            {
                return new BooleanConstant(this.FirstToken, !((BooleanConstant)this.Root).Value, this.Owner);
            }

            return this;
        }

        internal override Expression ResolveNames(Parser parser, System.Collections.Generic.Dictionary<string, Executable> lookup, string[] imports)
        {
            this.Root = this.Root.ResolveNames(parser, lookup, imports);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Root.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
