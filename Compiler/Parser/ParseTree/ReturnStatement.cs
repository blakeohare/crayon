using Parser.Resolver;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class ReturnStatement : Executable
    {
        public Expression Expression { get; private set; }

        public ReturnStatement(Token returnToken, Expression nullableExpression, Node owner)
            : base(returnToken, owner)
        {
            this.Expression = nullableExpression;
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.Resolve(parser);
            }
            return Listify(this);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveEntityNames(parser);
            }
            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            ResolvedType returnType;
            FunctionDefinition fd = this.Owner as FunctionDefinition;

            if (fd != null)
            {
                returnType = fd.ResolvedReturnType;
            }
            else if (this.Owner is ConstructorDefinition)
            {
                returnType = ResolvedType.VOID;
            }
            else
            {
                throw new System.Exception();
            }

            if (this.Expression == null)
            {
                if (returnType != ResolvedType.VOID)
                {
                    throw new ParserException(this, "Must return a value from a function. Empty return statements are not allowed.");
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            else
            {
                this.Expression.ResolveTypes(parser, typeResolver);

                if (!this.Expression.ResolvedType.CanAssignToA(returnType))
                {
                    throw new ParserException(this, "Cannot return this type from this function.");
                }
            }
        }

        public override bool IsTerminator { get { return true; } }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if (this.Expression != null)
            {
                this.Expression.PerformLocalIdAllocation(parser, varIds, phase);
            }
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            throw new System.NotImplementedException();
        }
    }
}
