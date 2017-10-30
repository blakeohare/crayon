using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class EnumReference : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public EnumDefinition EnumDefinition { get; set; }

        public EnumReference(Token token, EnumDefinition enumDefinition, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.EnumDefinition = enumDefinition;
        }

        public override bool CanAssignTo { get { return false; } }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
