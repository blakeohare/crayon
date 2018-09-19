using Parser.Resolver;

namespace Parser.ParseTree
{
    public class NegativeSign : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public Expression Root { get; private set; }

        public NegativeSign(Token sign, Expression root, Node owner)
            : base(sign, owner)
        {
            this.Root = root;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);
            if (this.Root is IntegerConstant)
            {
                return new IntegerConstant(this.FirstToken, ((IntegerConstant)this.Root).Value * -1, this.Owner);
            }

            if (this.Root is FloatConstant)
            {
                return new FloatConstant(this.FirstToken, ((FloatConstant)this.Root).Value * -1, this.Owner);
            }

            return this;
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);
            switch (this.Root.ResolvedType.Category)
            {
                case ResolvedTypeCategory.ANY:
                case ResolvedTypeCategory.FLOAT:
                case ResolvedTypeCategory.INTEGER:
                    // this is fine.
                    break;
                default:
                    throw new ParserException(this.FirstToken, "Cannot apply a negative sign to this type.");
            }
            this.ResolvedType = this.Root.ResolvedType;
            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Root.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveEntityNames(parser);
            return this;
        }
    }
}
