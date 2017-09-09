using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class LibraryFunctionReference : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public string Name { get; private set; }

        public LibraryFunctionReference(Token token, string name, TopLevelConstruct owner)
            : base(token, owner)
        {
            this.Name = name;
        }

        internal override Expression Resolve(Parser parser)
        {
            throw new ParserException(this.FirstToken, "Library functions cannot be passed around as references. They can only be invoked.");
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            throw new InvalidOperationException(); // Created during resolve name phase.
        }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            throw new NotImplementedException();
        }
    }
}
