using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class FunctionReference : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public FunctionDefinition FunctionDefinition { get; set; }

        public FunctionReference(Token token, FunctionDefinition funcDef, Executable owner)
            : base(token, owner)
        {
            this.FunctionDefinition = funcDef;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException(); // Generated in the resolve name phase.
        }
        
        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
