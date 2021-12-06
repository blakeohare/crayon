using Builder.Resolver;
using System.Collections.Generic;

namespace Builder.ParseTree
{
    internal class ThrowStatement : Executable
    {
        public override bool IsTerminator { get { return true; } }

        public Expression Expression { get; set; }
        public Token ThrowToken { get; set; }

        public ThrowStatement(Token throwToken, Expression expression, Node owner) : base(throwToken, owner)
        {
            this.ThrowToken = throwToken;
            this.Expression = expression;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.Expression = this.Expression.Resolve(parser);
            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            this.Expression.ResolveEntityNames(parser);
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.Expression.ResolveVariableOrigins(parser, varIds, phase);
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Expression = this.Expression.ResolveTypes(parser, typeResolver);
            ResolvedType exceptionType = this.Expression.ResolvedType;
            if (exceptionType == parser.TypeContext.ANY)
                return;
            if (exceptionType.Category != ResolvedTypeCategory.INSTANCE)
                throw new ParserException(this.Expression, "Only objects that extend from Core.Exception can be thrown.");

            ClassDefinition objType = exceptionType.ClassTypeOrReference;
            // TODO: check if objType extends from Core.Exception
        }
    }
}
