using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class BooleanNot : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression Root { get; private set; }

        public BooleanNot(Token bang, Expression root, Node owner)
            : base(bang, owner)
        {
            this.Root = root;
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root }; } }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);

            if (this.Root is BooleanConstant)
            {
                return new BooleanConstant(this.FirstToken, !((BooleanConstant)this.Root).Value, this.Owner);
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveEntityNames(parser);
            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);
            if (this.Root.ResolvedType == ResolvedType.ANY || this.Root.ResolvedType == ResolvedType.BOOLEAN)
            {
                this.ResolvedType = ResolvedType.BOOLEAN;
                return this;
            }
            throw new ParserException(this.FirstToken, "Cannot apply ! to an expression of this type.");
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }
    }
}
