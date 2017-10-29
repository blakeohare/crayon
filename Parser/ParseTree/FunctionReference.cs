using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    public class FunctionReference : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public FunctionDefinition FunctionDefinition { get; set; }

        public FunctionReference(Token token, FunctionDefinition funcDef, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.FunctionDefinition = funcDef;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            throw new InvalidOperationException(); // Generated in the resolve name phase.
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
