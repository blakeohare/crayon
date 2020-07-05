using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    internal class BooleanConstant : Expression, IConstantValue
    {
        public override bool IsInlineCandidate {  get { return true; } }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        public bool Value { get; private set; }

        public override bool IsLiteral { get { return true; } }

        public BooleanConstant(Token token, bool value, Node owner)
            : base(token, owner)
        {
            this.ResolvedType = ResolvedType.BOOLEAN;
            this.Value = value;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            return this;
        }

        public Expression CloneValue(Token token, Node owner)
        {
            return new BooleanConstant(token, this.Value, owner);
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
