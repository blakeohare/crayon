using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BooleanNot : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
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

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);

            if (this.Root is BooleanConstant)
            {
                return new BooleanConstant(this.FirstToken, !((BooleanConstant)this.Root).Value, this.Owner);
            }

            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Root.GetAllVariablesReferenced(vars);
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
