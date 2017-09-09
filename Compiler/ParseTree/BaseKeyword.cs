using System;
using System.Collections.Generic;

namespace Crayon.ParseTree
{
    internal class BaseKeyword : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public BaseKeyword(Token token, TopLevelConstruct owner)
            : base(token, owner)
        {
        }

        internal override Expression Resolve(Parser parser)
        {
            throw new ParserException(this.FirstToken, "'base' keyword can only be used as part of a method reference.");
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }

        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
