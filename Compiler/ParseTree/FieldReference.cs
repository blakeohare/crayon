using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class FieldReference : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return true; } }

        public FieldDeclaration Field { get; set; }

        public FieldReference(Token token, FieldDeclaration field, Executable owner)
            : base(token, owner)
        {
            this.Field = field;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            throw new InvalidOperationException(); // created in the resolve name phase.
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
