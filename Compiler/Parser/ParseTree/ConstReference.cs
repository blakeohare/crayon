using Parser.Resolver;
using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ConstReference : Expression
    {
        public ConstDefinition ConstStatement { get; private set; }

        public ConstReference(Token token, ConstDefinition con, Node owner)
            : base(token, owner)
        {
            this.ConstStatement = con;
        }

        internal override IEnumerable<Expression> Descendants { get { return Expression.NO_DESCENDANTS; } }

        internal override Expression Resolve(ParserContext parser)
        {
            if (parser.ConstantAndEnumResolutionState[this.ConstStatement] != ConstantResolutionState.RESOLVED)
            {
                this.ConstStatement.Resolve(parser);
            }

            IConstantValue value = this.ConstStatement.Expression as IConstantValue;
            if (value == null)
            {
                throw new ParserException(this.ConstStatement, "Could not resolve this expression into a constant value.");
            }
            return value.CloneValue(this.FirstToken, this.Owner);
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.ResolvedType = this.ConstStatement.ResolvedType;
            return this;
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
