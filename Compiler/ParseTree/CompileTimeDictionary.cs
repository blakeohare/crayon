using System.Collections.Generic;

namespace Crayon.ParseTree
{
    class CompileTimeDictionary : Expression
    {
        internal override Expression PastelResolve(Parser parser)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanAssignTo { get { return false; } }

        public string Type { get; private set; }

        public CompileTimeDictionary(Token firstToken, string type, TopLevelConstruct owner)
            : base(firstToken, owner)
        {
            this.Type = type;
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, TopLevelConstruct> lookup, string[] imports)
        {
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(Parser parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
