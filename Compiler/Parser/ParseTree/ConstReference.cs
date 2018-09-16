using System;

namespace Parser.ParseTree
{
    public class ConstReference : Expression
    {
        public override bool CanAssignTo { get { return false; } }

        public ConstDefinition ConstStatement { get; private set; }

        public ConstReference(Token token, ConstDefinition con, Node owner)
            : base(token, owner)
        {
            this.ConstStatement = con;
        }

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

        internal override void ResolveTypes(ParserContext parser)
        {
            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase) { }
    }
}
